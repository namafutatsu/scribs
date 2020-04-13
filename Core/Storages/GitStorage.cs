﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Core.Storages {
    public class GitStorage: ILocalStorage {
        private const string directoryDocument = ".dir.md";
        private SystemService system;
        private GitHubService gitHubService;
        public RepositoryService repositoryService;
        public string Root { get; }

        public GitStorage(GitStorageSettings settings, GitHubService gitHubService, RepositoryService repositoryService, SystemService system) {
            this.system = system;
            if (settings.Local)
                Root = settings.Root;
            this.gitHubService = gitHubService;
            this.repositoryService = repositoryService;
        }

        public Document Load(string userName, string name) => Load(userName, name, true);
        public void Save(Document project) => Save(project, null);

        public void Save(Document project, string message = null) {
            var path = system.PathCombine(Root, project.User.Path, project.Name);
            if (project.Disconnect) {
                if (!system.NodeExists(path))
                    system.CreateNode(path);
            } else {
                if (!repositoryService.IsRepo(path))
                    gitHubService.Create(project);
                if (system.NodeExists(path))
                    repositoryService.Pull(path);
                else
                    repositoryService.Clone(gitHubService.GetRepoName(project), path);
            }
            EmptyProject(path);
            SaveDirectory(project, system.PathCombine(Root, project.Path), true);
            if (message == null)
                message = DateTime.Now.ToString();
            if (!project.Disconnect)
                repositoryService.Commit(path, message);
        }

        private void EmptyProject(string path) {
            var gitPath = system.PathCombine(path, ".git");
            foreach (var file in system.GetLeaves(path))
                system.DeleteLeaf(file);
            foreach (var directory in system.GetNodes(path).Where(o => o != gitPath))
                system.DeleteNode(directory, true);
        }

        private string GetIndexPrefix(Document document) {
            string prefix = document.Index.ToString();
            return prefix.PadLeft(2, '0') + ".";
        }

        private void SaveDirectory(Document directory, string path, bool content) {
            if (!system.NodeExists(path))
                system.CreateNode(system.PathCombine(Root, path));
            if (directory.Metadata.Any(o => o.Value != null) || directory.Text != null)
                WriteDocument(directory, system.PathCombine(Root, path, directoryDocument), content);
            if (directory.Documents == null)
                return;
            foreach (var document in directory.Documents)
                if (!document.IsLeaf) {
                    SaveDirectory(document, system.PathCombine(path, (directory.IndexNodes.HasValue && directory.IndexNodes.Value ? GetIndexPrefix(document) : String.Empty) + document.Name), content);
                } else {
                    SaveFile(document, path, content, directory.IndexLeaves ?? true);
                }
        }

        private void SaveFile(Document file, string path, bool content, bool saveIndex) {
            WriteDocument(file, system.PathCombine(path, (saveIndex ? GetIndexPrefix(file) : String.Empty) + file.Name + ".md"), content);
        }

        private void WriteDocument(Document document, string path, bool content) {
            var metadataLines = new List<string>();
            foreach (var metadata in document.Metadata) {
                //if (metadata.Key == "id" && document.Text == null)
                //    continue;
                metadataLines.Add($"{metadata.Key}: {metadata.Value}");
            }
            if (metadataLines.Any() || (content && document.Text != null)) {
                var builder = new StringBuilder();
                if (metadataLines.Any()) {
                    builder.AppendLine("---");
                    foreach (var line in metadataLines)
                        builder.AppendLine(line);
                    builder.AppendLine("---");
                }
                if (content && document.Text != null)
                    builder.Append(document.Text);
                system.WriteLeaf(path, builder);
            }
        }

        public Document Load(string userName, string name, bool content = true) {
            var user = User.GetByName(userName);
            string path = system.PathCombine(Root, user.Path, name);
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

        private Document LoadDirectory(User user, Document parent, string path, bool content) {
            if (!system.NodeExists(path))
                system.CreateNode(path);
            var directory = LoadDocument(user, parent, path, content, false, parent?.IndexNodes ?? false);
            var documents = new List<Document>();
            foreach (var subdirectory in system.GetNodes(path, "*", SearchOption.TopDirectoryOnly)) {
                if (system.GetName(subdirectory).StartsWith("."))// == ".git")
                    continue;
                documents.Add(LoadDirectory(user, directory, subdirectory, content));
            }
            foreach (var file in system.GetLeaves(path).Where(o => o.EndsWith(".md") && !system.GetName(o).StartsWith(".")))
                documents.Add(LoadFile(user, directory, file, content));
            directory.Documents = new ObservableCollection<Document>(
                documents.OrderBy(o => o.IsLeaf).ThenBy(o => o.Index).ThenBy(o => o.Name));
            return directory;
        }

        private Document LoadDocument(User user, Document parent, string path, bool content, bool isLeaf, bool loadIndex) {
            var name = system.GetName(path);
            if (isLeaf)
                name = name.Substring(0, name.LastIndexOf("."));
            int index = 0;
            if (loadIndex) {
                var sections = name.Split('.');
                if (!int.TryParse(sections.First(), out index))
                    index = 0;
                else if (sections.Length > 1)
                    name = sections.Skip(1).Aggregate((a, b) => a + "." + b);
            }
            var document = new Document(name, user, parent, Utils.CreateGuid());
            document.Index = index;
            if (isLeaf) {
                ReadMetadata(document, path);
                if (content)
                    ReadDocument(document, path);
            } else {
                var hidden = system.GetLeaves(path, directoryDocument).SingleOrDefault();
                if (hidden != null) {
                    ReadMetadata(document, hidden);
                    if (content)
                        ReadDocument(document, hidden);
                }
            }
            return document;
        }

        private Document LoadFile(User user, Document parent, string path, bool content) {
            return LoadDocument(user, parent, path, content, true, parent.IndexLeaves ?? true);
        }

        private void ReadMetadata(Document document, string path) {
            using (var reader = system.ReadLeaf(path)) {
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
                            case "index.nodes":
                                document.IndexNodes = bool.Parse(value);
                                break;
                            case "index.leaves":
                                document.IndexLeaves = bool.Parse(value);
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
            using (var reader = system.ReadLeaf(path)) {
                if (reader.ReadLine().StartsWith("---")) {
                    string line = reader.ReadLine();
                    while (!line.StartsWith("---"))
                        line = reader.ReadLine();
                } else {
                    reader.Reset();
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