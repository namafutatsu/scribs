using LibGit2Sharp;
using Scribs.Core;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Console {
    class Program {
        static void Main(string[] args) {
            var gitStorage = new GitStorage(@"C:\Storage\disk\");
            //project.Name = "def";
            string username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["password"];
            Git.SetCredentials(username, password);
            var project = gitStorage.Load("gdrtf", "def", true);
            //gitStorage.Save(project);
            var jsonStorage = new JsonStorage(@"C:\Storage\json\");
            jsonStorage.Save(project);
            //var project = jsonStorage.Load("gdrtf", "jlg2", true);.
        }
    }
}
