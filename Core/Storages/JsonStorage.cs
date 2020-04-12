using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Scribs.Core.Entities;

namespace Scribs.Core.Storages {
    public class JsonStorage: ILocalStorage {
        public const string jsonDocument = ".json";
        public string Root { get; }

        public JsonStorage(JsonStorageSettings settings) {
            if (settings.Local)
                Root = settings.Root;
        }

        public object JsonConvert { get; private set; }

        public Document Load(string userName, string name) => Load(userName, name, true);
        public void Save(Document project) => Save(project, true);

        public Document Load(string userName, string name, bool content = true) {
            var user = User.GetByName(userName);
            string path = Path.Combine(Root, user.Path, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var project = ReadJson(Path.Combine(path, jsonDocument));
            Document.BuildProject(project, user);
            if (content)
                foreach (var document in project.AllDocuments.Values)
                    ReadDocument(path, document);
            return project;
        }

        private Document ReadJson(string path) {
            Document project;
            using (StreamReader reader = new StreamReader(path)) {
                var text = reader.ReadToEnd();
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(text))) {
                    var deserializer = new DataContractJsonSerializer(typeof(Document));
                    project = (Document)deserializer.ReadObject(ms);
                }
            }
            return project;
        }

        private void ReadDocument(string path, Document document) {
            if (!File.Exists(Path.Combine(path, document.Key)))
                return;
            using (StreamReader reader = new StreamReader(Path.Combine(path, document.Key)))
                document.Text = reader.ReadToEnd();
        }

        public void Save(Document project, bool content) {
            string path = Path.Combine(Root, project.User.Path, project.Name);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
            CreateJson(project, Path.Combine(path, jsonDocument));
            if (content)
                SaveDocument(project, path);
        }

        private void SaveDocument(Document document, string path) {
            if (document.Text != null)
                WriteDocument(document, Path.Combine(path, document.Key));
            if (document.Documents == null)
                return;
            foreach (var child in document.Documents)
                SaveDocument(child, path);
        }

        private void WriteDocument(Document document, string path) {
            using (StreamWriter sw = File.CreateText(path))
                sw.Write(document.Text);
        }

        private void CreateJson(Document project, string path) {
            var serializer = new DataContractJsonSerializer(typeof(Document));
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, project);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                using (StreamWriter sw = File.CreateText(path)) {
                    sw.Write(json);
                }
            }
        }
    }

    public class JsonStorageSettings : IStorageSettings {
        public bool Local { get; set; }
        public string Root { get; set; }
    }
}