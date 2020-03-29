using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Scribs.Core {
    public abstract class Storage {
        public abstract Project Load(string userName, string key, bool content = false);
        public abstract Project Save(bool content = false);
        public abstract void CreateFile(File file);
        public abstract void MoveFile(File file, Directory parent);
        public abstract void RenameFile(File file, string key);
        public abstract void DeleteFile(File file);
        public abstract void CreateDirectory(Directory directory);
        public abstract void MoveDirectory(Directory directory, Directory parent);
        public abstract void RenameDirectory(Directory directory, string key);
        public abstract void DeleteDirectory(Directory directory);
        public abstract void Submit();
    }

    public abstract class ServerStorage : Storage {
        protected string Root { get; private set; }
        public ServerStorage(string root) {
            Root = root;
        }
    }

    public class DiskStorage : ServerStorage {
        public DiskStorage(string root) : base(root) {
        }

        public override Project Save(bool content = false) {
            throw new System.NotImplementedException();
        }

        public override Project Load(string userName, string key, bool content = false) {
            var user = new User(userName);
            var directory = LoadDirectory(user, null, System.IO.Path.Combine(Root, user.Path, key), content);
            return Project.FromDirectory(this, directory);
        }

        public Directory LoadDirectory(User user, Directory parent, string path, bool content) {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            var directoryName = System.IO.Path.GetFileName(path);
            var sections = directoryName.Split('.');
            int index = 0;
            string name = sections.First();
            if (sections.Length == 2) {
                index = int.Parse(sections.First());
                name = sections.Skip(1).First();
            }
            var directory = new Directory(Utils.CreateGuid(), name, user, parent);
            directory.Index = index;
            var documents = new List<Document>();
            foreach (var subdirectory in System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.TopDirectoryOnly))
                documents.Add(LoadDirectory(user, parent, subdirectory, content));
            foreach (var file in System.IO.Directory.GetFiles(path).Where(o => o.EndsWith(".md")))
                documents.Add(LoadFile(user, parent, file, content));
            directory.Documents = new ObservableCollection<Document>(
                documents.OrderBy(o => o as Directory == null).ThenBy(o => o.Index).ThenBy(o => o.Name));
            return directory;
        }

        public File LoadFile(User user, Directory parent, string path, bool content) {
            var fileName = System.IO.Path.GetFileName(path);
            var sections = fileName.Split('.');
            int index = 0;
            string name = sections.First();
            if (sections.Length == 3) {
                index = int.Parse(sections.First());
                name = sections.Skip(1).SkipLast(1).Aggregate((a,b) => a + "." + b);
            }
            var file = new File(Utils.CreateGuid(), name, user, parent);
            file.Index = index;
            if (content)
                using (StreamReader stream = new StreamReader(path))
                    file.Text = stream.ReadToEnd();
            return file;
        }

        private string GetPath(Document document) => System.IO.Path.Combine(Root, document.Path);

        public override void CreateDirectory(Directory directory) {
            System.IO.Directory.CreateDirectory(GetPath(directory));
        }

        public override void CreateFile(File file) {
            System.IO.File.Create(GetPath(file));
        }

        public override void DeleteDirectory(Directory directory) {
            System.IO.Directory.Delete(GetPath(directory));
        }

        public override void DeleteFile(File file) {
            System.IO.File.Delete(GetPath(file));
        }

        public override void MoveDirectory(Directory directory, Directory parent) {
            System.IO.Directory.Move(GetPath(directory), System.IO.Path.Combine(GetPath(parent), directory.Key));
        }

        public override void MoveFile(File file, Directory parent) {
            System.IO.File.Move(GetPath(file), System.IO.Path.Combine(GetPath(parent), file.Key));
        }

        public override void RenameDirectory(Directory directory, string name) {
            System.IO.Directory.Move(GetPath(directory), System.IO.Path.Combine(GetPath(directory.Parent), name));
        }

        public override void RenameFile(File file, string name) {
            System.IO.File.Move(GetPath(file), System.IO.Path.Combine(GetPath(file.Parent), name));
        }

        public override void Submit() {
            throw new System.NotImplementedException();
        }
    }
}
