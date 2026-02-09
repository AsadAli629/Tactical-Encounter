using Combat.Models;
using Combat.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Combat.Views
{
    public partial class UnitCatalogPage : Page
    {
        private UnitService _service = new();
        private Unit _selectedUnit;
        private System.Collections.Generic.List<Unit> _allUnits;

        public UnitCatalogPage()
        {
            InitializeComponent();
            LoadUnits();

            // Search bar live filter
            txtSearch.TextChanged += (s, e) =>
            {
                var query = txtSearch.Text.ToLower();
                lvUnits.ItemsSource = _allUnits.Where(u => u.Name.ToLower().Contains(query)).ToList();
            };
        }

        private void LoadUnits()
        {
            _allUnits = _service.GetAllUnits();
            lvUnits.ItemsSource = _allUnits;
        }

        private void LvUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvUnits.SelectedItem is Unit selected)
            {
                _selectedUnit = selected;
                txtName.Text = selected.Name;
                txtCR.Text = selected.CR.ToString();
                cbCategory.SelectedItem = cbCategory.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(c => c.Content.ToString() == selected.Category);

                LoadAttackRows(selected.PrimaryAttacks, selected.BonusAttacks);

                btnDelete.IsEnabled = true;
            }
        }

        // Load existing attacks into separate sections
        private void LoadAttackRows(System.Collections.Generic.List<Attack> primary, System.Collections.Generic.List<Attack> bonus)
        {
            spPrimaryAttacks.Children.Clear();
            spBonusAttacks.Children.Clear();

            if (primary != null)
            {
                foreach (var atk in primary)
                    AddAttackRow(spPrimaryAttacks, atk);
            }

            if (bonus != null)
            {
                foreach (var atk in bonus)
                    AddAttackRow(spBonusAttacks, atk);
            }
        }

        // Add Primary or Bonus attack dynamically
        private void BtnAddPrimaryAttack_Click(object sender, RoutedEventArgs e)
        {
            AddAttackRow(spPrimaryAttacks, null);
        }

        private void BtnAddBonusAttack_Click(object sender, RoutedEventArgs e)
        {
            AddAttackRow(spBonusAttacks, null);
        }

        // Add attack row to specified section
        private void AddAttackRow(StackPanel parent, Attack existingAttack)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

            var txtName = new TextBox
            {
                Width = 120,
                Text = existingAttack?.Name ?? "",
                Background = (System.Windows.Media.Brush)Application.Current.Resources["BackgroundDark"],
                Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextPrimaryDark"]
            };

            var cbTarget = new ComboBox
            {
                Width = 110,
                Background = (Brush)FindResource("BackgroundDark"),
                Foreground = (Brush)FindResource("TextPrimaryDark"),
                BorderBrush = (Brush)FindResource("BorderDark"),
                ItemsSource = new[] { "Hostile", "Friendly" }, // or keep Enum
                SelectedIndex = 0,                             // Hostile by default
                Padding = new Thickness(6, 4, 6, 4)
            };

            var btnDelete = new Button
            {
                Content = "−",
                Width = 30,
                Background = (System.Windows.Media.Brush)Application.Current.Resources["DeleteRed"],
                Foreground = System.Windows.Media.Brushes.White
            };
            btnDelete.Click += (s, e) => parent.Children.Remove(sp);

            sp.Children.Add(txtName);
            sp.Children.Add(cbTarget);
            sp.Children.Add(btnDelete);

            // Save references for later
            sp.Tag = new { TxtName = txtName, CbTarget = cbTarget };

            parent.Children.Add(sp);
        }

        private void BtnAddNewUnit_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var unit = _selectedUnit ?? new Unit();
            unit.Name = txtName.Text;
            unit.Category = cbCategory.SelectedItem is ComboBoxItem cbi ? cbi.Content.ToString() : "";
            unit.CR = int.TryParse(txtCR.Text, out var cr) ? cr : 0;

            unit.PrimaryAttacks.Clear();
            unit.BonusAttacks.Clear();

            // Save Primary Attacks
            foreach (StackPanel sp in spPrimaryAttacks.Children)
            {
                dynamic t = sp.Tag;

                if (!string.IsNullOrWhiteSpace(t.TxtName.Text))
                {
                    var target = t.CbTarget.SelectedItem is TargetType tt
                        ? tt
                        : TargetType.Hostile; // default

                    unit.PrimaryAttacks.Add(new Attack
                    {
                        Name = t.TxtName.Text,
                        Target = target
                    });
                }
            }


            // Save Bonus Attacks
            foreach (StackPanel sp in spBonusAttacks.Children)
            {
                dynamic t = sp.Tag;

                if (!string.IsNullOrWhiteSpace(t.TxtName.Text))
                {
                    var target = t.CbTarget.SelectedItem is TargetType tt
                        ? tt
                        : TargetType.Hostile; // default

                    unit.BonusAttacks.Add(new Attack
                    {
                        Name = t.TxtName.Text,
                        Target = target
                    });
                }
            }


            _service.SaveUnit(unit);
            LoadUnits();
            MessageBox.Show("Unit saved!");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit != null)
            {
                _service.DeleteUnit(_selectedUnit.Id);
                LoadUnits();
                ClearForm();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtCR.Clear();
            cbCategory.SelectedIndex = -1;
            spPrimaryAttacks.Children.Clear();
            spBonusAttacks.Children.Clear();
            lvUnits.SelectedItem = null;
            _selectedUnit = null;
            btnDelete.IsEnabled = false;
        }
    }
}
