using SerialLogger.ViewModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM vm;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm = (MainVM)this.FindResource("viewModel");
            vm.ScrollTextToEnd += Vm_ScrollTextToEnd;
        }

        private void Vm_ScrollTextToEnd(object? sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => txtSerialData.ScrollToEnd());
            }
            catch
            {
                // Sometimes it throws here when closing the app
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (vm.PortOpen)
            {
                vm.PortOpenCloseCommand.Execute(null);
            }

            Dispatcher.InvokeShutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow win = new AboutWindow();
            win.Owner = this;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();
        }
    }
}