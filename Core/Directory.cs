using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public class Directory : Document {
        [DataMember]
        public ObservableCollection<Document> Documents { get; set; } = new ObservableCollection<Document>();

        public Directory(string key, string name, User user, Directory parent) : base(key, name, user, parent) {
        }

        public void OrderDocument(Document document, int index) {
            var oldIndex = Documents.IndexOf(document);
            Documents.Move(oldIndex, index);
        }

        public Directory CreateDirectory(string name, int index) {
            var directory = new Directory(Utils.CreateGuid(), name, User, this);
            Application.Current.Storage.CreateDirectory(directory);
            OrderDocument(directory, index);
            Application.Current.Storage.Submit();
            return directory;
        }

        public File CreateFile(string name, int index) {
            var file = new File(Utils.CreateGuid(), name, User, this);
            Application.Current.Storage.CreateFile(file);
            OrderDocument(file, index);
            Application.Current.Storage.Submit();
            return file;
        }

        public override void Rename(string name) {
            var key = Utils.CleanName(name);
            Application.Current.Storage.RenameDirectory(this, Key);
            Name = name;
            Key = key;
            Application.Current.Storage.Submit();
        }

        public override void Move(Directory directory, int index) {
            Application.Current.Storage.MoveDirectory(this, directory);
            if (directory != Parent) {
                Parent.Documents.Remove(this);
                Parent = directory;
                directory.Documents.Add(this);
            }
            directory.OrderDocument(this, index);
            Application.Current.Storage.Submit();
        }

        public override void Delete() {
            Application.Current.Storage.DeleteDirectory(this);
            Parent.Documents.Remove(this);
            Parent = null;
            Application.Current.Storage.Submit();
        }
    }
}
