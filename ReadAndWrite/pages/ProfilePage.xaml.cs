using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace ReadAndWrite.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadProfile();
        }

        private void LoadProfile()
        {
            TxtDisplayName.Text = CurrentUser.DisplayName;
            TxtLogin.Text = "Логин: " + CurrentUser.Login;
            TxtEmail.Text = "Email: " + CurrentUser.Email;

            string role = CurrentUser.IsAdmin ? "Администратор"
                        : CurrentUser.IsAuthor ? "Автор"
                        : "Читатель";
            TxtRole.Text = "Роль: " + role;

            TxtNewName.Text = CurrentUser.DisplayName;
            TxtNewEmail.Text = CurrentUser.Email;

            // Скрываем кнопку заявки если уже автор или админ
            AuthorRequestPanel.Visibility = CurrentUser.IsReader
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newName = TxtNewName.Text.Trim();
            string newEmail = TxtNewEmail.Text.Trim();
            string newPassword = TxtNewPassword.Password;

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newEmail))
            {
                MessageBox.Show("Имя и email не могут быть пустыми");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(newPassword))
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Users SET DisplayName = @name, Email = @email " +
                        "WHERE UserId = @id",
                        new SqlParameter[]
                        {
                            new SqlParameter("@name", newName),
                            new SqlParameter("@email", newEmail),
                            new SqlParameter("@id", CurrentUser.UserId)
                        });
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Users SET DisplayName = @name, Email = @email, " +
                        "PasswordHash = @pass WHERE UserId = @id",
                        new SqlParameter[]
                        {
                            new SqlParameter("@name", newName),
                            new SqlParameter("@email", newEmail),
                            new SqlParameter("@pass", newPassword),
                            new SqlParameter("@id", CurrentUser.UserId)
                        });
                }

                CurrentUser.DisplayName = newName;
                CurrentUser.Email = newEmail;
                LoadProfile();
                MessageBox.Show("Профиль обновлён!");
            }
            catch
            {
                MessageBox.Show("Ошибка при сохранении");
            }
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO RoleRequests (UserId, Status) " +
                    "VALUES (@id, N'Ожидает')",
                    new SqlParameter[]
                    {
                        new SqlParameter("@id", CurrentUser.UserId)
                    });
                MessageBox.Show("Заявка отправлена! Ожидайте решения администратора.");
            }
            catch
            {
                MessageBox.Show("Заявка уже отправлена или произошла ошибка");
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Clear();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Window.GetWindow(this).Close();
        }
    }
}