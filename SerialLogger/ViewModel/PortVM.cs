using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLogger.ViewModel
{
    internal class PortVM
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public PortVM(string  name, string description)
        {
            this.Name = name;
            this.Description = description; 
        }
    }
}
