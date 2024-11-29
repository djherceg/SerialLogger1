using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Management;
using System.Text.RegularExpressions;
using SerialLogger.ViewModel;
using System.IO;

namespace SerialLogger.Services
{
    internal class SerialPortService
    {
        private SerialPort _serialPort;
        private System.Threading.Timer _timer;
        private StringBuilder _dataBuffer = new StringBuilder();
        public event EventHandler<bool> PortStatusChanged;
        public event EventHandler<string> DataReceived;

        public SerialPortService()
        {
        }

        private bool _continue;
        private Thread _readThread;

        private void StartReading()
        {
            _continue = true;
            _readThread = new Thread(Read);
            _readThread.Name = "SerialReadThread";
            _readThread.Start();
        }

        private void Read()
        {
            while (_continue)
            {
                if (!_serialPort.IsOpen)
                {
                    break;
                }

                try
                {
                    string data = _serialPort.ReadLine();
                    if ((data == null) || (data.Length == 0))
                    {
                        continue;
                    }
                    lock (_dataBuffer)
                    {
                        _dataBuffer.Append(data);
                    }
                }

                catch (TimeoutException) { }
                catch (OperationCanceledException)
                {   // Handle disconnection
                    _continue = false;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    ClosePort();
                }
                catch (IOException)
                {   // Handle disconnection
                    _continue = false;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    ClosePort();
                }
            }
        }

        private void StopReading()
        {
            _continue = false;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _readThread.Join();
        }

        private void ClosePort()
        {
            if ((_serialPort != null) && (_serialPort.IsOpen))
            {
                _serialPort.Close();
            }
            _serialPort = null;

            OnPortStatusChanged(false);
        }

        /// <summary>
        /// This is called from the UI to stop reading
        /// </summary>
        public void Close()
        {
            StopReading();
            ClosePort();
        }



        public bool Open(string port, int baudRate)
        {
            if (_serialPort == null)
            {
                try
                {
                    _serialPort = new SerialPort(port, baudRate);
                    _serialPort.ReadTimeout = 1000;

                    if (!_serialPort.IsOpen)
                    {
                        _serialPort.Open();
                        _serialPort.DiscardInBuffer();
                        OnPortStatusChanged(true);
                        StartReading();
                        _timer = new System.Threading.Timer(ProcessData, null, 0, 100); // Process data every 100 ms
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _serialPort = null;
                }
            }
            return false;
        }



        private void ProcessData(object? state)
        {
            string data;
            lock (_dataBuffer)
            {
                data = _dataBuffer.ToString();
                _dataBuffer.Clear();
            }
            data = data.Replace("\r\n", "\n").Replace("\r", "\n");
            DataReceived?.Invoke(this, data);
        }


        private void OnPortStatusChanged(bool isOpen)
        {
            PortStatusChanged?.Invoke(this, isOpen);
        }



        #region --- enumerate ports ------
        public List<PortVM> EnumeratePorts()
        {
            string[] ports = SerialPort.GetPortNames();

            List<PortVM> portVMs = new List<PortVM>();
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");
            foreach (var device in searcher.Get())
            {
                PortVM pvm;
                string name = device["Name"]?.ToString();
                string deviceId = device["DeviceID"]?.ToString();
                string pname = ExtractPortName(name);
                if (deviceId != null && deviceId.Contains("USB"))
                {
                    pvm = new PortVM(pname, pname + " USB " + name);
                    //Console.WriteLine($"USB Device: {name}");
                }
                else
                {
                    pvm = new PortVM(pname, pname + " " + name);
                    //Console.WriteLine($"Non-USB Device: {name}");
                }
                portVMs.Add(pvm);
            }
            return portVMs;
        }

        static string ExtractPortName(string deviceName)
        {
            var match = Regex.Match(deviceName, @"\(COM\d+\)");
            return match.Value.Trim('(', ')');
        }
        #endregion
    }
}

