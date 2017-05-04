using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Controls;

namespace TextRedactor
{
    public class Project
    {
        private SuperRichTextBox control;

        public event SavingHandler OnChangingText;

        public delegate void SavingHandler();

        public Project(string name)
        {
            control.KeyDown += control_KeyDown;
        }

        void control_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (OnChangingText == null) { return; }
            OnChangingText.Invoke();
        }


    }
}
