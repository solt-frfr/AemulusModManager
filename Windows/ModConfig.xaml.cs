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


namespace AemulusModManager
{
    /// <summary>
    /// Interaction logic for ModConfig.xaml
    /// </summary>
    public partial class ModConfig : Window
    {
        public Metadata metadata;
        public string thumbnailPath;
        private bool focused = false;
        private bool edited = false;
        private bool editing = false;
        private string skippedVersion = "";

        public ModConfig(ConfigMetadata mm)
        {
            InitializeComponent();
            if (mm != null)
            {
                Title = $"Edit {mm.name} Configuration Options";
                Utilities.ParallelLogger.Log($"[ERROR] Within ModConfig, mm.modgame is set to {mm.modgame}.");
                Utilities.ParallelLogger.Log($"[ERROR] Within ModConfig, mm.modpath is set to {mm.modpath}.");
            }
        }
        public class ModConfigPath
        {
            public string modpath { get; set; }

            public ModConfigPath(ConfigMetadata mm)
            {
                Utilities.ParallelLogger.Log($"[ERROR] mm.modgame is set to {mm.modgame}.");
                Utilities.ParallelLogger.Log($"[ERROR] mm.modpath is set to {mm.modpath}.");
                modpath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Packages\{mm.modgame}\{mm.modpath}\";
                Utilities.ParallelLogger.Log($"[ERROR] Before leaving the ModConfigPath, path is set to {modpath}.");
            }
        }

            private void OptionNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OptionNameBox.Text))
                CreateButton.IsEnabled = true;
            else
                CreateButton.IsEnabled = false;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            metadata = new Metadata();
            ConfigMetadata mm = new ConfigMetadata(); // Look I'll be fully honest I'm not a great coder, whatever works works, I barely know what I'm doing so if this is bad feel free to correct me. Sincerely, Solt11.
            ModConfigPath config = new ModConfigPath(mm);
            string path = config.modpath;
            if (!Directory.Exists(path))
            {
                Utilities.ParallelLogger.Log($"[ERROR] path is set to {path}.");
                File.Create($"{path}/Test.txt").Dispose();
                Close();
            }
            else
            {
                Utilities.ParallelLogger.Log($"[ERROR] path is set to {path}.");
                Utilities.ParallelLogger.Log($"[ERROR] Failed.");
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
        public class Test
        {
            public void TestMethod()
            {
                ConfigMetadata mm = new ConfigMetadata(); // Assume this is instantiated properly
                ModConfigPath config = new ModConfigPath(mm);  // Create an instance of ModConfig
                string path = config.modpath;  // Access the non-static modpath field through the instance
                Console.WriteLine("ModPath: " + path);
            }
        }
    }
}
