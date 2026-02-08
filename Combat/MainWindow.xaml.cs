using System.Windows;
using System.Windows.Controls;

namespace Combat
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load default page
            MainFrame.Navigate(new Views.UnitCatalogPage());
        }

        private void UnitCatalog_Click(object sender, RoutedEventArgs e)
        {
            // Update tab visual state
            UnitCatalogTab.Tag = "Selected";
            CombatTrackerTab.Tag = null;

            // Navigate to page
            MainFrame.Navigate(new Views.UnitCatalogPage());
        }

        private void CombatTracker_Click(object sender, RoutedEventArgs e)
        {
            // Update tab visual state
            CombatTrackerTab.Tag = "Selected";
            UnitCatalogTab.Tag = null;

            // Navigate to page
            MainFrame.Navigate(new Views.CombatTrackerPage());
        }

        // Window control buttons
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}