using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Scribs.Core {
    public abstract class Storage {
        public abstract Document Load(string userName, string name, bool content = false);
        public abstract void Save(Document project, bool content = false);
    }

    public abstract class ServerStorage : Storage {
        protected string Root { get; private set; }
        public ServerStorage(string root) {
            Root = root;
        }
    }

    public class JsonStorage : ServerStorage {
        public JsonStorage(string root) : base(root) {
        }

        public override Document Load(string userName, string name, bool content = false) {
            throw new NotImplementedException();
        }

        public override void Save(Document project, bool content = false) {
            throw new NotImplementedException();
        }
    }

    public class DiskStorage : ServerStorage {
        public DiskStorage(string root) : base(root) {
        }

        public override void Save(Document project, bool content = false) {
            var path = Path.Combine(Root, project.User.Path, project.Name);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            SaveDirectory(project, content);
        }

        private void SaveDirectory(Document parent, bool content) {
            Directory.CreateDirectory(Path.Combine(Root, parent.Path));
            if (parent.Metadata.Any(o => o.Value != null) || parent.Text != null)
                WriteDocument(parent, Path.Combine(Root, parent.Path, ".document"), content);
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
            WriteDocument(document, Path.Combine(Root, document.Path), content);
        }

        private void WriteDocument(Document document, string path, bool content) {
            using (StreamWriter sw = File.CreateText(path)) {
                sw.WriteLine("---");
                foreach (var metadata in document.Metadata)
                    sw.WriteLine($"{metadata.Key}: {metadata.Value}");
                sw.WriteLine("---");
                if (content)
                    sw.Write(document.Text);
            }
        }

        public override Document Load(string userName, string name, bool content = false) {
            var user = new User(userName);
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
            if (content) {
                if (isLeaf) {
                    ReadDocument(document, path);
                } else {
                    var hidden = Directory.GetFiles(path, ".document").SingleOrDefault();
                    if (hidden != null)
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

        private void ReadDocument(Document document, string path) {
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
                } else {
                    reader.DiscardBufferedData();
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                }
                document.Text = reader.ReadToEnd();
            }
        }
    }
}
