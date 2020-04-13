using System;
using System.IO;
using System.Text;

namespace Scribs.Core.Services {

    public class SystemService {
        public string PathCombine(params string[] paths) => Path.Combine(paths);
        public string PathJoin(params string[] paths) => Path.Join(paths);
        public string GetName(string path) => Path.GetFileName(path);
        public bool NodeExists(string path) => Directory.Exists(path);
        public void CreateNode(string path) => Directory.CreateDirectory(path);
        public void DeleteNode(string path, bool recursive) => Directory.Delete(path, recursive);
        public string[] GetNodes(string path) => Directory.GetDirectories(path);
        public string[] GetNodes(string path, string searchPattern) => Directory.GetDirectories(path, searchPattern);
        public string[] GetNodes(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetDirectories(path, searchPattern, enumerationOptions);
        public string[] GetNodes(string path, string searchPattern, SearchOption searchOption) => Directory.GetDirectories(path, searchPattern, searchOption);
        public string[] GetLeaves(string path) => Directory.GetFiles(path);
        public string[] GetLeaves(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
        public string[] GetLeaves(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetFiles(path, searchPattern, enumerationOptions);
        public string[] GetLeaves(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
        public bool LeafExists(string path) => File.Exists(path);
        public void DeleteLeaf(string path) => File.Delete(path);
        public void WriteLeaf(string path, string content) {
            if (String.IsNullOrEmpty(content))
                return;
            using (StreamWriter sw = File.CreateText(path)) {
                sw.Write(content);
                sw.Close();
            }
        }
        public void WriteLeaf(string path, StringBuilder stringBuilder) => WriteLeaf(path, stringBuilder.ToString());
        public LeafReader ReadLeaf(string path) => new LeafReader(path);

        public class LeafReader : IDisposable {
            private StreamReader reader;

            public LeafReader(string path) {
                reader = new StreamReader(path);
            }

            public string ReadLine() => reader.ReadLine();
            public string ReadToEnd() => reader.ReadToEnd();
            public void Reset() {
                reader.DiscardBufferedData();
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            public void Dispose() {
                reader.Close();
                reader.Dispose();
            }
        }
    }
}