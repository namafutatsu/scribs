using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Scribs.Core {
    public abstract class Storage {
        public abstract Document Load(string userName, string name, bool content = false);
        //public abstract void Save(Document project, bool content = false);
    }

    public abstract class ServerStorage : Storage {
        protected string Root { get; private set; }
        public ServerStorage(string root) {
            Root = root;
        }
    }

    public class JsonStorage : ServerStorage {
        public const string jsonDocument = ".json";

        public JsonStorage(string root) : base(root) {
        }

        public object JsonConvert { get; private set; }

        public override Document Load(string userName, string name, bool content = false) {
            var user = User.GetByName(userName);
            string path = Path.Combine(Root, user.Path, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var project = ReadJson(Path.Combine(path, jsonDocument));
            Document.BuildProject(project, user);
            if (content)
                foreach (var document in project.AllDocuments.Values)
                    ReadDocument(path, document);
            return project;
        }

        private Document ReadJson(string path) {
            Document project;
            using (StreamReader reader = new StreamReader(path)) {
                var text = reader.ReadToEnd();
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(text))) {
                    var deserializer = new DataContractJsonSerializer(typeof(Document));
                    project = (Document)deserializer.ReadObject(ms);
                }
            }
            return project;
        }

        private void ReadDocument(string path, Document document) {
            if (!File.Exists(Path.Combine(path, document.Key)))
                return;
            using (StreamReader reader = new StreamReader(Path.Combine(path, document.Key)))
                document.Text = reader.ReadToEnd();
        }

        public void Save(Document project, bool content = false) {
            string path = Path.Combine(Root, project.User.Path, project.Name);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
            CreateJson(project, Path.Combine(path, jsonDocument));
            if (content)
                SaveDocument(project, path);
        }

        private void SaveDocument(Document document, string path) {
            if (document.Text != null)
                WriteDocument(document, Path.Combine(path, document.Key));
            if (document.Documents == null)
                return;
            foreach (var child in document.Documents)
                SaveDocument(child, path);
        }

        private void WriteDocument(Document document, string path) {
            using (StreamWriter sw = File.CreateText(path))
                sw.Write(document.Text);
        }

        private void CreateJson(Document project, string path) {
            var serializer = new DataContractJsonSerializer(typeof(Document));
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, project);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                using (StreamWriter sw = File.CreateText(path)) {
                    sw.Write(json);
                }
            }
        }
    }

    public class DiskStorage : ServerStorage {
        private const string directoryDocument = ".dir.md";
        private static LibGit2Sharp.Signature Signature => new LibGit2Sharp.Signature(new Identity("System", "system@scribs.io"), DateTimeOffset.Now);
        private UsernamePasswordCredentials credentials;
        private GitHubClient client;
        private Octokit.User user;

        public DiskStorage(string root) : base(root) {
        }

        public void Save(Document project, string message = null) {
            string repoName = $"scribs_{project.User.Name}_{project.Name}";
            Octokit.Repository repo;
            try {
                repo = client.Repository.Get(credentials.Username, repoName).Result;
            } catch {
                repo = client.Repository.Create(new NewRepository(repoName) { Private = true, AutoInit = true }).Result;
            }
            var path = Path.Combine(Root, project.User.Path, project.Name);
            if (Directory.Exists(path)) {
                gitPull(path);
            } else {
                gitClone(repoName, path);
            }
            EmptyProject(path);
            SaveDirectory(project, true);
            if (message == null)
                message = DateTime.Now.ToString();
            gitCommit(path, message);
        }

        private void gitCommit(string path, string message) {
            using (var repo = new LibGit2Sharp.Repository(path)) {
                var changes = repo.Diff.Compare<TreeChanges>();
                if (changes.Any()) {
                    Commands.Stage(repo, "*");
                    repo.Commit(message, Signature, Signature);
                    repo.Network.Push(repo.Branches["master"], new LibGit2Sharp.PushOptions {
                        CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => credentials)
                    });
                }
            }
        }

        public void SetCredentials(string username, string password) {
            var basicAuth = new Octokit.Credentials(username, password);
            client = new GitHubClient(new ProductHeaderValue("scribs"));
            client.Credentials = basicAuth;
            user = client.User.Get(username).Result;
            credentials = new UsernamePasswordCredentials() {
                Username = username,
                Password = password
            };
        }

        private void gitClone(string repoName, string path) {
            LibGit2Sharp.Repository.Clone("https://github.com/scribssys/" + repoName, path, new CloneOptions {
                CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => credentials)
            });
        }

        private void gitPull(string path) {
            using (var repo = new LibGit2Sharp.Repository(path)) {
                Commands.Pull(repo, Signature, new PullOptions {
                    FetchOptions = new FetchOptions {
                        CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => credentials)
                    }
                });
            }
        }

        private void EmptyProject(string path) {
            var gitPath = Path.Combine(path, ".git");
            foreach (var file in Directory.GetFiles(path))
                File.Delete(file);
            foreach (var directory in Directory.GetDirectories(path).Where(o => o != gitPath))
                Directory.Delete(directory, true);
        }

        private void SaveDirectory(Document parent, bool content) {
            if (!Directory.Exists(Path.Combine(Root, parent.Path)))
                Directory.CreateDirectory(Path.Combine(Root, parent.Path));
            if (parent.Metadata.Any(o => o.Value != null) || parent.Text != null)
                WriteDocument(parent, Path.Combine(Root, parent.Path, directoryDocument), content);
            if (parent.Documents == null)
                return;
            foreach (var document in parent.Documents) {
                if (!document.IsLeaf)
                    SaveDirectory(document, content);
                else
                    SaveFile(document, content);
            }
        }

        private void SaveFile(Document document, bool content) {
            WriteDocument(document, Path.Combine(Root, document.Path + ".md"), content);
        }

        private void WriteDocument(Document document, string path, bool content) {
            var metadataLines = new List<string>();
            foreach (var metadata in document.Metadata) {
                if (metadata.Key == "id" && document.Text == null)
                    continue;
                metadataLines.Add($"{metadata.Key}: {metadata.Value}");
            }
            if (metadataLines.Any() || (content && document.Text != null)) {
                using (StreamWriter sw = File.CreateText(path)) {
                    if (metadataLines.Any()) {
                        sw.WriteLine("---");
                        foreach (var line in metadataLines)
                            sw.WriteLine(line);
                        sw.WriteLine("---");
                    }
                    if (content && document.Text != null)
                        sw.Write(document.Text);
                }
            }
        }

        public override Document Load(string userName, string name, bool content = false) {
            var user = User.GetByName(userName);
            return LoadDirectory(user, null, Path.Combine(Root, user.Path, name), content);
        }

        private Document LoadDocument(User user, Document parent, string path, bool content, bool isLeaf) {
            var name = Path.GetFileName(path);
            if (isLeaf)
                name = name.Substring(0, name.LastIndexOf("."));
            var sections = name.Split('.');
            if (!int.TryParse(sections.First(), out int index))
                index = 0;
            else if (sections.Length > 1)
                name = sections.Skip(1).Aggregate((a, b) => a + "." + b);
            var document = new Document(Utils.CreateGuid(), name, user, parent);
            document.Index = index;
            if (isLeaf) {
                ReadMetadata(document, path);
                if (content)
                    ReadDocument(document, path);
            } else {
                var hidden = Directory.GetFiles(path, directoryDocument).SingleOrDefault();
                if (hidden != null) {
                    ReadMetadata(document, hidden);
                    if (content)
                        ReadDocument(document, hidden);
                }
            }
            return document;
        }

        private Document LoadDirectory(User user, Document parent, string path, bool content) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var directory = LoadDocument(user, parent, path, content, false);
            var documents = new List<Document>();
            foreach (var subdirectory in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
                documents.Add(LoadDirectory(user, directory, subdirectory, content));
            foreach (var file in Directory.GetFiles(path).Where(o => o.EndsWith(".md")))
                documents.Add(LoadFile(user, directory, file, content));
            directory.Documents = new ObservableCollection<Document>(
                documents.OrderBy(o => o.IsLeaf).ThenBy(o => o.Index).ThenBy(o => o.Name));
            return directory;
        }

        private Document LoadFile(User user, Document parent, string path, bool content) {
            return LoadDocument(user, parent, path, content, true);
        }

        private void ReadMetadata(Document document, string path) {
            using (StreamReader reader = new StreamReader(path)) {
                if (reader.ReadLine().StartsWith("---")) {
                    string line = reader.ReadLine();
                    while (!line.StartsWith("---")) {
                        var metadata = line.Split(":").Select(o => o.Trim());
                        string key = metadata.First().ToLower();
                        string value = metadata.Last();
                        switch (key) {
                            case "id":
                                document.Key = value;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        line = reader.ReadLine();
                    }
                }
            }
        }

        private void ReadDocument(Document document, string path) {
            using (StreamReader reader = new StreamReader(path)) {
                if (reader.ReadLine().StartsWith("---")) {
                    string line = reader.ReadLine();
                    while (!line.StartsWith("---"))
                        line = reader.ReadLine();
                } else {
                    reader.DiscardBufferedData();
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                }
                var text = reader.ReadToEnd();
                if (!String.IsNullOrEmpty(text))
                    document.Text = text;
            }
        }
    }
}
