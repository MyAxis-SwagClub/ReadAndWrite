using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace ReadAndWrite.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadUsers();
        }

        private void BtnTab_Click(object sender, RoutedEventArgs e)
        {
            string tab = ((Button)sender).Tag.ToString();
            if (tab == "users") LoadUsers();
            else if (tab == "books") LoadBooks();
            else if (tab == "complaints") LoadComplaints();
            else if (tab == "requests") LoadRequests();
        }

        // ── ПОЛЬЗОВАТЕЛИ ──────────────────────────────────────────────
        private void LoadUsers()
        {
            var data = DatabaseHelper.ExecuteQuery(@"
                SELECT u.UserId, u.Login, u.DisplayName,
                       r.RoleName, u.IsFrozen
                FROM Users u
                JOIN Roles r ON u.RoleId = r.RoleId
                ORDER BY u.Login");

            ContentPanel.Children.Clear();

            foreach (DataRow row in data.Rows)
            {
                bool isFrozen = (bool)row["IsFrozen"];
                int userId = (int)row["UserId"];

                var card = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = $"{row["DisplayName"]}  ({row["Login"]})",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(26, 82, 118))
                });
                info.Children.Add(new TextBlock
                {
                    Text = $"Роль: {row["RoleName"]}  •  " +
                           (isFrozen ? "🔒 Заморожен" : "✔ Активен"),
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                Grid.SetColumn(info, 0);
                grid.Children.Add(info);

                var btn = new Button
                {
                    Content = isFrozen ? "Разморозить" : "Заморозить",
                    Background = isFrozen
                        ? new SolidColorBrush(Color.FromRgb(30, 132, 73))
                        : new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = userId,
                    VerticalAlignment = VerticalAlignment.Center
                };
                btn.Click += BtnFreezeUser_Click;

                Grid.SetColumn(btn, 1);
                grid.Children.Add(btn);

                card.Child = grid;
                ContentPanel.Children.Add(card);
            }
        }

        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            var user = DatabaseHelper.ExecuteQuery(
                $"SELECT IsFrozen FROM Users WHERE UserId = {userId}");

            bool isFrozen = (bool)user.Rows[0]["IsFrozen"];

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Users SET IsFrozen = @val WHERE UserId = @id",
                new SqlParameter[]
                {
                    new SqlParameter("@val", !isFrozen),
                    new SqlParameter("@id", userId)
                });
            LoadUsers();
        }

        // ── КНИГИ ─────────────────────────────────────────────────────
        private void LoadBooks()
        {
            var data = DatabaseHelper.ExecuteQuery(@"
                SELECT b.BookId, b.Title, b.IsFrozen,
                       u.DisplayName AS AuthorName
                FROM Books b
                JOIN Users u ON b.AuthorId = u.UserId
                ORDER BY b.Title");

            ContentPanel.Children.Clear();

            foreach (DataRow row in data.Rows)
            {
                bool isFrozen = (bool)row["IsFrozen"];
                int bookId = (int)row["BookId"];

                var card = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = row["Title"].ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(26, 82, 118))
                });
                info.Children.Add(new TextBlock
                {
                    Text = $"Автор: {row["AuthorName"]}  •  " +
                           (isFrozen ? "🔒 Заморожена" : "✔ Активна"),
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                Grid.SetColumn(info, 0);
                grid.Children.Add(info);

                var btn = new Button
                {
                    Content = isFrozen ? "Разморозить" : "Заморозить",
                    Background = isFrozen
                        ? new SolidColorBrush(Color.FromRgb(30, 132, 73))
                        : new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = bookId,
                    VerticalAlignment = VerticalAlignment.Center
                };
                btn.Click += BtnFreezeBook_Click;

                Grid.SetColumn(btn, 1);
                grid.Children.Add(btn);

                card.Child = grid;
                ContentPanel.Children.Add(card);
            }
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            var book = DatabaseHelper.ExecuteQuery(
                $"SELECT IsFrozen FROM Books WHERE BookId = {bookId}");

            bool isFrozen = (bool)book.Rows[0]["IsFrozen"];

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Books SET IsFrozen = @val WHERE BookId = @id",
                new SqlParameter[]
                {
                    new SqlParameter("@val", !isFrozen),
                    new SqlParameter("@id", bookId)
                });
            LoadBooks();
        }

        // ── ЖАЛОБЫ ────────────────────────────────────────────────────
        private void LoadComplaints()
        {
            var data = DatabaseHelper.ExecuteQuery(@"
                SELECT c.ComplaintId, c.Reason,
                       u.DisplayName AS UserName,
                       b.Title AS BookTitle
                FROM Complaints c
                JOIN Users u ON c.UserId = u.UserId
                LEFT JOIN Books b ON c.BookId = b.BookId
                ORDER BY c.ComplaintId DESC");

            ContentPanel.Children.Clear();

            if (data.Rows.Count == 0)
            {
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = "Жалоб нет",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (DataRow row in data.Rows)
            {
                int complaintId = (int)row["ComplaintId"];

                var card = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = $"От: {row["UserName"]}",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(26, 82, 118))
                });
                info.Children.Add(new TextBlock
                {
                    Text = $"Книга: {row["BookTitle"]}",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 3, 0, 3)
                });
                info.Children.Add(new TextBlock
                {
                    Text = row["Reason"].ToString(),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
                });

                Grid.SetColumn(info, 0);
                grid.Children.Add(info);

                var btn = new Button
                {
                    Content = "Закрыть",
                    Background = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = complaintId,
                    VerticalAlignment = VerticalAlignment.Center
                };
                btn.Click += (s, e) =>
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "DELETE FROM Complaints WHERE ComplaintId = @id",
                        new SqlParameter[]
                        {
                            new SqlParameter("@id", complaintId)
                        });
                    LoadComplaints();
                };

                Grid.SetColumn(btn, 1);
                grid.Children.Add(btn);

                card.Child = grid;
                ContentPanel.Children.Add(card);
            }
        }

        // ── ЗАЯВКИ НА АВТОРА ──────────────────────────────────────────
        private void LoadRequests()
        {
            var data = DatabaseHelper.ExecuteQuery(@"
                SELECT rr.RoleRequestId, rr.Status,
                       u.UserId, u.DisplayName, u.Login
                FROM RoleRequests rr
                JOIN Users u ON rr.UserId = u.UserId
                WHERE rr.Status = N'Ожидает'
                ORDER BY rr.RoleRequestId");

            ContentPanel.Children.Clear();

            if (data.Rows.Count == 0)
            {
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = "Заявок нет",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (DataRow row in data.Rows)
            {
                int requestId = (int)row["RoleRequestId"];
                int userId = (int)row["UserId"];

                var card = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = $"{row["DisplayName"]}  ({row["Login"]})",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(26, 82, 118))
                });
                info.Children.Add(new TextBlock
                {
                    Text = "Хочет стать автором",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                Grid.SetColumn(info, 0);
                grid.Children.Add(info);

                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var btnApprove = new Button
                {
                    Content = "Одобрить",
                    Background = new SolidColorBrush(Color.FromRgb(30, 132, 73)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 0, 8, 0)
                };
                btnApprove.Click += (s, e) =>
                {
                    // Меняем роль на Автор (RoleId = 2)
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Users SET RoleId = 2 WHERE UserId = @uid;" +
                        "UPDATE RoleRequests SET Status = N'Одобрено' " +
                        "WHERE RoleRequestId = @rid",
                        new SqlParameter[]
                        {
                            new SqlParameter("@uid", userId),
                            new SqlParameter("@rid", requestId)
                        });
                    LoadRequests();
                };

                var btnReject = new Button
                {
                    Content = "Отклонить",
                    Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                btnReject.Click += (s, e) =>
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE RoleRequests SET Status = N'Отклонено' " +
                        "WHERE RoleRequestId = @rid",
                        new SqlParameter[]
                        {
                            new SqlParameter("@rid", requestId)
                        });
                    LoadRequests();
                };

                buttons.Children.Add(btnApprove);
                buttons.Children.Add(btnReject);

                Grid.SetColumn(buttons, 1);
                grid.Children.Add(buttons);

                card.Child = grid;
                ContentPanel.Children.Add(card);
            }
        }
    }
}