using Microsoft.Win32;
using System.Windows;

namespace SerialLogger.Services
{
    public class DialogBoxesService : IDialogBoxesService
    {
        public bool ChooseFolder(out string folder)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var folderDialog = new OpenFolderDialog
                {
                    Title = "Select Folder",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                bool? result = folderDialog.ShowDialog();
                if (result == true)
                {
                    folder = folderDialog.FolderName;
                    return true;
                }
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return false;
            }
        }

        public void ShowMessage(string message, string caption)
        {
            System.Windows.MessageBox.Show(message, caption);
        }


    }

}
