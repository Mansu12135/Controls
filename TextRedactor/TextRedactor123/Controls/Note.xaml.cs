using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для Note.xaml
    /// </summary>
    public partial class Note : UserControl
    {
        public Note(BitmapSource image)
        {
            InitializeComponent();
            if (image == null) { throw new NullReferenceException("Control can't have background."); }
            AttachEvents();
            MainControl.Source = image;
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
            MainControl.ContextMenu.Items.Add(new NoteContextMenuItem(0));
        }

        internal void OnMenuItemClick(MenuItemsType type)
        {
            switch (type)
            {

            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.RightButton==MouseButtonState.Pressed)
            {
            }
            else if(e.LeftButton==MouseButtonState.Pressed)
            {
                ContextMenu.IsOpen = true;
            }
        }

        internal enum MenuItemsType
        {

        }
    }
}
