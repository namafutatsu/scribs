using Scribs.Core;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Console {
    class Program {
        static void Main(string[] args) {
            //var diskStorage = new DiskStorage(@"C:\temp\scribs\");
            //var project = diskStorage.Load("gdrtf", "jlg", true);
            //project.Name = "jlg2";
            //diskStorage.Save(project, true);
            var jsonStorage = new JsonStorage(@"C:\temp\json\");
            //jsonStorage.Save(project, true);
            var project = jsonStorage.Load("gdrtf", "jlg2", true);
        }
    }
}
