using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Core.Storages {
    public class GitStorage: ILocalStorage {
        private SystemService System { get; }
        private GitHubService gitHubService;
        public RepositoryService repositoryService;
        public string Root { get; }
        public static string DirectoryDocumentName => ".dir.md";

        public GitStorage(GitStorageSettings settings, GitHubService gitHubService, RepositoryService repositoryService, SystemService system) {
            System = system;
            if (settings.Local)
                Root = settings.Root;
            this.gitHubService = gitHubService;
            this.repositoryService = repositoryService;
        }

        public Document Load(string userName, string name) => Load(userName, name, true);
        public void Save(Document project) => Save(project, null);

        public void Save(Document project, string message = null) {
            var path = Path.Combine(Root, project.User.Path, project.Name);
            if (project.Disconnect) {
                if (!System.NodeExists(path))
                    System.CreateNode(path);
            } else {
                if (!repositoryService.IsRepo(path))
                    gitHubService.Create(project);
                if (System.NodeExists(path))
                    repositoryService.Pull(path);
                else
                    repositoryService.Clone(gitHubService.GetRepoName(project), path);
            }
            EmptyProject(path);
            SaveDirectory(project, Path.Combine(Root, project.Path), true);
            if (message == null)
                message = DateTime.Now.ToString();
            if (!project.Disconnect)
                repositoryService.Commit(path, message);
        }

        public void EmptyProject(string path) {
            var gitPath = Path.Combine(path, ".git");
            foreach (var file in System.GetLeaves(path))
                System.DeleteLeaf(file);
            foreach (var directory in System.GetNodes(path).Where(o => o != gitPath))
                System.DeleteNode(directory, true);
        }

        public string GetIndexPrefix(Document document) {
            string prefix = document.Index.ToString();
            return prefix.PadLeft(2, '0') + ".";
        }

        public string GetDocumentPath(Document parent, Document document, string path) {
            string name = document.Name;
            if (parent != null) {
                if (document.IsLeaf && parent.IndexLeaves) {
                    name = GetIndexPrefix(document) + name;
                } else if (!document.IsLeaf && parent.IndexNodes) {
                    name = GetIndexPrefix(document) + name;
                }
            }
            if (document.IsLeaf)
                name += ".md";
            return Path.Join(path, name);
        }

        public bool NeedsDirectoryDocument(Document directory) => Document.Metadatas.Count(o => o.Get(directory) != null) > 1 || directory.Content != null;

        private void SaveDirectory(Document directory, string path, bool content) {
            if (!System.NodeExists(path))
                System.CreateNode(path);
            if (NeedsDirectoryDocument(directory))
                WriteDocument(directory, Path.Combine(path, DirectoryDocumentName), content);
            if (directory.IsLeaf)
                return;
            foreach (var child in directory.Children) {
                if (!child.IsLeaf)
                    SaveDirectory(child, GetDocumentPath(directory, child, path), content);
                else
                    SaveFile(directory, child, path, content);
            }
        }

        private void SaveFile(Document parent, Document file, string path, bool content) {
            WriteDocument(file, GetDocumentPath(parent, file, path), content);
        }

        public void WriteDocument(Document document, string path, bool content) {
            var metadataLines = new List<string>();
            foreach (var metadata in Document.Metadatas) {
                var value = metadata.Get(document);
                if (value != null && value != metadata.Default())
                    metadataLines.Add($"{metadata.Id}: {value}");
            }
            if (metadataLines.Count > 1 || (content && document.Content != null)) {
                var builder = new StringBuilder();
                if (metadataLines.Any()) {
                    builder.AppendLine("---");
                    foreach (var line in metadataLines)
                        builder.AppendLine(line);
                    builder.AppendLine("---");
                }
                if (content && document.Content != null)
                    builder.Append(document.Content);
                System.WriteLeaf(path, builder.ToString());
            }
        }

        public Document Load(string userName, string name, bool content = true) {
            var user = new User(userName);
            string path = Path.Combine(Root, user.Path, name);
            bool disconnect = false;
            try {
                repositoryService.Pull(path);
            } catch {
                disconnect = true;
            }
            var project = LoadDirectory(user, null, path, content);
            project.Disconnect = disconnect;
            if (!project.Disconnect)
                project.Repo = repositoryService.Url(path);
            return project;
        }

        public IEnumerable<string> GetNodes(string path) => System.GetNodes(path, "*", SearchOption.TopDirectoryOnly).Where(o => !System.GetName(o).StartsWith("."));

        public IEnumerable<string> GetLeaves(string path) => System.GetLeaves(path).Where(o => o.EndsWith(".md") && !System.GetName(o).StartsWith("."));

        public ObservableCollection<Document> OrderDocuments(IEnumerable<Document> documents) =>
            new ObservableCollection<Document>(documents.OrderBy(o => o.Index).ThenBy(o => o.IsLeaf).ThenBy(o => o.Name));

        public virtual ObservableCollection<Document> GetChildren(Document directory, User user, string path, bool content) {
            var documents = new List<Document>();
            foreach (var subdirectory in GetNodes(path))
                documents.Add(LoadDirectory(user, directory, subdirectory, content));
            foreach (var file in GetLeaves(path))
                documents.Add(LoadFile(user, directory, file, content));
            return OrderDocuments(documents);
        }

        public string GetDocumentName(string path, bool isLeaf, bool loadIndex, out int index) {
            var name = System.GetName(path);
            if (isLeaf)
                name = name.Substring(0, name.LastIndexOf("."));
            index = 0;
            if (loadIndex) {
                var sections = name.Split('.');
                if (!int.TryParse(sections.First(), out index))
                    index = 0;
                else if (sections.Length > 1)
                    name = sections.Skip(1).Aggregate((a, b) => a + "." + b);
            }
            return name;
        }

        public Document GetDocument(User user, Document parent, string path, bool isLeaf, bool loadIndex) {
            string name = GetDocumentName(path, isLeaf, loadIndex, out int index);
            var document = new Document(name, user, parent, Utils.CreateId());
            document.Index = index;
            return document;
        }

        public Document LoadDirectory(User user, Document parent, string path, bool content) {
            if (!System.NodeExists(path))
                System.CreateNode(path);
            var directory = GetDocument(user, parent, path, false, parent?.IndexNodes ?? false);
            var directoryDocument = System.GetLeaves(path, DirectoryDocumentName).SingleOrDefault();
            if (directoryDocument != null) {
                ReadMetadata(directory, directoryDocument);
                if (content)
                    ReadDocument(directory, directoryDocument);
            } else {
                SetMetadata(directory);
            }
            directory.Children = GetChildren(directory, user, path, content);
            return directory;
        }

        public Document LoadFile(User user, Document parent, string path, bool content) {
            var document = GetDocument(user, parent, path, true, parent.IndexLeaves);
            ReadMetadata(document, path);
            if (content)
                ReadDocument(document, path);
            return document;
        }

        public void ReadMetadata(Document document, string path) {
            using (var reader = System.ReadLeaf(path)) {
                if (reader.ReadLine().StartsWith("---")) {
                    string line = reader.ReadLine();
                    var metadatas = new Dictionary<string, string>();
                    while (!line.StartsWith("---")) {
                        if (!line.Contains(':'))
                            continue;
                        int index = line.IndexOf(':');
                        string key = line.Substring(0, index).Trim().ToLower();
                        string value = line.Substring(index + 1).Trim();
                        metadatas.Add(key, value);
                        line = reader.ReadLine();
                    }
                    SetMetadata(document, metadatas);
                }
            }
        }

        public void SetMetadata(Document document, Dictionary<string, string> metadatas = null) {
            foreach (var metadata in Document.Metadatas) {
                if (metadatas == null || !metadatas.ContainsKey(metadata.Id)) {
                    //if (metadata.Get(document) != null)
                        //metadata.Set(document, metadata.Default());
                } else {
                    metadata.Set(document, metadatas[metadata.Id]);
                }
            }
        }

        public void ReadDocument(Document document, string path) {
            using (var reader = System.ReadLeaf(path)) {
                if (reader.ReadLine().StartsWith("---")) {
                    string line = reader.ReadLine();
                    while (!line.StartsWith("---"))
                        line = reader.ReadLine();
                } else {
                    reader.Reset();
                }
                var text = reader.ReadToEnd();
                if (!String.IsNullOrEmpty(text))
                    document.Content = text;
            }
        }
    }

    public class GitStorageSettings : IStorageSettings {
        public virtual bool Local { get; set; }
        public virtual string Root { get; set; }
    }
}