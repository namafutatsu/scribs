using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Scribs.Core.Services {

    public enum FileType {
        markdown,
        html
    }


    public class PandocService {

        public string GetExtension(FileType type) {
            switch (type) {
                case FileType.html:
                    return ".html";
                case FileType.markdown:
                    return ".md";
                default:
                    return String.Empty;
            }
        }

        private FileType? GetFileType(string path) {
            foreach (FileType type in Enum.GetValues(typeof(FileType)))
                if (path.EndsWith(GetExtension(type)))
                    return type;
            return null;
        }

        public string Convert(string text, FileType from, FileType to) {
            using (var process = new Process()) {
                process.StartInfo.FileName = "pandoc";
                process.StartInfo.Arguments = $"-f {from} -t {to}";
                if (to == FileType.markdown)
                    process.StartInfo.Arguments += " --wrap=none";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.Start();
                using (process.StandardInput)
                    using (var sw = new StreamWriter(process.StandardInput.BaseStream, Encoding.UTF8, 1024, true))
                        sw.Write(text);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        //public void ConvertFile(string input, string output) {
        //}
    }
}
