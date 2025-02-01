using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AemulusModManager
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindowPQ : Window
    {
        private MainWindow main;
        private bool language_handled;

        public ConfigWindowPQ(MainWindow _main)
        {
            main = _main;
            InitializeComponent();

            if (main.modPath != null)
                OutputTextbox.Text = main.modPath;
            if (main.gamePath != null)
                ROMTextbox.Text = main.gamePath;
            if (main.launcherPath != null)
                CitraTextbox.Text = main.launcherPath;
            if (main.titleID != null)
                TitleIDBox.Text = main.titleID;
            else
            {
                main.titleID = "0004000000123400";
                main.config.pqConfig.titleID = "0004000000123400";
                TitleIDBox.Text = main.titleID;
            }
            BuildFinishedBox.IsChecked = main.config.pqConfig.buildFinished;
            BuildWarningBox.IsChecked = main.config.pqConfig.buildWarning;
            ChangelogBox.IsChecked = main.config.pqConfig.updateChangelog;
            DeleteBox.IsChecked = main.config.pqConfig.deleteOldVersions;
            UpdateAllBox.IsChecked = main.config.pqConfig.updateAll;
            UpdateBox.IsChecked = main.config.pqConfig.updatesEnabled;
            HoldenBox.IsChecked = main.config.pqConfig.holdenEnabled;
            if (main.config.pqConfig.holdenEnabled)
            {
                LanguageBox.Visibility = Visibility.Visible;
                TitleIDBox.Visibility = Visibility.Visible;
            }
            else
            {
                LanguageBox.Visibility = Visibility.Collapsed;
                TitleIDBox.Visibility = Visibility.Collapsed;
            }
            if (main.config.pqConfig.holdenEnabled)
            {
                OutputTextblock.Text = "Holden 3DS\nFolder";
            }
            Utilities.ParallelLogger.Log("[INFO] Config launched");
        }
        private void modDirectoryClick(object sender, RoutedEventArgs e)
        {
            var directory = openFolder();
            if (directory != null)
            {
                Utilities.ParallelLogger.Log($"[INFO] Setting output folder to {directory}");
                main.config.pqConfig.modDir = directory;
                main.modPath = directory;
                main.MergeButton.IsHitTestVisible = true;
                main.MergeButton.Foreground = new SolidColorBrush(Color.FromRgb(0xfb, 0x84, 0x6a));
                main.updateConfig();
                OutputTextbox.Text = directory;
            }
        }
        private void BuildWarningChecked(object sender, RoutedEventArgs e)
        {
            main.buildWarning = true;
            main.config.pqConfig.buildWarning = true;
            main.updateConfig();
        }

        private void BuildWarningUnchecked(object sender, RoutedEventArgs e)
        {
            main.buildWarning = false;
            main.config.pqConfig.buildWarning = false;
            main.updateConfig();
        }
        private void BuildFinishedChecked(object sender, RoutedEventArgs e)
        {
            main.buildFinished = true;
            main.config.pqConfig.buildFinished = true;
            main.updateConfig();
        }
        private void BuildFinishedUnchecked(object sender, RoutedEventArgs e)
        {
            main.buildFinished = false;
            main.config.pqConfig.buildFinished = false;
            main.updateConfig();
        }
        private void ChangelogChecked(object sender, RoutedEventArgs e)
        {
            main.updateChangelog = true;
            main.config.pqConfig.updateChangelog = true;
            main.updateConfig();
        }
        private void ChangelogUnchecked(object sender, RoutedEventArgs e)
        {
            main.updateChangelog = false;
            main.config.pqConfig.updateChangelog = false;
            main.updateConfig();
        }
        private void UpdateAllChecked(object sender, RoutedEventArgs e)
        {
            main.updateAll = true;
            main.config.pqConfig.updateAll = true;
            main.updateConfig();
        }
        private void UpdateAllUnchecked(object sender, RoutedEventArgs e)
        {
            main.updateAll = false;
            main.config.pqConfig.updateAll = false;
            main.updateConfig();
        }
        private void UpdateChecked(object sender, RoutedEventArgs e)
        {
            main.updatesEnabled = true;
            main.config.pqConfig.updatesEnabled = true;
            main.updateConfig();
            UpdateAllBox.IsEnabled = true;
        }

        private void UpdateUnchecked(object sender, RoutedEventArgs e)
        {
            main.updatesEnabled = false;
            main.config.pqConfig.updatesEnabled = false;
            main.updateConfig();
            UpdateAllBox.IsChecked = false;
            UpdateAllBox.IsEnabled = false;
        }
        private void DeleteChecked(object sender, RoutedEventArgs e)
        {
            main.deleteOldVersions = true;
            main.config.pqConfig.deleteOldVersions = true;
            main.updateConfig();
        }
        private void DeleteUnchecked(object sender, RoutedEventArgs e)
        {
            main.deleteOldVersions = false;
            main.config.pqConfig.deleteOldVersions = false;
            main.updateConfig();
        }
        private void HoldenChecked(object sender, RoutedEventArgs e)
        {
            main.holdenEnabled = true;
            main.config.pqConfig.holdenEnabled = true;
            main.updateConfig();
            OutputTextblock.Text = "Holden 3DS\nFolder";
            LanguageBox.Visibility = Visibility.Visible;
            TitleIDBox.Visibility = Visibility.Visible;
        }
        private void HoldenUnhecked(object sender, RoutedEventArgs e)
        {
            main.holdenEnabled = false;
            main.config.pqConfig.holdenEnabled = false;
            main.updateConfig();
            OutputTextblock.Text = "Output Folder";
            LanguageBox.Visibility = Visibility.Collapsed;
            TitleIDBox.Visibility = Visibility.Collapsed;
        }

        private void onClose(object sender, CancelEventArgs e)
        {
            Utilities.ParallelLogger.Log("[INFO] Config closed");
        }

        // Used for selecting
        private string openFolder()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.AllowNonFileSystemItems = true;
            openFolder.IsFolderPicker = true;
            openFolder.EnsurePathExists = true;
            openFolder.EnsureValidNames = true;
            openFolder.Multiselect = false;
            openFolder.Title = "Select Output Folder";
            if (openFolder.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openFolder.FileName;
            }

            return null;
        }

        private void SetupROMShortcut(object sender, RoutedEventArgs e)
        {
            string pqRom = selectExe("Select Persona Q ROM", "*.3ds;*.app;*.cxi");
            if (pqRom != null)
            {
                main.gamePath = pqRom;
                main.config.pqConfig.ROMPath = pqRom;
                main.updateConfig();
                ROMTextbox.Text = pqRom;
            }
            else
            {
                Utilities.ParallelLogger.Log("[ERROR] No ROM selected.");
            }
        }

        private void SetupCitraShortcut(object sender, RoutedEventArgs e)
        {
            string[] ctrEmus = {"citra-qt.exe", "lime-qt.exe", "lime3ds-gui.exe" };

            string citraExe = selectExe("Select citra-qt.exe", "*.exe");
            if (ctrEmus.Contains(Path.GetFileName(citraExe).ToLowerInvariant()))
            {
                main.launcherPath = citraExe;
                main.config.pqConfig.launcherPath = citraExe;
                main.updateConfig();
                CitraTextbox.Text = citraExe;
            }
            else
            {
                Utilities.ParallelLogger.Log("[ERROR] Invalid exe.");
            }
        }

        private string selectExe(string title, string extension)
        {
            string type = "Application";
            if (extension == "*.3ds;*.app;*.cxi")
                type = "ROM";
            if (extension == "*.cpk")
                type = "File Container";
            var openExe = new CommonOpenFileDialog();
            openExe.Filters.Add(new CommonFileDialogFilter(type, extension));
            openExe.EnsurePathExists = true;
            openExe.EnsureValidNames = true;
            openExe.Title = title;
            if (openExe.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openExe.FileName;
            }
            return null;
        }

        private async void UnpackPacsClick(object sender, RoutedEventArgs e)
        {
            string selectedPath = selectExe("Select PQ data.cpk to unpack", ".cpk");
            if (selectedPath == null)
            {
                Utilities.ParallelLogger.Log("[ERROR] Incorrect file chosen for unpacking.");
                return;
            }
            main.ModGrid.IsHitTestVisible = false;
            UnpackButton.IsHitTestVisible = false;
            foreach (var button in main.buttons)
            {
                button.IsHitTestVisible = false;
                button.Foreground = new SolidColorBrush(Colors.Gray);
            }
            main.GameBox.IsHitTestVisible = false;
            await main.pacUnpack(selectedPath);
            UnpackButton.IsHitTestVisible = true;
        }

        // Stops the user from changing the displayed "Notifications" text to the names of one of the combo boxes
        private void NotifBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            NotifBox.SelectedIndex = 0;
        }
        private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            language_handled = true;
        }

        private void LanguageBox_DropDownClosed(object sender, EventArgs e)
        {
            if (language_handled)
            {
                var language = (LanguageBox.SelectedValue as ComboBoxItem).Content as String;
                if (language.Contains("North America"))
                {
                    language = "0004000000123400";
                }
                else if (language.Contains("Europe"))
                {
                    language = "0004000000149F00";
                }
                else if (language.Contains("Japan"))
                {
                    language = "00040000000C3600";
                }
                if (main.config.pqConfig.titleID != language)
                {
                    Utilities.ParallelLogger.Log($"[INFO] TitleID changed to {language}");
                    main.config.pqConfig.titleID = language;
                    main.updateConfig();
                }
                TitleIDBox.Text = language;
                language_handled = false;
            }
        }
    }
}
