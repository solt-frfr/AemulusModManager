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
            Height = 383;
            TypeBox.SelectedIndex = 0;
            TypeDescBox.Text = "Only one option can be chosen";
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
            public int type1 { get; set; }
            public StoredPage(string optionname, int optionnum, string description, string previewpath, string[] choice, int type)
            {
                optionname1 = optionname;
                optionnum1 = optionnum;
                description1 = description;
                previewpath1 = previewpath;
                choice1 = choice;
                type1 = type;
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
            // My code is kinda messy so I'll do my best to comment on what I'm pretty sure it does   -Solt11
            Height = 383; // Reset the hight to the default
            configpage = configpage + 1; // Set the page variable to the next
            PageBox.Text = $"Option {configpage}";
            Utilities.ParallelLogger.Log($@"[DEBUG] configpage = {configpage}");
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
            Utilities.ParallelLogger.Log($@"[DEBUG] {configpage - 1} was reported to have {choicenumber} choices.");
            Utilities.ParallelLogger.Log($@"[DEBUG] The type dropdown was reported to have index {TypeBox.SelectedIndex}.");
            int index = configpage - 1; // Local variable that I can use. Converts the current page to a number starting at 0 instead of 1.
            if (index <= 0) // Double checks for validity because I've had so many problems ;m;
                index = 0;
            if (index - 1 >= 0 && index - 1 < storedpages.Count) // Checks to see if the page (before clicking next) is already stored somewhere.
                storedpages[index - 1] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex); // If yes, overwrite it.
            else
                storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex)); // If not, store and add it.
            Utilities.ParallelLogger.Log($"[DEBUG] Stored to index {index - 1}");
            choiceTextBoxes.Clear(); // Since it's now been stored, clear everything.
            TextBoxContainer.Children.Clear();
            choiceTextBoxes.Add(Choice1Box);
            choiceTextBoxes.Add(Choice2Box);
            if (index >= 0 && index < storedpages.Count) // If the data for this page exists, read it and set it to the text boxes.
            {
                StoredPage nextpage = storedpages[index];
                OptionNameBox.Text = nextpage.optionname1;
                DescBox.Text = nextpage.description1;
                PreviewBox.Text = nextpage.previewpath1;
                if (nextpage.optionnum1 <= 2)
                    nextpage.optionnum1 = 2;
                choicenumber = nextpage.optionnum1;
                TypeBox.SelectedIndex = nextpage.type1;
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
                // Debug
                Utilities.ParallelLogger.Log($"[DEBUG] choiceTextBoxes.Count = {choiceTextBoxes.Count}, nextpage.optionnum1 = {nextpage.optionnum1}, nextpage pulled from index {index}");

                if (nextpage.optionnum1 >= 3) // If there was more than two options, add more textboxes.
                {
                    for (int i = 3; i <= nextpage.optionnum1; i++) // Starts at 3 because 1 and 2 already exist
                    {
                        System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                        {
                            Name = "Choice" + i + "Box",
                            Text = nextpage.choice1[i - 1], // These are stored starting at 0, so I have to use the one before it
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
                for (int i = 0; i < nextpage.optionnum1 && i < nextpage.choice1.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textboxes
                {
                    choiceTextBoxes[i].Text = nextpage.choice1[i];
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
                PageBox.Text = $"Option {configpage}";
                Utilities.ParallelLogger.Log($@"[DEBUG] configpage = {configpage}");
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
                Utilities.ParallelLogger.Log($@"[DEBUG] {configpage + 1} was reported to have {choicenumber} choices.");
                Utilities.ParallelLogger.Log($@"[DEBUG] The type dropdown was reported to have index {TypeBox.SelectedIndex}.");
                int index = configpage - 1; // Local variable that I can use. Converts the current page to a number starting at 0 instead of 1.
                if (index <= 0) // Double checks for validity because, again, I've had so many problems
                    index = 0;
                if (index + 1 >= 0 && index + 1 < storedpages.Count) // Checks to see if the page (before clicking back) is already stored somewhere.
                    storedpages[index + 1] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex); // If yes, overwrite it.
                else
                    storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex)); // If not, store and add it.
                Utilities.ParallelLogger.Log($"[DEBUG] Stored to index {index + 1}");
                choiceTextBoxes.Clear();
                TextBoxContainer.Children.Clear();
                choiceTextBoxes.Add(Choice1Box);
                choiceTextBoxes.Add(Choice2Box);
                if (index >= 0 && index < storedpages.Count) // If the data for this page exists, read it and set it to the text boxes.
                {
                    StoredPage prevpage = storedpages[index];
                    OptionNameBox.Text = prevpage.optionname1;
                    DescBox.Text = prevpage.description1;
                    PreviewBox.Text = prevpage.previewpath1;
                    if (prevpage.optionnum1 <= 2)
                        prevpage.optionnum1 = 2;
                    choicenumber = prevpage.optionnum1;
                    TypeBox.SelectedIndex = prevpage.type1;
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
                    // Debug
                    Utilities.ParallelLogger.Log($"[DEBUG] choiceTextBoxes.Count = {choiceTextBoxes.Count}, prevpage.optionnum1 = {prevpage.optionnum1}, prevpage pulled from index {index}");


                    if (prevpage.optionnum1 >= 3)
                    {
                        for (int i = 3; i <= prevpage.optionnum1; i++) // Starts at 3 because 1 and 2 already exist
                        {
                            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                            {
                                Name = "Choice" + i + "Box",
                                Text = prevpage.choice1[i - 1], // These are stored starting at 0, so I have to use the one before it
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
                    for (int i = 0; i < prevpage.optionnum1 && i < prevpage.choice1.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textboxes
                    {
                        choiceTextBoxes[i].Text = prevpage.choice1[i];
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
            Utilities.ParallelLogger.Log($@"[DEBUG] {configpage} was reported to have {choicenumber} choices.");
            Utilities.ParallelLogger.Log($@"[DEBUG] The type dropdown was reported to have index {TypeBox.SelectedIndex}.");
            int index = configpage - 1; // Local variable that I can use. Converts the current page to a number starting at 0 instead of 1.
            if (index <= 0) // Double checks for validity because I've had so many problems ;m;
                index = 1;
            if (index >= 0 && index < storedpages.Count) // Checks to see if the page (before clicking next) is already stored somewhere.
                storedpages[index] = new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex); // If yes, overwrite it.
            else
                storedpages.Add(new StoredPage(OptionNameBox.Text, choicenumber, DescBox.Text, PreviewBox.Text, choice, TypeBox.SelectedIndex)); // If not, store and add it.
            Utilities.ParallelLogger.Log($"[DEBUG] Stored to index {index}");

            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}";
            // Even if the path was set correctly, it for whatever reason can't find it. Instead, it checks for common errors.
            if (path != $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\\") // Ensures the game and mod folder for the mod were found
            {
                // This is where I would put my JSON writing code
                // If I had one
                // (I'll get on it once I fix my other errors)
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
