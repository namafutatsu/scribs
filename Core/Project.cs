using System.IO;
using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public class Project : Document {
        public Storage Storage { get; set; }

        public Project(Storage storage, string key, string name, User user, Document parent) : base(key, name, user, parent) {
            Storage = storage;
        }

        public static Project FromDocument(Storage storage, Document document) {
            return new Project(storage, document.Key, document.Name, document.User, null) {
                Documents = document.Documents,
                Text = document.Text
            };
        }
    }
}
