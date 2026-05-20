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
            var data = DatabaseHelper.ExecuteQuery(
                $"SELECT IsFrozen FROM Users WHERE UserId = {CurrentUser.UserId}");
            if (data.Rows.Count > 0)
                CurrentUser.IsFrozen = (bool)data.Rows[0]["IsFrozen"];

            TxtDisplayName.Text = CurrentUser.DisplayName;
            TxtLogin.Text = "Логин: " + CurrentUser.Login;
            TxtEmail.Text = "Email: " + CurrentUser.Email;

            string role = CurrentUser.IsAdmin ? "Администратор"
                        : CurrentUser.IsAuthor ? "Автор"
                        : "Читатель";
            TxtRole.Text = "Роль: " + role;

            TxtNewName.Text = CurrentUser.DisplayName;
            TxtNewEmail.Text = CurrentUser.Email;

            AuthorRequestPanel.Visibility = CurrentUser.IsReader && !CurrentUser.IsFrozen
                ? Visibility.Visible : Visibility.Collapsed;

            if (CurrentUser.IsFrozen)
            {
                TxtRole.Text += "  🔒 Аккаунт заморожен";
                TxtRole.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете изменять данные.");
                return;
            }

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
            if (CurrentUser.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Вы не можете подавать заявки.");
                return;
            }

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