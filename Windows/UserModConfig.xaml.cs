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
            string previewpath = path + $@"\Preview{configpage}.png";
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            preview.UriSource = new Uri(previewpath, UriKind.Absolute);
            preview.EndInit();
            Preview.Source = preview;
            if (File.Exists(jsonpath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true // I'm not sure if I need this for reading it but might as well play it safe.
                };
                string jsonString = File.ReadAllText(jsonpath);
                storedpages = JsonSerializer.Deserialize<List<StoredPage>>(jsonString, jsonoptions);
                NameText.Text = $"{storedpages[0].optionname}";
            }
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
                    choiceTextBoxes.Add(newTextBox);
                    Height += 23;
                }
            }
            for (int i = 0; i < storedpages[0].optionnum && i < storedpages[0].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textblocks
            {
                choiceTextBoxes[i].Text = storedpages[0].choice[i];
            }
            if (storedpages[0].type == 0)
            {
                TypeDescText.Text = "Only one option can be chosen.";
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
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (storedpages.Count == configpage || storedpages.Count < configpage)
                NextButton.IsEnabled = false;
            else
            {
                choiceTextBoxes.Clear();
                TextBoxContainer.Children.Clear();
                BackButton.IsEnabled = true;
                configpage += 1;
                Utilities.ParallelLogger.Log($"[DEBUG] Number of indecies in json = {storedpages.Count}.");
                Utilities.ParallelLogger.Log($"[DEBUG] 'Page' Number = {configpage}.");
                Utilities.ParallelLogger.Log($"[DEBUG] Index Number = {index}.");
                if (storedpages.Count == configpage || storedpages.Count < configpage)
                    NextButton.IsEnabled = false;
                NameText.Text = $"{storedpages[index].optionname}";
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
                        choiceTextBoxes.Add(newTextBox);
                        Height += 23;
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
                    TextBoxContainer.Children.Add(moddrop);
                }
                for (int i = 0; i < storedpages[index].optionnum && i < storedpages[index].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textblocks
                {
                    choiceTextBoxes[i].Text = storedpages[index].choice[i];
                }
                if (storedpages[index].type == 0)
                {
                    TypeDescText.Text = "Only one option can be chosen.";
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
            if (configpage == 1)
                BackButton.IsEnabled = false;
            else
            {
                NextButton.IsEnabled = true;
                choiceTextBoxes.Clear();
                TextBoxContainer.Children.Clear();
                configpage -= 1;
                if (configpage == 1)
                    BackButton.IsEnabled = false;
                Utilities.ParallelLogger.Log($"[DEBUG] Number of indecies in json = {storedpages.Count}.");
                Utilities.ParallelLogger.Log($"[DEBUG] 'Page' Number = {configpage}.");
                Utilities.ParallelLogger.Log($"[DEBUG] Index Number = {index}.");
                NameText.Text = $"{storedpages[index].optionname}";
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
                    choiceTextBoxes.Add(newTextBox);
                    Height += 23;
                }
            }
            for (int i = 0; i < storedpages[index].optionnum && i < storedpages[index].choice.Length && i < choiceTextBoxes.Count; i++) // Put the data onto the textblocks
            {
                choiceTextBoxes[i].Text = storedpages[index].choice[i];
            }
            if (storedpages[index].type == 0)
            {
                TypeDescText.Text = "Only one option can be chosen.";
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
            Close();
        }
    }
}
