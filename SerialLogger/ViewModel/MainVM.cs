using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialLogger.Services;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SerialLogger.ViewModel
{
    internal class MainVM : ObservableObject
    {
        private readonly SerialPortService _serialService;
        private readonly IDialogBoxesService _dialogBoxesService;
        private readonly IFileOutputService _fileOutputService;


        public ICommand EnumeratePortsCommand { get; }
        public ICommand PortOpenCloseCommand { get; }
        public ICommand ChooseOutputFolderCommand { get; }
        public ICommand ClearTextCommand { get; }
        public ICommand SmallerFontCommand { get; }
        public ICommand LargerFontCommand { get; }

        public MainVM()
        {
            _serialService = new SerialPortService();
            _serialService.PortStatusChanged += _serialService_PortStatusChanged;
            _serialService.DataReceived += _serialService_DataReceived;
            _dialogBoxesService = new DialogBoxesService();
            _fileOutputService = new FileOutputService();

            EnumeratePortsCommand = new RelayCommand(EnumeratePorts);
            PortOpenCloseCommand = new RelayCommand(OpenClosePort);
            ClearTextCommand = new RelayCommand(() => Text = string.Empty);
            ChooseOutputFolderCommand = new RelayCommand(ChooseOutputFolder);
            SmallerFontCommand = new RelayCommand(() => FontSize--);
            LargerFontCommand = new RelayCommand(() => FontSize++);
            this.SaveToFileChanged += MainVM_SaveToFileChanged;

            EnumeratePorts();
        }

        private void MainVM_SaveToFileChanged(object? sender, bool e)
        {
            if (e)
            {
                LogFileName = _fileOutputService.Start(this.OutputFolder);
            }
            else
            {
                _fileOutputService.Stop();
                LogFileName = null;
            }
        }

        private void ChooseOutputFolder()
        {
            if (_dialogBoxesService.ChooseFolder(out string folder))
            {
                OutputFolder = folder;
            }
        }

        private void _serialService_DataReceived(object? sender, string e)
        {
            StringBuilder sb = new StringBuilder();
            int pos;
            while ((pos = e.IndexOf("\n")) >= 0)
            {
                string line = e.Substring(0, pos);
                sb.Append(string.Format("{0:d} {0:HH:mm:ss}  |  {1}\n", DateTime.Now, line));

                if (e.Length == pos + 1)
                    break;

                e = e.Substring(pos + 1);
            }

            string str = sb.ToString();

            if (saveToFile)
            {
                _fileOutputService.Write(str);
            }

            string res = text + str;
            int rows = res.Count(c => c == '\n');
            while (rows-- > maxLines)
            {
                pos = res.IndexOf("\n");
                res = res.Substring(pos + 1);
            }

            Text = res;
            if (scrollToEnd)
            {
                ScrollTextToEnd?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ScrollTextToEnd;

        public event EventHandler<bool> SaveToFileChanged;

        private void OnSaveToFileChanged(bool t)
        {
            SaveToFileChanged?.Invoke(this, t);
        }

        private void _serialService_PortStatusChanged(object? sender, bool e)
        {
            PortOpen = e;
        }


        private void EnumeratePorts()
        {
            ClosePort();
            Port = string.Empty;
            PortOpen = false;
            Ports = new ObservableCollection<PortVM>(_serialService.EnumeratePorts());
        }

        private void OpenClosePort()
        {
            if (portOpen)
                ClosePort();
            else
                OpenPort();
        }

        private void OpenPort()
        {
            if (port != null)
            {
                if (!portOpen)
                {
                    if (!_serialService.Open(port, baudRate))
                    {
                        _dialogBoxesService.ShowMessage("Otvaranje porta nije uspelo.", "Open port");
                    }
                    return;
                }
                _dialogBoxesService.ShowMessage("Otvaranje porta nije uspelo.", "Close port");
            }
            else
            {
                _dialogBoxesService.ShowMessage("Port nije odabran.\nOdaberite port.", "Open port");
            }
        }

        private void ClosePort()
        {
            if (portOpen)
            {
                _serialService.Close();
            }
            _fileOutputService.Stop();
        }

        private bool saveToFile = false;

        public bool SaveToFile
        {
            get => saveToFile; set
            {
                SetProperty(ref saveToFile, value);
                OnSaveToFileChanged(value);
            }
        }


        private string outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string OutputFolder
        {
            get => outputFolder; set => SetProperty(ref outputFolder, value);
        }

        private string? logFileName;

        public string? LogFileName
        {
            get => logFileName; set => SetProperty(ref logFileName, value);
        }

        private bool scrollToEnd = true;

        public bool ScrollToEnd
        {
            get => scrollToEnd; set => SetProperty(ref scrollToEnd, value);
        }

        private int maxLines = 25;
        public int MaxLines { get => maxLines; set => SetProperty(ref maxLines, value); }

        private int fontSize = 10;
        public int FontSize
        {
            get => fontSize;
            set
            {
                if ((value >= 5) && (value <= 36))
                {
                    SetProperty(ref fontSize, value);
                }
            }
        }

        private string text;
        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        private int[] baudRates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        public int[] BaudRates { get => baudRates; }


        private ObservableCollection<PortVM> ports;

        public ObservableCollection<PortVM> Ports
        {
            get => ports ??= new ObservableCollection<PortVM>();
            set => SetProperty(ref ports, value);
        }

        private string port;

        public string Port
        {
            get => port;
            set => SetProperty(ref port, value);
        }

        private int baudRate = 115200;

        public int BaudRate
        {
            get => baudRate;
            set => SetProperty(ref baudRate, value);
        }

        private bool portOpen;

        public bool PortOpen
        {
            get => portOpen;
            set
            {
                SetProperty(ref portOpen, value);
                OnPropertyChanged(nameof(PortOpenButtonTitle));
            }
        }

        public string PortOpenButtonTitle
        {
            get => PortOpen ? "Close port" : "Open port";
        }
    }
}
