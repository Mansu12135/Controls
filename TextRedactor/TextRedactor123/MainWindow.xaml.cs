using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using ApplicationLayer;
using UILayer;

namespace TextRedactor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISettings
    {
        public MainWindow()
        {
            InitializeComponent();
            Settings = new SettingsManager();
           // TextRedactor.InitFullScr(this);
        }
        BinaryFormatter formatter = new BinaryFormatter();
        private SettingsManager Settings;

        public event EventHandler OnDataLoad;


        void ISettings.SaveSettings()
        {
            SaveValue("CurrentStateProjectBrowserProjects");
            SaveValue("SettingFontsList");
            SaveValue("SettingMarginsList");
            //Settings.Value["SettingPathToProjectFolder"] = TextRedactor.BrowseProject.ProjectsPath;
            Settings.Save();
        }

        private void SaveValue(string value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                switch (value)
                {
                    case "CurrentStateProjectBrowserProjects":
                        {
                            //formatter.Serialize(stream, TextRedactor.BrowseProject.Notes);
                            Settings.Value[value] = Convert.ToBase64String(stream.ToArray());
                        }
                        break;
                    case "SettingFontsList":
                        {
                            formatter.Serialize(stream, FormatPanel.FontSize);
                            Settings.Value[value] = Convert.ToBase64String(stream.ToArray());
                        }
                        break;
                    case "SettingMarginsList":
                        {
                            formatter.Serialize(stream, FormatPanel.MarginWigth);
                            Settings.Value[value] = Convert.ToBase64String(stream.ToArray());
                        }
                        break;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //TextRedactor.Dispose();
            ((ISettings)this).SaveSettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Load();
            LoadValue("CurrentStateProjectBrowserProjects");
            LoadValue("SettingFontsList");
            //LoadValue("SettingMarginsList");
            //TextRedactor.BrowseProject.ProjectsPath = string.IsNullOrEmpty(Settings.Value["SettingPathToProjectFolder"]) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TextRedactor\\MyProjects" : Settings.Value["SettingPathToProjectFolder"];
            //TextRedactor.SetTextWidth(Settings.Value["SettingMarginWight"]);
            //TextRedactor.SetTextFont(Settings.Value["SettingFont"]);
            //TextRedactor.SetTextInterval(Settings.Value["SettingLineSpacing"]);
            if (OnDataLoad != null)
            {
                OnDataLoad.Invoke(this, new EventArgs());
            }
        }

        private void LoadValue(string value)
        {
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(Settings.Value[value])))
            {
                switch (value)
                {
                    case "CurrentStateProjectBrowserProjects":
                        {
                            if (string.IsNullOrEmpty(Settings.Value[value]))
                            {
                                return;
                            }
                            var notes = formatter.Deserialize(stream) as Dictionary<string, Project>;
                            if (notes != null)
                            {
                               // TextRedactor.BrowseProject.RefreshNotes(notes);
                            }
                        }
                        break;
                    case "SettingFontsList":
                        {
                            if (string.IsNullOrEmpty(Settings.Value[value]))
                            {
                                FormatPanel.FontSize.Add(1, 8);
                                FormatPanel.FontSize.Add(2, 9);
                                FormatPanel.FontSize.Add(3, 11);
                                FormatPanel.FontSize.Add(4, 12);
                                FormatPanel.FontSize.Add(5, 14);
                                FormatPanel.FontSize.Add(6, 16);
                                FormatPanel.FontSize.Add(7, 18);
                                FormatPanel.FontSize.Add(8, 20);
                                FormatPanel.FontSize.Add(9, 22);
                                FormatPanel.FontSize.Add(10, 24);
                                FormatPanel.FontSize.Add(11, 26);
                                FormatPanel.FontSize.Add(12, 28);
                                FormatPanel.FontSize.Add(13, 36);
                                FormatPanel.FontSize.Add(14, 48);
                                FormatPanel.FontSize.Add(15, 72);
                                return;
                            }
                            var notes = formatter.Deserialize(stream) as Dictionary<int, double>;
                            if (notes != null)
                            {
                                FormatPanel.FontSize = notes;
                            }
                        }
                        break;
                    case "SettingMarginsList":
                        {
                            if (string.IsNullOrEmpty(Settings.Value[value]))
                            {
                                FormatPanel.MarginWigth.Add(1, 100);
                                FormatPanel.MarginWigth.Add(2, 200);
                                FormatPanel.MarginWigth.Add(3, 300);
                                FormatPanel.MarginWigth.Add(4, 400);
                                FormatPanel.MarginWigth.Add(5, 500);
                                FormatPanel.MarginWigth.Add(6, 600);
                                FormatPanel.MarginWigth.Add(7, 700);
                                FormatPanel.MarginWigth.Add(8, 800);
                                FormatPanel.MarginWigth.Add(9, 900);
                                return;
                            }
                            var notes = formatter.Deserialize(stream) as Dictionary<int, double>;
                            if (notes != null)
                            {
                                FormatPanel.MarginWigth = notes;
                            }
                        }
                        break;
                }
            }
        }
    }
}
