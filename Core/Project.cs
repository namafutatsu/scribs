using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public class Project : Directory {
        public Storage Storage { get; set; }

        public Project(Storage storage, string key, string name, User user, Directory parent) : base(key, name, user, parent) {
            Storage = storage;
        }

        public static Project FromDirectory(Storage storage, Directory directory) {
            return new Project(storage, directory.Key, directory.Name, directory.User, null) {
                Documents = directory.Documents
            };
        }
    }
}
