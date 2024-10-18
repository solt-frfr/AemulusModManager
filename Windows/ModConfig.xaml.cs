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
using System.Runtime.CompilerServices;


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

        public class StoredPage
        {
            public string optionname { get; set; }
            public int optionnum { get; set; }
            public string description { get; set; }
            [JsonIgnore]
            public string previewpath { get; set; }
            public string[] choice { get; set; }
            public int type { get; set; }
            public StoredPage() { }
            public StoredPage(string optionname1, int optionnum1, string description1, string previewpath1, string[] choice1, int type1)
            {
                this.optionname = optionname1;
                this.optionnum = optionnum1;
                this.description = description1;
                this.previewpath = previewpath1;
                this.choice = choice1;
                this.type = type1;
            }
        }
        public ModConfig(ConfigMetadata mm)
        {
            InitializeComponent();
            configpage = 1;
            choicenumber = 2;
            choiceTextBoxes.Add(Choice1Box);
            choiceTextBoxes.Add(Choice2Box);
            PageBox.Text = $"Page {configpage}";
            Height = 383;
            TypeBox.SelectedIndex = 0;
            TypeDescBox.Text = "Only one option can be chosen";
            if (mm != null)
            {
                cfgmetadata = mm;
                Title = $"Edit {mm.name} Configuration Options";
            }
            // There's a lot of code in here but the first page has to be set up within this same area or it won't show up at first.
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}";
            string filepath = path + $@"\config.json";
            if (File.Exists(filepath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true // I'm not sure if I need this for reading it but might as well play it safe.
                };
                string jsonString = File.ReadAllText(filepath);
                // Set the first page up from JSON
                storedpages = JsonSerializer.Deserialize<List<StoredPage>>(jsonString, jsonoptions);
                OptionNameBox.Text = storedpages[0].optionname;
                DescBox.Text = storedpages[0].description;
                PreviewBox.Text = storedpages[0].previewpath;
                if (storedpages[0].optionnum <= 2)
                    storedpages[0].optionnum = 2;
                choicenumber = storedpages[0].optionnum;
                TypeBox.SelectedIndex = storedpages[0].type;
                if (TypeDescBox != null)
                {
                    if (TypeBox.SelectedIndex == 0)
                    {
                        TypeDescBox.Text = "Only one option can be chosen";
                    }
                    else if (TypeBox.SelectedIndex == 1)
                    {
                        TypeDescBox.Text = "Multiple options can be chosen";
                    }
                    else if (TypeBox.SelectedIndex == 2)
                    {
                        TypeDescBox.Text = "One option can be chosen from a dropdown";
                    }
                }
                if (storedpages[0].optionnum >= 3) // If there was more than two options, add more textboxes.
                {
                    for (int i = 3; i <= storedpages[0].optionnum; i++) // Starts at 3 because 1 and 2 already exist
                    {
                        System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                        {
                            Name = "Choice" + i + "Box",
                            Text = storedpages[0].choice[i - 1], // These are stored starting at 0, so I have to use the one before it
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
                }
                for (int i = 0; i < storedpages[0].optionnum && i < storedpages[0].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textboxes
                {
                    choiceTextBoxes[i].Text = storedpages[0].choice[i];
                }
            }
        }
        private void OptionNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OptionNameBox.Text))
            {
                CreateButton.IsEnabled = true;
                NextButton.IsEnabled = true;
                PreviewButton.IsEnabled = true;
                PreviewBox.IsEnabled = true;
                TypeBox.IsEnabled = true;
                DescBox.IsEnabled = true;
                Choice1Box.IsEnabled = true;
                Choice2Box.IsEnabled = true;
                AddChoiceButton.IsEnabled = true;
            }
            else
            {
                CreateButton.IsEnabled = false;
                NextButton.IsEnabled = false;
                PreviewButton.IsEnabled = false;
                PreviewBox.IsEnabled = false;
                TypeBox.IsEnabled = false;
                DescBox.IsEnabled = false;
                Choice1Box.IsEnabled = false;
                Choice2Box.IsEnabled = false;
                AddChoiceButton.IsEnabled = false;
            }
        }
        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeDescBox != null)
            {
                if (TypeBox.SelectedIndex == 0)
                {
                    TypeDescBox.Text = "Only one option can be chosen";
                }
                else if (TypeBox.SelectedIndex == 1)
                {
                    TypeDescBox.Text = "Multiple options can be chosen";
                }
                else if (TypeBox.SelectedIndex == 2)
                {
                    TypeDescBox.Text = "One option can be chosen from a dropdown";
                }
            }
        }

        private void AddChoiceButton_Click(object sender, RoutedEventArgs e)
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
            // My code is kinda messy so I'll do my best to comment on what I'm pretty sure it does   -Solt11
            Height = 383; // Reset the hight to the default
            configpage = configpage + 1; // Set the page variable to the next
            PageBox.Text = $"Page {configpage}";
            if (configpage > 1) // Allows back button if the page isn't the first page (it shouldn't be but its always best to ensure)
                BackButton.IsEnabled = true;
            if (choicenumber < 2) // Guarentees essentially that the number is valid before moving on, it'll get overwritten shortly
                choicenumber = 2;
            string[] choice = new string[choicenumber];
            choicenumber = choiceTextBoxes.Count;
            for (int i = 0; i <= choicenumber; i++) // Makes sure the textboxes contain data, if not, only the ones that do will be saved.
            {
                try
                {
                    if (string.IsNullOrEmpty(choiceTextBoxes[i].Text))
                    {
                        choicenumber = i;
                        break;
                    }
                }
                catch
                {
                    choicenumber = i;
                }
            }
            for (int i = 0; i <= choicenumber - 1; i++)
            {
                choice[i] = choiceTextBoxes[i].Text;
            }
            int index = configpage - 1; // Local variable that I can use. Converts the current page to a number starting at 0 instead of 1.
            if (index <= 0) // Double checks for validity because I've had so many problems ;m;
                index = 0;
            if (index - 1 >= 0 && index - 1 < storedpages.Count) // Checks to see if the page (before clicking next) is already stored somewhere.
                storedpages[index - 1] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex); // If yes, overwrite it.
            else
                storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex)); // If not, store and add it.
            choiceTextBoxes.Clear(); // Since it's now been stored, clear everything.
            TextBoxContainer.Children.Clear();
            choiceTextBoxes.Add(Choice1Box);
            choiceTextBoxes.Add(Choice2Box);
            if (index >= 0 && index < storedpages.Count) // If the data for this page exists, read it and set it to the text boxes.
            {
                StoredPage nextpage = storedpages[index];
                OptionNameBox.Text = nextpage.optionname;
                DescBox.Text = nextpage.description;
                PreviewBox.Text = nextpage.previewpath;
                if (nextpage.optionnum <= 2)
                    nextpage.optionnum = 2;
                choicenumber = nextpage.optionnum;
                TypeBox.SelectedIndex = nextpage.type;
                if (TypeDescBox != null)
                {
                    if (TypeBox.SelectedIndex == 0)
                    {
                        TypeDescBox.Text = "Only one option can be chosen";
                    }
                    else if (TypeBox.SelectedIndex == 1)
                    {
                        TypeDescBox.Text = "Multiple options can be chosen";
                    }
                    else if (TypeBox.SelectedIndex == 2)
                    {
                        TypeDescBox.Text = "One option can be chosen from a dropdown";
                    }
                }
                if (nextpage.optionnum >= 3) // If there was more than two options, add more textboxes.
                {
                    for (int i = 3; i <= nextpage.optionnum; i++) // Starts at 3 because 1 and 2 already exist
                    {
                        System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                        {
                            Name = "Choice" + i + "Box",
                            Text = nextpage.choice[i - 1], // These are stored starting at 0, so I have to use the one before it
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
                }
                for (int i = 0; i < nextpage.optionnum && i < nextpage.choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textboxes
                {
                    choiceTextBoxes[i].Text = nextpage.choice[i];
                }
            }
            else // Otherwise, clear everything and set up a blank page.
            {
                OptionNameBox.Text = "";
                DescBox.Text = "";
                PreviewBox.Text = "";
                for (int i = 0; i < choiceTextBoxes.Count; i++)
                {
                    choiceTextBoxes[i].Text = "";
                }
                TypeBox.SelectedIndex = 0;
                TypeDescBox.Text = "Only one option can be chosen";
                CreateButton.IsEnabled = false;
                NextButton.IsEnabled = false;
                PreviewButton.IsEnabled = false;
                PreviewBox.IsEnabled = false;
                TypeBox.IsEnabled = false;
                DescBox.IsEnabled = false;
                Choice1Box.IsEnabled = false;
                Choice2Box.IsEnabled = false;
                AddChoiceButton.IsEnabled = false;
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (configpage == 1) // It shouldn't be possible to click the back button on page one, but this is a failsafe.
            {
                configpage = 1;
                BackButton.IsEnabled = false;
            }
            else
            {
                Height = 383; // Reset the hight to the default
                configpage = configpage - 1; // Set the page variable to the previous
                if (configpage == 1) // Disable the back button if the page is now the first page
                    BackButton.IsEnabled = false;
                PageBox.Text = $"Page {configpage}";
                int index = configpage - 1; // Local variable that I can use. Converts the current page to a number starting at 0 instead of 1.
                if (index <= 0) // Double checks for validity because, again, I've had so many problems
                    index = 0;
                if (!string.IsNullOrWhiteSpace(OptionNameBox.Text))
                {
                    if (choicenumber < 2) // Guarentees essentially that the number is valid before moving on, it'll get overwritten shortly
                        choicenumber = 2;
                    string[] choice = new string[choicenumber];
                    choicenumber = choiceTextBoxes.Count;
                    for (int i = 0; i <= choicenumber; i++) // Makes sure the textboxes contain data, if not, only the ones that do will be saved.
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(choiceTextBoxes[i].Text))
                            {
                                choicenumber = i;
                                break;
                            }
                        }
                        catch
                        {
                            choicenumber = i;
                        }
                    }
                    for (int i = 0; i <= choicenumber - 1; i++)
                    {
                        choice[i] = choiceTextBoxes[i].Text;
                    }
                    if (index + 1 >= 0 && index + 1 < storedpages.Count) // Checks to see if the page (before clicking back) is already stored somewhere.
                        storedpages[index + 1] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex); // If yes, overwrite it.
                    else
                        storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex)); // If not, store and add it.
                }
                choiceTextBoxes.Clear();
                TextBoxContainer.Children.Clear();
                choiceTextBoxes.Add(Choice1Box);
                choiceTextBoxes.Add(Choice2Box);
                if (index >= 0 && index < storedpages.Count) // If the data for this page exists, read it and set it to the text boxes.
                {
                    StoredPage prevpage = storedpages[index];
                    OptionNameBox.Text = prevpage.optionname;
                    DescBox.Text = prevpage.description;
                    PreviewBox.Text = prevpage.previewpath;
                    if (prevpage.optionnum <= 2)
                        prevpage.optionnum = 2;
                    choicenumber = prevpage.optionnum;
                    TypeBox.SelectedIndex = prevpage.type;
                    if (TypeDescBox != null)
                    {
                        if (TypeBox.SelectedIndex == 0)
                        {
                            TypeDescBox.Text = "Only one option can be chosen";
                        }
                        else if (TypeBox.SelectedIndex == 1)
                        {
                            TypeDescBox.Text = "Multiple options can be chosen";
                        }
                        else if (TypeBox.SelectedIndex == 2)
                        {
                            TypeDescBox.Text = "One option can be chosen from a dropdown";
                        }
                    }
                    if (prevpage.optionnum >= 3)
                    {
                        for (int i = 3; i <= prevpage.optionnum; i++) // Starts at 3 because 1 and 2 already exist
                        {
                            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                            {
                                Name = "Choice" + i + "Box",
                                Text = prevpage.choice[i - 1], // These are stored starting at 0, so I have to use the one before it
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
                    }
                    for (int i = 0; i < prevpage.optionnum && i < prevpage.choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textboxes
                    {
                        choiceTextBoxes[i].Text = prevpage.choice[i];
                    }
                }
                else // Otherwise, clear everything and set up a blank page.
                {
                    OptionNameBox.Text = "";
                    DescBox.Text = "";
                    PreviewBox.Text = "";
                    for (int i = 0; i < choiceTextBoxes.Count; i++)
                    {
                        choiceTextBoxes[i].Text = "";
                    }
                    TypeBox.SelectedIndex = 0;
                    TypeDescBox.Text = "Only one option can be chosen";
                    CreateButton.IsEnabled = false;
                    NextButton.IsEnabled = false;
                    PreviewButton.IsEnabled = false;
                    PreviewBox.IsEnabled = false;
                    TypeBox.IsEnabled = false;
                    DescBox.IsEnabled = false;
                    Choice1Box.IsEnabled = false;
                    Choice2Box.IsEnabled = false;
                    AddChoiceButton.IsEnabled = false;
                }
            }
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Stores the config data on the current page to ensure it's up to date
            if (choicenumber < 2) // Guarentees essentially that the number is valid before moving on, it'll get overwritten shortly
                choicenumber = 2;
            string[] choice = new string[choicenumber];
            choicenumber = choiceTextBoxes.Count;
            for (int i = 0; i <= choicenumber; i++) // Makes sure the textboxes contain data, if not, only the ones that do will be saved.
            {
                try
                {
                    if (string.IsNullOrEmpty(choiceTextBoxes[i].Text))
                    {
                        choicenumber = i;
                        break;
                    }
                }
                catch
                {
                    choicenumber = i;
                }
            }
            for (int i = 0; i <= choicenumber - 1; i++)
                {
                    choice[i] = choiceTextBoxes[i].Text;
                }
            int index = configpage - 1; // Local variable that I can use. Converts the current page to a number starting at 0 instead of 1.
            if (index <= 0) // Double checks for validity because I've had so many problems ;m;
                index = 1;
            if (index >= 0 && index < storedpages.Count) // Checks to see if the page (before clicking next) is already stored somewhere.
                storedpages[index] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex); // If yes, overwrite it.
            else
                storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex)); // If not, store and add it.

            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}";
            // Even if the path was set correctly, it for whatever reason can't find it. Instead, it checks for common errors.
            if (path != $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\\") // Ensures the game and mod folder for the mod were found
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(storedpages, jsonoptions);
                string filepath = path + $@"\config.json";
                File.WriteAllText(filepath, jsonString);
                Utilities.ParallelLogger.Log($"[INFO] config.json written to {filepath}.");
                for (int i = 0; i < storedpages.Count; i++)
                {
                    if (File.Exists(storedpages[i].previewpath))
                        System.IO.File.Copy(storedpages[i].previewpath, path + $@"\Preview" + (i + 1) + ".png", true);
                }
                Close();
            }
            else if (string.IsNullOrEmpty(path)) // If the previous check failed, check to see if it was completely blank
            {
                Utilities.ParallelLogger.Log($"[ERROR] The path was not set.");
                Close();
            }
            else // If anything else happened, it's likely that the game and mod folder couldn't be found
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
