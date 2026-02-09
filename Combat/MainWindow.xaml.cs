using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Combat
{
    public partial class MainWindow : Window
    {

        private Views.UnitCatalogPage _unitCatalogPage;
        private Views.CombatTrackerPage _combatTrackerPage;
        public MainWindow()
        {
            InitializeComponent();

            _unitCatalogPage = new Views.UnitCatalogPage();
            _combatTrackerPage = new Views.CombatTrackerPage();

            // Load default page
            MainFrame.Navigate(_unitCatalogPage);
            chkAlwaysOnTop.IsChecked = false;
        }

        private void chkAlwaysOnTop_Checked(object sender, RoutedEventArgs e)
        {
            SetAlwaysOnTop(true);
        }

        private void chkAlwaysOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAlwaysOnTop(false);
        }



        private void SetAlwaysOnTop(bool isOnTop)
        {
            // Simple WPF way (works in most cases)
            Topmost = isOnTop;

            // If you want more reliable behavior (especially with games / full-screen apps)
            // You can use Win32 API - see alternative method below
        }

        private void UnitCatalog_Click(object sender, RoutedEventArgs e)
        {
            // Update tab visual state
            UnitCatalogTab.Tag = "Selected";
            CombatTrackerTab.Tag = null;

            // Navigate to page
            MainFrame.Navigate(_unitCatalogPage);
        }

        private void CombatTracker_Click(object sender, RoutedEventArgs e)
        {
            // Update tab visual state
            CombatTrackerTab.Tag = "Selected";
            UnitCatalogTab.Tag = null;

            // Navigate to page
            MainFrame.Navigate(_combatTrackerPage);
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