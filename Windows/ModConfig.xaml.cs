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
using System.Data;


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
        List<StoredPage> storedpages = new List<StoredPage>();

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
        public class StoredPage
        {
            public string optionname1 { get; set; }
            public int optionnum1 { get; set; }
            public string description1 { get; set; }
            public string previewpath1 { get; set; }
            public string[] choice1 { get; set; }
            public StoredPage(string optionname, int optionnum, string description, string previewpath, string[] choice)
            {
                optionname1 = optionname;
                optionnum1 = optionnum;
                description1 = description;
                previewpath1 = previewpath;
                choice1 = choice;
            }
        }
        private void OptionNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OptionNameBox.Text))
            {
                CreateButton.IsEnabled = true;
                NextButton.IsEnabled = true;
            }
            else
            {
                CreateButton.IsEnabled = false;
                NextButton.IsEnabled = false;
            }
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
            Height = 350;
            configpage = configpage + 1;
            PageBox.Text = $"Option {configpage}";
            Utilities.ParallelLogger.Log($@"[DEBUG] configpage = {configpage}");
            if (configpage > 1)
                BackButton.IsEnabled = true;
            
            if (choicenumber < 3)
                choicenumber = 3;
            string[] choice = new string[choicenumber];
            for (int i = 0; i < choiceTextBoxes.Count; i++)
            {
                choice[i] = choiceTextBoxes[i].Text;
                if (string.IsNullOrEmpty(choice[i]))
                {
                    choicenumber = i - 1;
                    break;
                }
            }
            int index = configpage - 1;
            if (index <= 0)
                index = 1;
            if (index - 1 >= 0 && index - 1 < storedpages.Count)
                storedpages[index - 1] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice);
            else
                storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice));
            if (index + 1 >= 0 && index < storedpages.Count)
            {
                StoredPage nextpage = storedpages[index];
                OptionNameBox.Text = nextpage.optionname1;
                DescBox.Text = nextpage.description1;
                PreviewBox.Text = nextpage.previewpath1;
                // Debug
                Utilities.ParallelLogger.Log($"[DEBUG] choiceTextBoxes.Count = {choiceTextBoxes.Count}, nextpage.optionnum1 = {nextpage.optionnum1}");

                choiceTextBoxes.Clear();
                TextBoxContainer.Children.Clear();
                choiceTextBoxes.Add(Choice1Box);
                choiceTextBoxes.Add(Choice2Box);
                if (nextpage.optionnum1 >= 2)
                {
                    choicenumber = nextpage.optionnum1;
                    for (int i = 3; i < nextpage.optionnum1; i++)
                    {
                        System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                        {
                            Name = "Choice" + i + "Box",
                            Text = nextpage.choice1[i - 1],
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
                    }
                    for (int i = 0; i < nextpage.optionnum1 && i < nextpage.choice1.Length && i < choiceTextBoxes.Count; i++)
                    {
                        choiceTextBoxes[i].Text = nextpage.choice1[i];
                    }
                    if (nextpage.optionnum1 < choiceTextBoxes.Count)
                    {
                        for (int ii = nextpage.optionnum1; ii < choiceTextBoxes.Count && ii < choiceTextBoxes.Count; ii++)
                        {
                            choiceTextBoxes[ii].Text = "";
                        }
                    }
                }
            }
            else
            {
                choiceTextBoxes.Clear();
                TextBoxContainer.Children.Clear();
                choiceTextBoxes.Add(Choice1Box);
                choiceTextBoxes.Add(Choice2Box);
                OptionNameBox.Text = "";
                DescBox.Text = "";
                PreviewBox.Text = "";
                for (int i = 0; i < choiceTextBoxes.Count; i++)
                {
                    choiceTextBoxes[i].Text = "";
                }
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (configpage == 1)
            {
                configpage = 1;
                BackButton.IsEnabled = false;
            }
            else
            {
                Height = 350;
                configpage = configpage - 1;
                if (configpage == 1)
                    BackButton.IsEnabled = false;
                PageBox.Text = $"Option {configpage}";
                Utilities.ParallelLogger.Log($@"[DEBUG] configpage = {configpage}");
                if (choicenumber < 2)
                    choicenumber = 2;
                int index = configpage + 1;
                if (OptionNameBox.Text != null)
                {
                    string[] choice = new string[choicenumber];
                    for (int i = 0; i < choiceTextBoxes.Count; i++)
                    {
                        choice[i] = choiceTextBoxes[i].Text;
                        if (string.IsNullOrEmpty(choice[i]))
                        {
                            choicenumber = i - 1;
                            break;
                        }
                    }
                    if (index >= 0 && index < storedpages.Count)
                        storedpages[index] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice);
                    else
                        storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice));
                }
                if (index - 2 >= 0 && index - 2 < storedpages.Count)
                {
                    StoredPage prevpage = storedpages[index - 2];
                    OptionNameBox.Text = prevpage.optionname1;
                    DescBox.Text = prevpage.description1;
                    PreviewBox.Text = prevpage.previewpath1;
                    // Debug
                    Utilities.ParallelLogger.Log($"[DEBUG] choiceTextBoxes.Count = {choiceTextBoxes.Count}, prevpage.optionnum1 = {prevpage.optionnum1}");

                    choiceTextBoxes.Clear();
                    TextBoxContainer.Children.Clear();
                    choiceTextBoxes.Add(Choice1Box);
                    choiceTextBoxes.Add(Choice2Box);
                    if (prevpage.optionnum1 >= 2)
                    {
                        choicenumber = prevpage.optionnum1;
                        for (int i = 3; i < prevpage.optionnum1; i++)
                        {
                            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                            {
                                Name = "Choice" + i + "Box",
                                Text = prevpage.choice1[i - 1],
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
                        }
                        for (int i = 0; i < prevpage.optionnum1 && i < prevpage.choice1.Length && i < choiceTextBoxes.Count; i++)
                        {
                            choiceTextBoxes[i].Text = prevpage.choice1[i];
                        }
                        if (prevpage.optionnum1 < choiceTextBoxes.Count)
                        {
                            for (int ii = prevpage.optionnum1; ii < choiceTextBoxes.Count && ii < choiceTextBoxes.Count; ii++)
                            {
                                choiceTextBoxes[ii].Text = "";
                            }
                        }
                    }
                }
                else
                {
                    choiceTextBoxes.Clear();
                    TextBoxContainer.Children.Clear();
                    choiceTextBoxes.Add(Choice1Box);
                    choiceTextBoxes.Add(Choice2Box);
                    OptionNameBox.Text = "";
                    DescBox.Text = "";
                    PreviewBox.Text = "";
                    for (int i = 0; i < choiceTextBoxes.Count; i++)
                    {
                        choiceTextBoxes[i].Text = "";
                    }
                }
            }
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
