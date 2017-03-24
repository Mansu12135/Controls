using System;
using System.Drawing;
using System.Windows.Forms;

namespace Controls
{
    public partial class Note : Control
    {
        private bool disposed;

        internal Note(Image image)
        {
            
            if (image == null){ throw new NullReferenceException("Control can't have background.");}
            InitializeComponent();
            AttachEvents();
            BackgroundImage = image;
        }

        public string Caption { get; set; }

        private void AttachEvents()
        {
           
        }

        private void DettachEvents()
        {
          
        }

        internal virtual void CreatePopupMenu()
        {
            ContextMenu = new ContextMenu();
            var menuItem = new NoteContextMenuItem(0);
            menuItem.ChoosedMenuItem += OnMenuItemClick;
            ContextMenu.MenuItems.Add(new NoteContextMenuItem(0));
        }

        internal void OnMenuItemClick(MenuItemsType type)
        {
            switch (type)
            {
                    
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Right)
            {
            }
            else
            {
                ContextMenu.Show(this,e.Location);
            }
        }

       

        internal enum MenuItemsType
        {
            
        }
    }
}
