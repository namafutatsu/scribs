using Scribs.Core;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Console {
    class Program {
        static void Main(string[] args) {
            var diskStorage = new DiskStorage(@"C:\temp\scribs\");
            var project = diskStorage.Load("gdrtf", "jlg", true);
            var serializer = new DataContractJsonSerializer(typeof(Document));
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, project);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
            }
            project.Name = "jlg2";
            diskStorage.Save(project, true);
        }
    }
}
 