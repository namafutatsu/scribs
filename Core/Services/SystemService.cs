using System;
using System.IO;
using System.Text;

namespace Scribs.Core.Services {

    public class SystemService {
        public string PathCombine(params string[] paths) => Path.Combine(paths);
        public string PathJoin(params string[] paths) => Path.Join(paths);
        public string GetName(string path) => Path.GetFileName(path);
        public virtual bool NodeExists(string path) => Directory.Exists(path);
        public virtual void CreateNode(string path) => Directory.CreateDirectory(path);
        public virtual void DeleteNode(string path, bool recursive) => Directory.Delete(path, recursive);
        public virtual string[] GetNodes(string path) => Directory.GetDirectories(path);
        public virtual string[] GetNodes(string path, string searchPattern) => Directory.GetDirectories(path, searchPattern);
        public virtual string[] GetNodes(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetDirectories(path, searchPattern, enumerationOptions);
        public virtual string[] GetNodes(string path, string searchPattern, SearchOption searchOption) => Directory.GetDirectories(path, searchPattern, searchOption);
        public virtual string[] GetLeaves(string path) => Directory.GetFiles(path);
        public virtual string[] GetLeaves(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
        public virtual string[] GetLeaves(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetFiles(path, searchPattern, enumerationOptions);
        public virtual string[] GetLeaves(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
        public virtual bool LeafExists(string path) => File.Exists(path);
        public virtual void DeleteLeaf(string path) => File.Delete(path);
        public virtual void WriteLeaf(string path, string content) {
            if (String.IsNullOrEmpty(content))
                return;
            using (StreamWriter sw = File.CreateText(path)) {
                sw.Write(content);
                sw.Close();
            }
        }
        public virtual LeafReader ReadLeaf(string path) => new LeafReader(path);
    }

    public class LeafReader : IDisposable {
        private StreamReader reader;

        public LeafReader(string path) {
            if (path != null)
                reader = new StreamReader(path);
        }

        public virtual string ReadLine() => reader.ReadLine();
        public virtual string ReadToEnd() => reader.ReadToEnd();
        public void Reset() {
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose() {
            if (reader == null)
                return;
            reader.Close();
            reader.Dispose();
        }
    }
}