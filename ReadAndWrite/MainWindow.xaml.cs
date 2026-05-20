using System.Windows;
using ReadAndWrite.pages;
using ReadAndWrite.Pages;

namespace ReadAndWrite
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupSidebar();
            MainFrame.Navigate(new CatalogPage());
        }

        public void SetupSidebar()
        {
            BtnAdmin.Visibility = CurrentUser.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;
            BtnAuthor.Visibility = CurrentUser.IsAuthor
                ? Visibility.Visible : Visibility.Collapsed;
            BtnFrozen.Visibility = CurrentUser.IsFrozen
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new CatalogPage());
        private void BtnLists_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ReadingListPage());
        private void BtnProfile_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ProfilePage());
        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new AuthorPage());
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new AdminPage());
        private void BtnFrozen_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ProfilePage());
    }
}