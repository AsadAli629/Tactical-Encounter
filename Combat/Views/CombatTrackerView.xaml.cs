using Combat.Models;
using Combat.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Combat.Views
{
    public partial class CombatTrackerPage : Page
    {
        private UnitService _unitService = new();
        private int totalTXE = 0;
        private System.Collections.Generic.List<Unit> _unitCatalog;

        public CombatTrackerPage()
        {
            InitializeComponent();
            LoadUnitCatalog();
        }

        private void LoadUnitCatalog()
        {
            _unitCatalog = _unitService.GetAllUnits(); // Load from UnitCatalog
        }

        #region Party Members

        private void BtnAddMember_Click(object sender, RoutedEventArgs e)
        {
            if (spPartyMembers.Children.Count >= 6)
            {
                MessageBox.Show("Maximum 6 party members allowed!");
                return;
            }

            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            var txt = new TextBox { Width = 120 };
            var btnClear = new Button { Content = "X", Width = 30, Margin = new Thickness(5, 0, 0, 0) };
            btnClear.Click += (s, ev) => spPartyMembers.Children.Remove(sp);

            sp.Children.Add(txt);
            sp.Children.Add(btnClear);
            spPartyMembers.Children.Add(sp);
        }

        private void BtnClearMembers_Click(object sender, RoutedEventArgs e)
        {
            spPartyMembers.Children.Clear();
        }

        #endregion

        #region Active Enemies

        private void BtnAddEnemy_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEnemyWindow(_unitCatalog);
            if (window.ShowDialog() == true)
            {
                var unit = window.SelectedUnit;
                AddEnemyInstance(unit);
            }
        }

        private int GetNextEnemyNumber(string unitName)
        {
            int max = 0;
            foreach (StackPanel sp in spActiveEnemies.Children)
            {
                var btn = (Button)sp.Children[0];
                if (btn.Content.ToString().StartsWith(unitName))
                {
                    var parts = btn.Content.ToString().Split(' ');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int num))
                        max = Math.Max(max, num);
                }
            }
            return max + 1;
        }

        private void AddEnemyInstance(Unit unit)
        {
            int number = GetNextEnemyNumber(unit.Name);
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            var btnUnit = new Button { Content = $"{unit.Name} {number}", Width = 150 };
            btnUnit.Click += (s, e) => ExecuteEnemyAttack(unit, btnUnit.Content.ToString());

            var btnDefeat = new Button { Content = "Defeat", Width = 60, Background = Brushes.Red, Foreground = Brushes.White };
            btnDefeat.Click += (s, e) => DefeatEnemy(unit, sp);

            var btnDelete = new Button { Content = "Delete", Width = 60 };
            btnDelete.Click += (s, e) => spActiveEnemies.Children.Remove(sp);

            sp.Children.Add(btnUnit);
            sp.Children.Add(btnDefeat);
            sp.Children.Add(btnDelete);

            spActiveEnemies.Children.Add(sp);
        }

        private void ExecuteEnemyAttack(Unit unit, string instanceName)
        {
            var rnd = new Random();

            var primary = unit.PrimaryAttacks.Count > 0 ? unit.PrimaryAttacks[rnd.Next(unit.PrimaryAttacks.Count)] : null;
            var bonus = unit.BonusAttacks.Count > 0 ? unit.BonusAttacks[rnd.Next(unit.BonusAttacks.Count)] : null;

            if (primary != null)
                AppendCombatLog($"{instanceName} uses {primary.Name} on {GetTarget(primary.Target, instanceName)}.");

            if (bonus != null)
                AppendCombatLog($"{instanceName} uses {bonus.Name} on {GetTarget(bonus.Target, instanceName)}.");
        }

        private string GetTarget(TargetType type, string excludeName)
        {
            var rnd = new Random();
            if (type == TargetType.Hostile)
            {
                var members = spPartyMembers.Children.Cast<StackPanel>()
                    .Select(sp => ((TextBox)sp.Children[0]).Text)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();
                if (members.Count == 0) return "Unknown";
                return members[rnd.Next(members.Count)];
            }
            else
            {
                var enemies = spActiveEnemies.Children.Cast<StackPanel>()
                    .Select(sp => ((Button)sp.Children[0]).Content.ToString())
                    .Where(n => n != excludeName)
                    .ToList();
                if (enemies.Count == 0) return excludeName;
                return enemies[rnd.Next(enemies.Count)];
            }
        }

        private void DefeatEnemy(Unit unit, StackPanel enemyPanel)
        {
            spActiveEnemies.Children.Remove(enemyPanel);
            var rnd = new Random();
            int txeGained = unit.CR * rnd.Next(0, 7);
            totalTXE += txeGained;
            txtTXE.Text = $"TXE: {totalTXE}";
            AppendCombatLog($"TXE +{txeGained} gained from defeating {unit.Name}.");
        }

        private void BtnResetTXE_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Reset TXE?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                totalTXE = 0;
                txtTXE.Text = $"TXE: {totalTXE}";
            }
        }

        private void AppendCombatLog(string text)
        {
            rtbCombatLog.AppendText(text + "\n");
            rtbCombatLog.ScrollToEnd();
        }

        #endregion
    }
}
