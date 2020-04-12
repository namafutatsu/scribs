using Scribs.Core.Entities;

namespace Scribs.Core {

    public interface ILocalStorage {
        public string Root { get; }
        public void Save(Document project);
        public Document Load(string userName, string name);
    }

    public interface IStorageSettings {
        public bool Local { get; set; }
        public string Root { get; set; }
    }
}