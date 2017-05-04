using System;
using System.Collections.Generic;
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
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для NotesBrowser.xaml
    /// </summary>
    public partial class NotesBrowser : UserControl
    {
        private readonly Dictionary<string, Note> NotesCollection = new Dictionary<string, Note>();
        
        public NotesBrowser()
        {
            InitializeComponent();
        }

        public Image NotePicture { get; set; }

        public double PreviousWidth { get; private set; }


        public Dictionary<string, Note> Notes
        {
            get { return NotesCollection; }
        }

        public void HidePanel()
        {
            PreviousWidth = Width;
            Width = 0;
        }

        public void ShowPanel()
        {
            Width = PreviousWidth;
        }

        public void AddNote(string caption, Point location)
        {
            //NotesCollection.Add(caption, new Note(NotePicture) { Caption = caption, Bounds = new Rectangle(location, NotePicture.Size) });
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                PreviousWidth = Width;
            }
        }
    }
}
