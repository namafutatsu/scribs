using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public class Document {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ObservableCollection<Document> Documents { get; set; }
        [DataMember]
        public int Index { get; set; }
        //[DataMember]
        public string Text { get; set; }

        public bool IsLeaf => Documents == null;
        public Document Parent { get; protected set; }
        public Document Project { get; protected set; }
        public User User { get; private set; }
        public string Path => System.IO.Path.Join(Parent != null ? Parent.Path : User.Path, Name);

        public Document(string key, string name, User user, Document parent) {
            Key = key;
            Name = name;
            User = user;
            Parent = parent;
            if (parent != null)
                Project = parent.Project;
        }

        public void OrderDocument(Document document, int index) {
            var oldIndex = Documents.IndexOf(document);
            Documents.Move(oldIndex, index);
        }

        public IDictionary<string, string> Metadata => new Dictionary<string, string> {
            ["id"] = Key
        };
    }
}
