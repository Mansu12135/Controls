using System;
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

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для ExportPanel.xaml
    /// </summary>
    public partial class ExportPanel : UserControl
    {
        public Project project;
        public ExportPanel()
        {
            InitializeComponent();
        }

        public void Init()
        {
            TextBoxAuthor.Text = project.Author;
            TextBoxTitle.Text = project.Name;
            ExportTextBox.Document = new FlowDocument();
            foreach (var doc in project.Files)
            {
                FlowDocument paragraph = new FlowDocument();
                //using (var fStream = new FileStream(doc.Path, FileMode.OpenOrCreate, FileAccess.Read))
                //{
                try
                {
                    FileWorkerManager.Do(paragraph, doc.Path, null, false);
                    //  new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Load(fStream, System.Windows.DataFormats.Rtf);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return;
                }
                //   }
                ExportTextBox.Document.Blocks.AddRange(paragraph.Blocks.ToList());
            }

        }
    }
    public class ExportInfo
    { 
        public string Title { get; set; }
        public string Author { get; set; }
        public string SavePath { get; set; }
        public DateTime? DatePublish { get; set; }

    }
}
