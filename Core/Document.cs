using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public abstract class Document : IPhysical {
        [DataMember]
        public string Key { get; protected set; }
        //[DataMember]
        //public string SafeName { get; protected set; }
        [DataMember]
        public string Name { get; protected set; }
        public int Index { get; set; }
        public Directory Parent { get; protected set; }
        public Directory Project { get; protected set; }
        public User User { get; private set; }
        public string Path => System.IO.Path.Join(Parent != null ? Parent.Path : User.Path, Name);

        public abstract void Rename(string name);
        public abstract void Move(Directory directory, int index);
        public abstract void Delete();

        public Document(string key, string name, User user, Directory parent) {
            Key = key;
            Name = name;
            //SafeName = Utils.CleanName(name);
            User = user;
            Parent = parent;
            if (parent != null)
                Project = parent.Project;
        }
    }
}
