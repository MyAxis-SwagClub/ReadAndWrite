using System.Windows;
using ReadAndWrite.Pages;

namespace ReadAndWrite
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoginFrame.Navigate(new LoginPage());
        }
    }
}