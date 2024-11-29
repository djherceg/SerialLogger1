using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLogger.Services
{
    public interface IDialogBoxesService
    {
        void ShowMessage(string message, string caption);

        bool ChooseFolder(out string folder);
    }

}
