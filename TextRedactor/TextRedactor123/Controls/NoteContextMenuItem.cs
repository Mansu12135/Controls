using System;
using System.Windows.Forms;

namespace Controls
{
    class NoteContextMenuItem:MenuItem
    {
        internal delegate void MenuItemEventHandler(Note.MenuItemsType type);
        public event MenuItemEventHandler ChoosedMenuItem;
        private Note.MenuItemsType type;

        public NoteContextMenuItem(Note.MenuItemsType type)
        {
            this.type = type;
        }

        protected override void OnClick(EventArgs e)
        {
            ChoosedMenuItem.Invoke(type);
        }
    }
}
