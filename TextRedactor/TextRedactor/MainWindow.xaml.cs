using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using Controls;

namespace TextRedactor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BinaryFormatter formatter = new BinaryFormatter();
        private SettingsManager Settings;
        public MainWindow()
        {
            InitializeComponent();
            Settings = new SettingsManager();
            TextRedactor.InitFullScr(this);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TextRedactor.Dispose();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, TextRedactor.BrowseProject.Notes);
                Settings.Value["CurrentStateProjectBrowserProjects"] = Convert.ToBase64String(stream.ToArray());
            }
            Settings.Value["SettingPathToProjectFolder"] = TextRedactor.BrowseProject.ProjectsPath;
            Settings.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Load();
            if (!string.IsNullOrEmpty(Settings.Value["CurrentStateProjectBrowserProjects"]))
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(Settings.Value["CurrentStateProjectBrowserProjects"])))
                {
                    var notes = formatter.Deserialize(stream) as Dictionary<string, Project>;
                    if (notes != null)
                    {
                        TextRedactor.BrowseProject.RefreshNotes(notes);
                    }
                }
            }

            TextRedactor.BrowseProject.ProjectsPath = string.IsNullOrEmpty(Settings.Value["SettingPathToProjectFolder"]) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TextRedactor\\MyProjects" : Settings.Value["SettingPathToProjectFolder"];
            TextRedactor.SetTextWidth(Settings.Value["SettingMarginWight"]);
            TextRedactor.SetTextFont(Settings.Value["SettingFont"]);
            TextRedactor.SetTextInterval(Settings.Value["SettingLineSpacing"]);
        }

    }
}
