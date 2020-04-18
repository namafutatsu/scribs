using Scribs.Core.Entities;

namespace Scribs.Core {

    public interface IStorage {
        public void Save(Document project);
        public Document Load(string userName, string name);
    }

    public interface ILocalStorage : IStorage {
        public string Root { get; }
    }

    public interface IStorageSettings {
        public bool Local { get; set; }
        public string Root { get; set; }
    }
}