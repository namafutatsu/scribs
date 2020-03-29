using Scribs.Core;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Console {
    class Program {
        static void Main(string[] args) {
            var project = new DiskStorage(@"C:\temp\scribs\").Load("gdrtf", "jlg", true);
            var serializer = new DataContractJsonSerializer(typeof(Project));
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, project);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
            }
        }
    }
}
