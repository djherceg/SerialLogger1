using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLogger.Services
{
    internal interface IFileOutputService
    {
        public string? Start(string path);

        public void Stop();

        public void WriteLine(string line);

        public void Write(string text);
    }
}
