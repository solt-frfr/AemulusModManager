using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;


namespace AemulusModManager
{
    /// <summary>
    /// Interaction logic for ModConfig.xaml
    /// </summary>
    public partial class ModConfig : Window
    {
        public ConfigMetadata cfgmetadata;
        public int configpage { get; set; }
        public int choicenumber { get; set; }
        public string thumbnailPath;
        private List<System.Windows.Controls.TextBox> choiceTextBoxes = new List<System.Windows.Controls.TextBox>();

        public ModConfig(ConfigMetadata mm)
        {
            InitializeComponent();
            configpage = 1;
            choicenumber = 2;
            choiceTextBoxes.Add(Choice1Box);
            choiceTextBoxes.Add(Choice2Box);
            PageBox.Text = $"Page {configpage}";
            Height = 350;
            if (mm != null)
            {
                cfgmetadata = mm;
                Title = $"Edit {mm.name} Configuration Options";
            }
        }
        private void OptionNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OptionNameBox.Text))
                CreateButton.IsEnabled = true;
            else
                CreateButton.IsEnabled = false;
        }
        private void AddChoiceBox_Click(object sender, RoutedEventArgs e)
        {
            if (choicenumber < 3)
                choicenumber = 3;
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "Choice" + choicenumber + "Box",
                Width = 365,
                Height = 17,
                Margin = new Thickness(0, 9, 0, 9),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020"),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#424242"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#f2f2f2"),
            };
            TextBoxContainer.Children.Add(newTextBox);
            choiceTextBoxes.Add(newTextBox);
            Height = Height + 35;
            choicenumber = choicenumber + 1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            configpage = configpage + 1;
            PageBox.Text = $"Page {configpage}";
            Utilities.ParallelLogger.Log($@"[DEBUG] configpage = {configpage}");
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}";
            if (path != $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\\")
            {
                if (choicenumber < 3) // Double check that choicenumber was set
                    choicenumber = 3;
                string[] choice = new string[choicenumber];
                for (int i = 0; i < choiceTextBoxes.Count; i++)
                {
                    choice[i] = choiceTextBoxes[i].Text;
                    if (string.IsNullOrEmpty(choice[i]))
                    {
                        choicenumber = i - 1;
                        if (choicenumber < 2)
                        {
                            Utilities.ParallelLogger.Log($"[ERROR] You need at least two valid options.");
                            Close();
                            return;
                        }
                        break;
                    }
                }
                Utilities.ParallelLogger.Log($@"[INFO] Config file written to [{path}\config.json].");
                Utilities.ParallelLogger.Log($@"[DEBUG] Not really tho, listing variables:");
                Utilities.ParallelLogger.Log($@"[DEBUG] Option Name = {OptionNameBox.Text}");
                Utilities.ParallelLogger.Log($@"[DEBUG] Description = {DescBox.Text}");
                Utilities.ParallelLogger.Log($@"[DEBUG] Preview Path = {PreviewBox.Text}");
                for (int i = 0; i < choicenumber - 1; i++)
                {
                    Utilities.ParallelLogger.Log($@"[DEBUG] choice[{i}] = {choice[i]}");
                }
                Close();
            }
            else if (string.IsNullOrEmpty(path))
            {
                Utilities.ParallelLogger.Log($"[ERROR] The path was not set.");
                Close();
            }
            else
            {
                Utilities.ParallelLogger.Log($"[ERROR] Failed to grab the mod metadata.");
                Close();
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var openPng = new CommonOpenFileDialog();
            openPng.Filters.Add(new CommonFileDialogFilter("Preview", "*.*"));
            openPng.EnsurePathExists = true;
            openPng.EnsureValidNames = true;
            openPng.Title = "Select Preview";
            if (openPng.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PreviewBox.Text = openPng.FileName;
                thumbnailPath = openPng.FileName;
            }
            // Bring Mod Config window back to foreground after closing dialog
            this.Activate();
        }
    }
}
