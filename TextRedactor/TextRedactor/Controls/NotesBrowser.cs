using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Controls
{
    public partial class NotesBrowser : Component
    {
        private readonly Dictionary<string, Note> NotesCollection = new Dictionary<string, Note>();

        public NotesBrowser()
        {
            InitializeComponent();
        }

        public Image NotePicture { get; set; }

        public Dictionary<string, Note> Notes { 
            get { return NotesCollection;}
        }

        public void AddNote(string caption, Point location)
        {
            NotesCollection.Add(caption, new Note(NotePicture) { Caption = caption, Bounds = new Rectangle(location,NotePicture.Size) });
        }

    }
}
