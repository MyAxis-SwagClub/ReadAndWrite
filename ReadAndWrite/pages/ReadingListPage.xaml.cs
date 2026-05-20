using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadAndWrite.Pages
{
    public partial class ReadingListPage : Page
    {
        public ReadingListPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadList();
        }

        private void LoadList(string section = "")
        {
            string query = @"
                SELECT rl.ReadingListId, b.BookId, b.Title,
                       u.DisplayName AS AuthorName,
                       rl.Section,
                       ISNULL(AVG(CAST(r.Rating AS FLOAT)), 0) AS AvgRating
                FROM ReadingLists rl
                JOIN Books b ON rl.BookId = b.BookId
                JOIN Users u ON b.AuthorId = u.UserId
                LEFT JOIN Reviews r ON b.BookId = r.BookId
                WHERE rl.UserId = " + CurrentUser.UserId;

            if (!string.IsNullOrEmpty(section))
                query += $" AND rl.Section = '{section}'";

            query += " GROUP BY rl.ReadingListId, b.BookId, b.Title, u.DisplayName, rl.Section";
            query += " ORDER BY rl.Section, b.Title";

            var data = DatabaseHelper.ExecuteQuery(query);
            ListPanel.Children.Clear();

            if (data.Rows.Count == 0)
            {
                ListPanel.Children.Add(new TextBlock
                {
                    Text = "Список пуст",
                    FontSize = 15,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (DataRow row in data.Rows)
            {
                var card = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Информация о книге
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
                    Text = row["AuthorName"].ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 3, 0, 3)
                });

                info.Children.Add(new TextBlock
                {
                    Text = $"⭐ {(double)row["AvgRating"]:F1}  •  {row["Section"]}",
                    FontSize = 12,
                    Foreground = Brushes.SlateGray
                });

                Grid.SetColumn(info, 0);
                grid.Children.Add(info);

                // Кнопки справа
                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var btnOpen = new Button
                {
                    Content = "Открыть",
                    Background = new SolidColorBrush(Color.FromRgb(36, 113, 163)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 0, 8, 0),
                    Tag = row["BookId"]
                };
                btnOpen.Click += (s, e) =>
                    NavigationService.Navigate(new BookPage((int)((Button)s).Tag));

                var btnRemove = new Button
                {
                    Content = "Удалить",
                    Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = row["ReadingListId"]
                };
                btnRemove.Click += BtnRemove_Click;

                buttons.Children.Add(btnOpen);
                buttons.Children.Add(btnRemove);

                Grid.SetColumn(buttons, 1);
                grid.Children.Add(buttons);

                card.Child = grid;
                ListPanel.Children.Add(card);
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем цвет всех кнопок
            foreach (var child in ((StackPanel)((Border)((StackPanel)ListPanel.Parent)
                .Parent).Parent).Children)
            {
                // проще через имена
            }

            string section = ((Button)sender).Tag?.ToString() ?? "";
            LoadList(section);
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM ReadingLists WHERE ReadingListId = @id",
                new Microsoft.Data.SqlClient.SqlParameter[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@id", id)
                });
            LoadList();
        }
    }
}