using System.Windows;
using ReadAndWrite.Pages;

namespace ReadAndWrite
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }
    }
}