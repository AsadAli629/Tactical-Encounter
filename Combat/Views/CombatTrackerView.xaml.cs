using Combat.Models;
using Combat.Services;
using LiteDB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;

namespace Combat.Views
{
    public partial class CombatTrackerPage : Page
    {
        private UnitService _unitService = new();
        private int totalTXE = 0;
        private System.Collections.Generic.List<Unit> _unitCatalog;
        private PartyMemberService _partyService = new();
        private CombatLogService _logService = new();



        public CombatTrackerPage()
        {
            InitializeComponent();
            LoadUnitCatalog();
            LoadPartyMembers();
            LoadCombatLogs();
        }

        private void LoadCombatLogs()
        {
            var logs = _logService.GetAll();
            foreach (var log in logs)
            {
                AppendCombatLogUI(log.Message);
            }
        }

        private void AppendCombatLogUI(string text)
        {
            var paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0);

            var run = new Run(text + "\n");

            // Simple keyword-based coloring
            if (text.Contains("uses") || text.Contains("attacks") || text.Contains("casts"))
            {
                run.Foreground = (Brush)FindResource("LogEnemyAttack");
            }
            else if (text.Contains("TXE") || text.Contains("gained") || text.Contains("experience"))
            {
                run.Foreground = (Brush)FindResource("LogTXE");
            }
            else if (text.Contains("Defeated") || text.Contains("killed") || text.Contains("down"))
            {
                run.Foreground = (Brush)FindResource("LogCritical");
            }
            else if (text.Contains("failed") || text.Contains("missed"))
            {
                run.Foreground = (Brush)FindResource("LogWarning");
            }
            else
            {
                run.Foreground = (Brush)FindResource("LogDefault");
            }

            // Optional: color names differently (very hacker-like)
            if (text.Contains(" on "))
            {
                var parts = text.Split(new[] { " on " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    paragraph.Inlines.Add(new Run(parts[0] + " on ") { Foreground = (Brush)FindResource("LogEnemyAttack") });

                    // Target name
                    var targetPart = parts[1];
                    paragraph.Inlines.Add(new Run(targetPart)
                    {
                        Foreground = targetPart.ContainsAny(new[] { "Goblin", "Orc", "Dragon", "Bandit" /* add your enemy types */ })
                            ? (Brush)FindResource("LogNameEnemy")
                            : (Brush)FindResource("LogNamePlayer")
                    });

                    paragraph.Inlines.Add(new Run("\n"));
                    rtbCombatLog.Document.Blocks.Add(paragraph);
                    rtbCombatLog.ScrollToEnd();
                    return;
                }
            }

            paragraph.Inlines.Add(run);
            rtbCombatLog.Document.Blocks.Add(paragraph);
            rtbCombatLog.ScrollToEnd();
        }



        private void LoadPartyMembers()
        {
            var members = _partyService.GetAll();
            spPartyMembers.Children.Clear(); // Clear existing UI first
            foreach (var m in members)
                AddMemberUI(m.Name, m.Id); // Pass the ID for updates
        }

        private void AddMemberUI(string name = "", int id = 0)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            var txt = new TextBox
            {
                Width = 150,
                Text = name,
                Background = (Brush)FindResource("SurfaceDark"),
                Foreground = (Brush)FindResource("TextPrimaryDark"),
                BorderBrush = (Brush)FindResource("BorderDark"),
                Tag = id // Store the ID in the Tag property
            };

            // Add text changed handler to save to database
            txt.TextChanged += (s, ev) => SavePartyMember(txt);

            var btnClear = new Button
            {
                Content = "X",
                Width = 30,
                Margin = new Thickness(5, 0, 0, 0),
                Tag = id // Store ID for deletion
            };
            btnClear.Click += (s, ev) => RemovePartyMember(sp, (int)btnClear.Tag);

            sp.Children.Add(txt);
            sp.Children.Add(btnClear);
            spPartyMembers.Children.Add(sp);
        }

        private void SavePartyMember(TextBox textBox)
        {
            var memberId = (int)textBox.Tag;
            var member = new PartyMember
            {
                Id = memberId,
                Name = textBox.Text
            };

            // You need to add an Update method to PartyMemberService
            _partyService.Update(member);
        }

        private void RemovePartyMember(StackPanel panel, int id)
        {
            spPartyMembers.Children.Remove(panel);
            _partyService.Delete(id); // Need to add Delete method
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

            // Generate a new ID (you might want to implement a proper ID generator)
            var existingIds = _partyService.GetAll().Select(m => m.Id).ToList();
            var newId = existingIds.Count > 0 ? existingIds.Max() + 1 : 1;

            var newMember = new PartyMember
            {
                Id = newId,
                Name = ""
            };

            _partyService.Add(newMember);
            AddMemberUI("", newId);
        }


        private void BtnClearMembers_Click(object sender, RoutedEventArgs e)
        {
            spPartyMembers.Children.Clear();
            _partyService.Clear();
        }

        #endregion

        #region Active Enemies

        private void BtnAddEnemy_Click(object sender, RoutedEventArgs e)
        {
            // Get the actual window that hosts this page
            Window window = Window.GetWindow(this);

            if (window == null) return;  // safety check (shouldn't happen)

            bool wasTopmost = window.Topmost;

            // Temporarily disable always-on-top
            window.Topmost = false;

            var addWindow = new AddEnemyWindow(_unitCatalog);
            addWindow.Owner = window;                    // important for correct z-order & centering

            bool? result = addWindow.ShowDialog();

            // Restore original state
            window.Topmost = wasTopmost;

            if (result == true)
            {
                var unit = addWindow.SelectedUnit;
                AddEnemyInstance(unit);
            }
        }

        private int GetNextEnemyNumber(string unitName)
        {
            int max = 0;

            foreach (Border border in spActiveEnemies.Children.OfType<Border>())
            {
                if (border.Child is StackPanel cardStack &&
                    cardStack.Children.Count > 0 &&
                    cardStack.Children[0] is TextBlock nameTextBlock)
                {
                    var btnContent = nameTextBlock.Text;    // e.g. "Goblin 3"
                    if (btnContent.StartsWith(unitName))
                    {
                        var parts = btnContent.Split(' ');
                        if (parts.Length > 1 && int.TryParse(parts[^1], out int num))
                        {
                            max = Math.Max(max, num);
                        }
                    }
                }
            }

            return max + 1;
        }

        private void AddEnemyInstance(Unit unit)
        {
            int number = GetNextEnemyNumber(unit.Name);
            string instanceName = $"{unit.Name} {number}";

            // ────────────────────────────────────────────────
            // Card container
            var cardBorder = new Border
            {
                Background = (Brush)FindResource("SurfaceDark"),
                BorderBrush = (Brush)FindResource("BorderDark"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 12),
                // Optional: subtle hover effect later
            };

            var cardStack = new StackPanel
            {
                // We use vertical stack, but name will be centered
            };

            // ────────────────────────────────────────────────
            // Clickable Name – centered
            var nameText = new TextBlock
            {
                Text = instanceName,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryAccent"),     // make it stand out
                TextAlignment = TextAlignment.Center,                   // center text
                TextDecorations = TextDecorations.Underline,            // underline = clickable hint
                Cursor = Cursors.Hand,                                  // hand cursor on hover
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(8, 6, 8, 6),
            };

            // Very important: make it look interactive on hover
            nameText.MouseEnter += (s, e) =>
            {
                nameText.Background = new SolidColorBrush(Color.FromArgb(40, 100, 150, 255)); // light blue tint
                nameText.Foreground = Brushes.White;
            };

            nameText.MouseLeave += (s, e) =>
            {
                nameText.Background = Brushes.Transparent;
                nameText.Foreground = (Brush)FindResource("PrimaryAccent");
            };

            // Click to attack
            nameText.MouseLeftButtonUp += (s, e) =>
                ExecuteEnemyAttack(unit, instanceName);

            // ────────────────────────────────────────────────
            // Buttons row
            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,   // center buttons too
                Margin = new Thickness(0, 0, 0, 4)
            };

            var btnDefeat = new Button
            {
                Content = "Defeat",
                Width = 90,
                Margin = new Thickness(0, 0, 12, 0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush(Colors.DarkRed),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Medium
            };
            btnDefeat.Click += (s, e) => DefeatEnemy(unit, cardBorder);

            var btnDelete = new Button
            {
                Content = "Delete",
                Width = 90,
                Padding = new Thickness(12, 6, 12, 6),
                Background = (Brush)FindResource("BorderDark"),
                Foreground = (Brush)FindResource("TextPrimaryDark"),
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Medium
            };
            btnDelete.Click += (s, e) => spActiveEnemies.Children.Remove(cardBorder);

            buttonRow.Children.Add(btnDefeat);
            buttonRow.Children.Add(btnDelete);

            // ────────────────────────────────────────────────
            // Assemble card
            cardStack.Children.Add(nameText);
            cardStack.Children.Add(buttonRow);

            cardBorder.Child = cardStack;

            spActiveEnemies.Children.Add(cardBorder);
        }


        private void ExecuteEnemyAttack(Unit unit, string instanceName)
        {
            var rnd = new Random();

            // Primary attack
            if (unit.PrimaryAttacks.Count > 0)
            {
                var primary = unit.PrimaryAttacks[rnd.Next(unit.PrimaryAttacks.Count)];
                var target = GetTarget(primary.Target, instanceName);

                if (target != null)
                {
                    AppendCombatLog(
                        $"{instanceName} uses {primary.Name} on {target}.",
                        (Brush)FindResource("LogEnemyAttack")
                    );
                }
                // else → silently skip (no log)
            }

            // Bonus attack
            if (unit.BonusAttacks.Count > 0)
            {
                var bonus = unit.BonusAttacks[rnd.Next(unit.BonusAttacks.Count)];
                var target = GetTarget(bonus.Target, instanceName);

                if (target != null)
                {
                    AppendCombatLog(
                        $"{instanceName} uses {bonus.Name} on {target}.",
                        (Brush)FindResource("LogEnemyAttack")
                    );
                }
                // else → silently skip
            }
        }

        private string GetTarget(TargetType type, string excludeName)
        {
            var rnd = new Random();
            if (type == TargetType.Hostile)
            {
                // Get all non-empty party member names
                var members = spPartyMembers.Children
                    .Cast<StackPanel>()
                    .Select(sp => ((TextBox)sp.Children[0]).Text?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                if (members.Count == 0)
                {
                    return null;           // ← signal that no valid target exists
                }

                return members[rnd.Next(members.Count)];
            }
            else // Friendly target (other enemies)
            {
                var enemies = new List<string>();

                foreach (Border border in spActiveEnemies.Children)
                {
                    if (border.Child is StackPanel cardStack)
                    {
                        // The first child in cardStack is a TextBlock with the enemy name
                        if (cardStack.Children[0] is TextBlock nameTextBlock)
                        {
                            var enemyName = nameTextBlock.Text;
                            if (enemyName != excludeName)
                            {
                                enemies.Add(enemyName);
                            }
                        }
                    }
                }

                if (enemies.Count == 0) return excludeName; // No other enemies, target self
                return enemies[rnd.Next(enemies.Count)];
            }
        }

        private void DefeatEnemy(Unit unit, Border enemyCard)
        {
            spActiveEnemies.Children.Remove(enemyCard);

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

        private void AppendCombatLog(string text, Brush color = null)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0) };
            var run = new Run(text + "\n")
            {
                Foreground = color ?? (Brush)FindResource("LogDefault")
            };
            paragraph.Inlines.Add(run);
            rtbCombatLog.Document.Blocks.Add(paragraph);
            rtbCombatLog.ScrollToEnd();

            _logService.Add(text); // still save plain text to db
        }


        #endregion

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logService.Clear();
            rtbCombatLog.Document.Blocks.Clear();
        }
    }
}


public static class StringExtensions
{
    public static bool ContainsAny(this string text, string[] keywords)
    {
        return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
