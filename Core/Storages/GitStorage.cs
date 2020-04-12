using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Core.Storages {
    public class GitStorage: ILocalStorage {
        private const string directoryDocument = ".dir.md";
        private GitHubService gitHubService;
        public RepositoryService repositoryService;
        public string Root { get; }

        public GitStorage(GitStorageSettings settings, GitHubService gitHubService, RepositoryService repositoryService) {
            if (settings.Local)
                Root = settings.Root;
            this.gitHubService = gitHubService;
            this.repositoryService = repositoryService;
        }

        public Document Load(string userName, string name) => Load(userName, name, true);
        public void Save(Document project) => Save(project, null);

        public void Save(Document project, string message = null) {
            var path = Path.Combine(Root, project.User.Path, project.Name);
            if (!repositoryService.IsRepo(path))
                gitHubService.Create(project);
            if (Directory.Exists(path))
                repositoryService.Pull(path);
            else
                repositoryService.Clone(gitHubService.GetRepoName(project), path);
            EmptyProject(path);
            SaveDirectory(project, true);
            if (message == null)
                message = DateTime.Now.ToString();
            repositoryService.Commit(path, message);
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
            foreach (var document in parent.Documents)
                if (!document.IsLeaf)
                    SaveDirectory(document, content);
                else
                    SaveFile(document, content);
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

        public Document Load(string userName, string name, bool content = true) {
            var user = User.GetByName(userName);
            string path = Path.Combine(Root, user.Path, name);
            repositoryService.Pull(path);
            var project = LoadDirectory(user, null, path, content);
            project.Repo = repositoryService.Url(path);
            return project;
        }

        private Document LoadDirectory(User user, Document parent, string path, bool content) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var directory = LoadDocument(user, parent, path, content, false);
            var documents = new List<Document>();
            foreach (var subdirectory in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)) {
                if (Path.GetFileName(subdirectory) == ".git")
                    continue;
                documents.Add(LoadDirectory(user, directory, subdirectory, content));
            }
            foreach (var file in Directory.GetFiles(path).Where(o => o.EndsWith(".md")))
                documents.Add(LoadFile(user, directory, file, content));
            directory.Documents = new ObservableCollection<Document>(
                documents.OrderBy(o => o.IsLeaf).ThenBy(o => o.Index).ThenBy(o => o.Name));
            return directory;
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
            var document = new Document(name, user, parent, Utils.CreateGuid());
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
                            case "repo":
                                document.Repo = value;
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

    public class GitStorageSettings : IStorageSettings {
        public virtual bool Local { get; set; }
        public virtual string Root { get; set; }
    }
}