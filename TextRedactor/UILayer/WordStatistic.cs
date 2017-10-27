using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UILayer
{
    public class WordStatistic: INotifyPropertyChanged
    {
        private int _LetterCount;
        private int _WordCount;
        public int LetterCount { get { return _LetterCount; } set { _LetterCount = value; OnPropertyChanged(); } }
        public int WordCount { get
            { return _WordCount; }
            set { _WordCount = value;
                OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
