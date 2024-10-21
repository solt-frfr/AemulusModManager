using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using Octokit;
using System.Xml.Linq;

namespace AemulusModManager.Windows
{
    /// <summary>
    /// Interaction logic for UserModConfig.xaml
    /// </summary>
    public partial class UserModConfig : Window
    {
        public ConfigMetadata cfgmetadata;
        public int configpage { get; set; }
        public int index
        {
            get { return configpage - 1; }
        }
        public int choicenumber { get; set; }
        public string thumbnailPath;
        private List<System.Windows.Controls.TextBlock> choiceTextBoxes = new List<System.Windows.Controls.TextBlock>();
        private List<System.Windows.Controls.CheckBox> checkBoxes = new List<System.Windows.Controls.CheckBox>();
        private List<System.Windows.Controls.ComboBox> combobox = new List<System.Windows.Controls.ComboBox>();
        List<StoredPage> storedpages = new List<StoredPage>();
        List<List<int>> userchoices = new List<List<int>>();

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
        public UserModConfig(ConfigMetadata mm)
        {
            InitializeComponent();
            if (mm != null)
            {
                cfgmetadata = mm;
                Title = $"Choose {mm.name} Configuration Options";
            }
            BackButton.IsEnabled = false;
            configpage = 1;
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}";
            string jsonpath = path + $@"\config.json";
            string userpath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Config\{cfgmetadata.modgame}\Mods\{cfgmetadata.modid}.json";
            string previewpath = path + $@"\Preview{configpage}.png";
            if (File.Exists(previewpath))
            {
                BitmapImage preview = new BitmapImage();
                preview.BeginInit();
                preview.UriSource = new Uri(previewpath, UriKind.Absolute);
                preview.EndInit();
                Preview.Source = preview;
            }
            if (File.Exists(userpath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true // I'm not sure if I need this for reading it but might as well play it safe.
                };
                string jsonString = File.ReadAllText(userpath);
                userchoices = JsonSerializer.Deserialize<List<List<int>>>(jsonString, jsonoptions);
            }
            if (File.Exists(jsonpath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true // I'm not sure if I need this for reading it but might as well play it safe.
                };
                string jsonString = File.ReadAllText(jsonpath);
                storedpages = JsonSerializer.Deserialize<List<StoredPage>>(jsonString, jsonoptions);
                NameText.Text = $"{storedpages[0].optionname}";
                DescBox.Text = $"{storedpages[0].description}";
                if (storedpages[0].type == 0 || storedpages[0].type == 1) // If the type needs it, add textblocks.
                {
                    for (int i = 1; i <= storedpages[0].optionnum; i++)
                    {
                        System.Windows.Controls.TextBlock newTextBox = new System.Windows.Controls.TextBlock
                        {
                            Name = "Choice" + i + "Box",
                            Text = storedpages[0].choice[i - 1], // These are stored starting at 0, so I have to use the one before it
                            Margin = new Thickness(0, 3, 0, 3),
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#d3d3d3"),
                            FontSize = 15,
                            FontWeight = FontWeights.Bold
                        };
                        TextBoxContainer.Children.Add(newTextBox);
                        CheckBox checkBox = new CheckBox
                        {
                            Name = "Check" + i + "Box",
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            Margin = new Thickness(0, 5.5, 0, 6),
                        };
                        checkBox.Checked += CheckBox_Checked;
                        checkBox.Unchecked += CheckBox_Unchecked;
                        CheckBoxContainer.Children.Add(checkBox);
                        choiceTextBoxes.Add(newTextBox);
                        checkBoxes.Add(checkBox);
                        Height += 23;
                    }
                    for (int i = 0; i <= storedpages[0].optionnum; i++)
                    {
                        if (userchoices.Count > 0 && userchoices[index].Contains(i))
                            checkBoxes[i].IsChecked = true;
                    }
                }
                else if (storedpages[0].type == 2)
                {
                    ComboBox moddrop = new ComboBox();
                    moddrop.Width = 200;
                    moddrop.Height = 30;
                    for (int i = 1; i <= storedpages[0].optionnum; i++)
                    {
                        moddrop.Items.Add($"{storedpages[0].choice[i - 1]}");
                    }
                    moddrop.SelectedIndex = 0; // Select the first item by default
                    DropBoxContainer.Children.Add(moddrop);
                    combobox.Add(moddrop);
                    for (int i = 0; i <= storedpages[index].optionnum; i++)
                    {
                        if (userchoices.Count > index && userchoices[index].Contains(i))
                            combobox[0].SelectedIndex = i;
                    }
                }
                for (int i = 0; i < storedpages[0].optionnum && i < storedpages[0].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textblocks
                {
                    choiceTextBoxes[i].Text = storedpages[0].choice[i];
                }
                if (storedpages[0].type == 0)
                {
                    TypeDescText.Text = "Only one option can be chosen.\r\nUncheck one to select another.";
                }
                else if (storedpages[0].type == 1)
                {
                    TypeDescText.Text = "Multiple options can be chosen.";
                }
                else if (storedpages[0].type == 2)
                {
                    TypeDescText.Text = "One option can be chosen from a dropdown.";
                }
            }
            else
            {
                Utilities.ParallelLogger.Log($"[ERROR] config.json not found. Make one in Edit Mod Configuration [MODDER].");
                Close();
            }
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkedBox = sender as CheckBox;
            if (storedpages[index].type == 0)
            {
                foreach (var checkBox in checkBoxes)
                {
                    if (checkBox != checkedBox)
                    {
                        checkBox.IsEnabled = false;
                    }
                }
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in checkBoxes)
            {
                checkBox.IsEnabled = true;
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (storedpages.Count == configpage || storedpages.Count < configpage)
                NextButton.IsEnabled = false;
            else
            {
                if (storedpages[index].type == 0 || storedpages[index].type == 1)
                {
                    List<int> chosen = new List<int>();
                    for (int i = 0; i < checkBoxes.Count; i++)
                    {
                        if (checkBoxes[i].IsChecked == true)
                        {
                            chosen.Add(i);
                        }
                    };
                    if (index >= 0 && index < userchoices.Count)
                        userchoices[index] = chosen;
                    else
                        userchoices.Add(chosen);
                }
                else if (storedpages[index].type == 2)
                {
                    List<int> chosen = new List<int>();
                    chosen.Add(combobox[0].SelectedIndex);
                    if (index >= 0 && index < userchoices.Count)
                        userchoices[index] = chosen;
                    else
                        userchoices.Add(chosen);

                }
                choiceTextBoxes.Clear();
                checkBoxes.Clear();
                combobox.Clear();
                TextBoxContainer.Children.Clear();
                CheckBoxContainer.Children.Clear();
                DropBoxContainer.Children.Clear();
                BackButton.IsEnabled = true;
                configpage += 1;
                if (storedpages.Count == configpage || storedpages.Count < configpage)
                    NextButton.IsEnabled = false;
                NameText.Text = $"{storedpages[index].optionname}";
                DescBox.Text = $"{storedpages[index].description}";
                string previewpath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}\Preview{configpage}.png";
                BitmapImage preview = new BitmapImage();
                preview.BeginInit();
                preview.UriSource = new Uri(previewpath, UriKind.Absolute);
                preview.EndInit();
                Preview.Source = preview;
                if (storedpages[index].type == 0 || storedpages[index].type == 1) // If the type needs it, add textblocks.
                {
                    for (int i = 1; i <= storedpages[index].optionnum; i++)
                    {
                        System.Windows.Controls.TextBlock newTextBox = new System.Windows.Controls.TextBlock
                        {
                            Name = "Choice" + i + "Box",
                            Text = storedpages[index].choice[i - 1], // These are stored starting at 0, so I have to use the one before it
                            Margin = new Thickness(0, 3, 0, 3),
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#d3d3d3"),
                            FontSize = 15,
                            FontWeight = FontWeights.Bold
                        };
                        TextBoxContainer.Children.Add(newTextBox);
                        CheckBox checkBox = new CheckBox
                        {
                            Name = "Check" + i + "Box",
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            Margin = new Thickness(0, 5.5, 0, 6),
                        };
                        checkBox.Checked += CheckBox_Checked;
                        checkBox.Unchecked += CheckBox_Unchecked;
                        CheckBoxContainer.Children.Add(checkBox);
                        choiceTextBoxes.Add(newTextBox);
                        checkBoxes.Add(checkBox);
                        Height += 23;
                    }
                    for (int i = 0; i <= storedpages[index].optionnum; i++)
                    {
                        if (userchoices.Count > index && userchoices[index].Contains(i))
                            checkBoxes[i].IsChecked = true;
                    }
                }
                else if (storedpages[index].type == 2)
                {
                    ComboBox moddrop = new ComboBox();
                    moddrop.Width = 200;
                    moddrop.Height = 30;
                    for (int i = 1; i <= storedpages[index].optionnum; i++)
                    {
                        moddrop.Items.Add($"{storedpages[index].choice[i - 1]}");
                    }
                    moddrop.SelectedIndex = 0; // Select the first item by default
                    DropBoxContainer.Children.Add(moddrop);
                    combobox.Add(moddrop);
                    for (int i = 0; i <= storedpages[index].optionnum; i++)
                    {
                        if (userchoices.Count > index && userchoices[index].Contains(i))
                            combobox[0].SelectedIndex = i;
                    }
                }
                this.SizeToContent = SizeToContent.WidthAndHeight;
                for (int i = 0; i < storedpages[index].optionnum && i < storedpages[index].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textblocks
                {
                    choiceTextBoxes[i].Text = storedpages[index].choice[i];
                }
                if (storedpages[index].type == 0)
                {
                    TypeDescText.Text = "Only one option can be chosen.\r\nUncheck one to select another.";
                }
                else if (storedpages[index].type == 1)
                {
                    TypeDescText.Text = "Multiple options can be chosen.";
                }
                else if (storedpages[index].type == 2)
                {
                    TypeDescText.Text = "One option can be chosen from a dropdown.";
                }
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (storedpages[index].type == 0 || storedpages[index].type == 1)
            {
                List<int> chosen = new List<int>();
                for (int i = 0; i < checkBoxes.Count; i++)
                {
                    if (checkBoxes[i].IsChecked == true)
                    {
                        chosen.Add(i);
                    }
                };
                if (index >= 0 && index < userchoices.Count)
                    userchoices[index] = chosen;
                else
                    userchoices.Add(chosen);
            }
            else if (storedpages[index].type == 2)
            {
                List<int> chosen = new List<int>();
                chosen.Add(combobox[0].SelectedIndex);
                if (index >= 0 && index < userchoices.Count)
                    userchoices[index] = chosen;
                else
                    userchoices.Add(chosen);

            }
            if (configpage == 1)
                BackButton.IsEnabled = false;
            else
            {
                NextButton.IsEnabled = true;
                choiceTextBoxes.Clear();
                checkBoxes.Clear();
                combobox.Clear();
                TextBoxContainer.Children.Clear();
                CheckBoxContainer.Children.Clear();
                DropBoxContainer.Children.Clear();
                configpage -= 1;
                this.SizeToContent = SizeToContent.WidthAndHeight;
                if (configpage == 1)
                    BackButton.IsEnabled = false;
                NameText.Text = $"{storedpages[index].optionname}";
                DescBox.Text = $"{storedpages[index].description}";
                string previewpath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{cfgmetadata.modgame}\{cfgmetadata.modpath}\Preview{configpage}.png";
                BitmapImage preview = new BitmapImage();
                preview.BeginInit();
                preview.UriSource = new Uri(previewpath, UriKind.Absolute);
                preview.EndInit();
                Preview.Source = preview;
            }
            if (storedpages[index].type == 0 || storedpages[index].type == 1) // If the type needs it, add textblocks.
            {
                for (int i = 1; i <= storedpages[index].optionnum; i++)
                {
                    System.Windows.Controls.TextBlock newTextBox = new System.Windows.Controls.TextBlock
                    {
                        Name = "Choice" + i + "Box",
                        Text = storedpages[index].choice[i - 1], // These are stored starting at 0, so I have to use the one before it
                        Margin = new Thickness(0, 3, 0, 3),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#d3d3d3"),
                        FontSize = 15,
                        FontWeight = FontWeights.Bold
                    };
                    TextBoxContainer.Children.Add(newTextBox);
                    CheckBox checkBox = new CheckBox
                    {
                        Name = "Check" + i + "Box",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        Margin = new Thickness(0, 5.5, 0, 6),
                    };
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += CheckBox_Unchecked;
                    CheckBoxContainer.Children.Add(checkBox);
                    choiceTextBoxes.Add(newTextBox);
                    checkBoxes.Add(checkBox);
                    Height += 23;
                }
                for (int i = 0; i <= storedpages[index].optionnum; i++)
                {
                    if (userchoices.Count > index && userchoices[index].Contains(i))
                        checkBoxes[i].IsChecked = true;
                }
            }
            else if (storedpages[index].type == 2)
            {
                ComboBox moddrop = new ComboBox();
                moddrop.Width = 200;
                moddrop.Height = 30;
                Height += 30;
                for (int i = 1; i <= storedpages[index].optionnum; i++)
                {
                    moddrop.Items.Add($"{storedpages[index].choice[i - 1]}");
                }
                moddrop.SelectedIndex = 0; // Select the first item by default
                DropBoxContainer.Children.Add(moddrop);
                combobox.Add(moddrop);
                for (int i = 0; i <= storedpages[index].optionnum; i++)
                {
                    if (userchoices.Count > index && userchoices[index].Contains(i))
                        combobox[0].SelectedIndex = i;
                }
            }
            this.SizeToContent = SizeToContent.WidthAndHeight;
            for (int i = 0; i < storedpages[index].optionnum && i < storedpages[index].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textblocks
            {
                choiceTextBoxes[i].Text = storedpages[index].choice[i];
            }
            if (storedpages[index].type == 0)
            {
                TypeDescText.Text = "Only one option can be chosen.\r\nUncheck one to select another.";
            }
            else if (storedpages[index].type == 1)
            {
                TypeDescText.Text = "Multiple options can be chosen.";
            }
            else if (storedpages[index].type == 2)
            {
                TypeDescText.Text = "One option can be chosen from a dropdown.";
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (storedpages[index].type == 0 || storedpages[index].type == 1)
            {
                List<int> chosen = new List<int>();
                for (int i = 0; i < checkBoxes.Count; i++)
                {
                    if (checkBoxes[i].IsChecked == true)
                    {
                        chosen.Add(i);
                    }
                };
                if (index >= 0 && index < userchoices.Count)
                    userchoices[index] = chosen;
                else
                    userchoices.Add(chosen);
            }
            else if (storedpages[index].type == 2)
            {
                List<int> chosen = new List<int>();
                chosen.Add(combobox[0].SelectedIndex);
                if (index >= 0 && index < userchoices.Count)
                    userchoices[index] = chosen;
                else
                    userchoices.Add(chosen);

            }
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Config\{cfgmetadata.modgame}\Mods";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string jsonString = JsonSerializer.Serialize(userchoices, jsonoptions);
            string filepath = path + $@"\{cfgmetadata.modid}.json";
            File.WriteAllText(filepath, jsonString);
            Utilities.ParallelLogger.Log($"[INFO] {cfgmetadata.modid}.json written to {filepath}.");
            Close();
        }
    }
}
