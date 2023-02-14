using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHandler
{
    public class FileHandler
    {
        public FileHandler()
        {

        }
        
        /// <summary>
        /// Gets the file cleaned from comments and spaces.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public List<string> GetFile(string str)
        {
            List<string> lines = File.ReadAllLines(str).ToList();
            List<string> cleanlines = lines.Select(RemoveComments).Where(x => x.Length != 0).ToList();
            return cleanlines;
        }

        public List<string> GetFiles(string str, string extension, string specialfile)
        {
            string[] files = Directory.GetFiles(str);
            List<string> vmfiles = new List<string>();
            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(extension))
                {
                    vmfiles.Add(files[i]);
                }
            }

            if (vmfiles.Contains(str + "\\" + specialfile))
            {
                vmfiles.Remove(str + "\\" + specialfile);
                vmfiles.Insert(0, str + "\\" + specialfile);
            }

            List<string> lines = new List<string>();
            for (var i = 0; i < vmfiles.Count; i++)
            {
                lines.AddRange(GetFile(vmfiles[i]).ToList());
            }
            return lines;
        }
        
        /// <summary>
        /// Creates an outputfile
        /// </summary>
        /// <param name="str"></param>
        /// <param name="converted"></param>
        public void PrintFile(string str,List<string> converted, string filetype)
        {
            string fname = Path.GetFileNameWithoutExtension(str);
            string path = Path.GetDirectoryName(str);
            File.WriteAllLines(path + "//" + fname + filetype, converted);
        }

        public void PrintFile(string str, List<string> converted, string filetype, string path)
        {
            string fname = Path.GetFileNameWithoutExtension(str);
            File.WriteAllLines(path + "//" + fname + filetype, converted);
        }

        /// <summary>
        /// Removes comments from a line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string RemoveComments(string line)
        {
            return line.Split("//", StringSplitOptions.None)[0].Trim().Replace("  ", string.Empty);
        }
    }
}
