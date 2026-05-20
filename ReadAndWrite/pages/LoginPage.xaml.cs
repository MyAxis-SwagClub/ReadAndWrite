using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using ReadAndWrite.Models;
using ReadAndWrite.Pages;

namespace ReadAndWrite.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void BtnLoginTab_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            BtnLoginTab.Background = System.Windows.Media.Brushes.Crimson;
            BtnRegisterTab.Background =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(15, 52, 96));
        }

        private void BtnRegisterTab_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            BtnRegisterTab.Background = System.Windows.Media.Brushes.Crimson;
            BtnLoginTab.Background =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(15, 52, 96));
        }

 
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Заполните все поля";
                return;
            }

            var result = DatabaseHelper.ExecuteQuery(
                "SELECT UserId, Login, DisplayName, Email, RoleId, IsFrozen " +
                "FROM Users WHERE Login = @login AND PasswordHash = @password",
                new SqlParameter[]
                {
                    new SqlParameter("@login", login),
                    new SqlParameter("@password", password)
                });

            if (result.Rows.Count == 0)
            {
                TxtError.Text = "Неверный логин или пароль";
                return;
            }

            var row = result.Rows[0];
            CurrentUser.UserId = (int)row["UserId"];
            CurrentUser.Login = row["Login"].ToString();
            CurrentUser.DisplayName = row["DisplayName"].ToString();
            CurrentUser.Email = row["Email"].ToString();
            CurrentUser.RoleId = (int)row["RoleId"];
            CurrentUser.IsFrozen = (bool)row["IsFrozen"];

            var mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this).Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtRegLogin.Text.Trim();
            string name = TxtRegName.Text.Trim();
            string email = TxtRegEmail.Text.Trim();
            string password = TxtRegPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Заполните все поля";
                return;
            }

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Users (Login, PasswordHash, Email, DisplayName, RoleId, IsFrozen) " +
                    "VALUES (@login, @password, @email, @name, 1, 0)",
                    new SqlParameter[]
                    {
                        new SqlParameter("@login", login),
                        new SqlParameter("@password", password),
                        new SqlParameter("@email", email),
                        new SqlParameter("@name", name)
                    });

                TxtError.Foreground = System.Windows.Media.Brushes.Green;
                TxtError.Text = "Регистрация успешна! Теперь войдите.";
                BtnLoginTab_Click(null, null);
            }
            catch
            {
                TxtError.Text = "Логин или email уже заняты";
            }
        }
    }
}