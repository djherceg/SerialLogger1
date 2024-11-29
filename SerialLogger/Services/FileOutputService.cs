using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLogger.Services
{
    internal class FileOutputService : IFileOutputService
    {
        private const int writeLimit = 1000;
        private string path;
        StreamWriter? sw;
        private int linesWritten;

        public FileOutputService()
        {
            path = string.Empty;
        }

        public string? Start(string path)
        {
            this.path = path;
            if (Path.Exists(path))
            {
                string file;
                while (true)
                {
                    file = GenerateFilename() + ".log";
                    file = Path.Combine(path, file);
                    if (!File.Exists(file))
                        break;
                }
                linesWritten = 0;
                sw = new StreamWriter(file);
                return file;
            }
            return null;
        }

        public void Stop()
        {
            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
            }
        }

        public void WriteLine(string line)
        {
            if (sw != null)
            {
                try
                {
                    sw.WriteLine(line);
                    linesWritten++;

                    if (linesWritten >= writeLimit)
                    {
                        Stop();
                        Start(path);
                    }
                }
                catch
                {
                    sw?.Dispose();
                    sw = null;
                }
            }
        }

        public void Write(string text)
        {
            if (sw != null)
            {
                try
                {
                    sw.Write(text);
                    linesWritten += text.Count(c => c == '\n');

                    if (linesWritten >= writeLimit)
                    {
                        Stop();
                        Start(path);
                    }
                }
                catch
                {
                    sw?.Dispose();
                    sw = null;
                }
            }
        }



        static string GenerateFilename()
        {
            string dateTimePart = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string randomPart = GenerateRandomString(3);
            return $"{dateTimePart}_{randomPart}";
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
    }
}
