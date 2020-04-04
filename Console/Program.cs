using LibGit2Sharp;
using Scribs.Core;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Console {
    class Program {
        static void Main(string[] args) {
            var diskStorage = new DiskStorage(@"C:\Storage\disk\");
            var project = diskStorage.Load("gdrtf", "test", true);
            project.Name = "def";
            string username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["password"];
            diskStorage.SetCredentials(username, password);
            diskStorage.Save(project);
            //var jsonStorage = new JsonStorage(@"C:\Storage\json\");
            //jsonStorage.Save(project, true);
            //var project = jsonStorage.Load("gdrtf", "jlg2", true);
        }
    }
}
