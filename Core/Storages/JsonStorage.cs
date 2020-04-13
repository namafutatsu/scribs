using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Core.Storages {
    public class JsonStorage: ILocalStorage {
        private SystemService system;
        public const string jsonDocument = ".json";
        public string Root { get; }

        public JsonStorage(JsonStorageSettings settings, SystemService system) {
            this.system = system;
            if (settings.Local)
                Root = settings.Root;
        }

        public object JsonConvert { get; private set; }

        public Document Load(string userName, string name) => Load(userName, name, true);
        public void Save(Document project) => Save(project, true);

        public Document Load(string userName, string name, bool content = true) {
            var user = User.GetByName(userName);
            string path = system.PathCombine(Root, user.Path, name);
            if (!system.NodeExists(path))
                system.CreateNode(path);
            var project = ReadJson(system.PathCombine(path, jsonDocument));
            Document.BuildProject(project, user);
            if (content)
                foreach (var document in project.AllDocuments.Values)
                    ReadDocument(path, document);
            return project;
        }

        private Document ReadJson(string path) {
            Document project;
            using (var reader = system.ReadLeaf(path)) {
                var text = reader.ReadToEnd();
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(text))) {
                    var deserializer = new DataContractJsonSerializer(typeof(Document));
                    project = (Document)deserializer.ReadObject(ms);
                }
            }
            return project;
        }

        private void ReadDocument(string path, Document document) {
            if (!system.LeafExists(system.PathCombine(path, document.Key)))
                return;
            using (var reader = system.ReadLeaf(system.PathCombine(path, document.Key)))
                document.Text = reader.ReadToEnd();
        }

        public void Save(Document project, bool content) {
            string path = system.PathCombine(Root, project.User.Path, project.Name);
            if (system.NodeExists(path))
                system.DeleteNode(path, true);
            system.CreateNode(path);
            CreateJson(project, system.PathCombine(path, jsonDocument));
            if (content)
                SaveDocument(project, path);
        }

        private void SaveDocument(Document document, string path) {
            if (document.Text != null)
                WriteDocument(document, system.PathCombine(path, document.Key));
            if (document.Documents == null)
                return;
            foreach (var child in document.Documents)
                SaveDocument(child, path);
        }

        private void WriteDocument(Document document, string path) {
            system.WriteLeaf(path, document.Text);
        }

        private void CreateJson(Document project, string path) {
            var serializer = new DataContractJsonSerializer(typeof(Document));
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, project);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                system.WriteLeaf(path, json);
            }
        }
    }

    public class JsonStorageSettings : IStorageSettings {
        public virtual bool Local { get; set; }
        public virtual string Root { get; set; }
    }
}