using Combat.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Combat.Views
{
    public partial class AddEnemyWindow : Window
    {
        private List<Unit> _catalog;
        public Unit SelectedUnit { get; private set; }

        public AddEnemyWindow(List<Unit> catalog)
        {
            InitializeComponent();
            _catalog = catalog;
            lbUnits.ItemsSource = _catalog;

            txtSearch.TextChanged += (s, e) =>
            {
                var query = txtSearch.Text.ToLower();
                lbUnits.ItemsSource = _catalog.Where(u => u.Name.ToLower().Contains(query)).ToList();
            };
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lbUnits.SelectedItem is Unit selected)
            {
                SelectedUnit = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Select a unit first!");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
