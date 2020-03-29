using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public class File : Document {
        public string Text { get; set; }

        public File(string key, string name, User user, Directory parent) : base(key, name, user, parent) {
        }

        public override void Move(Directory directory, int index) {
            Application.Current.Storage.MoveFile(this, directory);
            if (directory != Parent) {
                Parent.Documents.Remove(this);
                Parent = directory;
                directory.Documents.Add(this);
            }
            directory.OrderDocument(this, index);
            Application.Current.Storage.Submit();
        }

        public override void Rename(string name) {
            var key = Utils.CleanName(name);
            Application.Current.Storage.RenameFile(this, Key);
            Name = name;
            Key = key;
            Application.Current.Storage.Submit();
        }

        public override void Delete() {
            Application.Current.Storage.DeleteFile(this);
            Parent.Documents.Remove(this);
            Parent = null;
            Application.Current.Storage.Submit();
        }
    }
}
