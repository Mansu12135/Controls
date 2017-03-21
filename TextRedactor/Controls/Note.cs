using System;
using System.Windows.Documents;
using System.Runtime.Serialization;

namespace Controls
{
    [Serializable]
    public class Note : Item
    {
        public Note(string caption, string text, TextRange range, int pointerStart, int pointerEnd)
        {
            Text = text;
            Name = caption;
            OffsetStart = pointerStart;
            OffsetEnd = pointerEnd;
            Range = range;
        }
        public int OffsetStart {  set; get; }
        [NonSerialized]
        public TextRange Range;// { set; get; }
        public int OffsetEnd {  set; get; }
        public string Text { set; get; }

        public override string Name
        {
            get { return vName; }
            set
            {
                if (string.IsNullOrEmpty(value)) { return; }
                vName = value;
            }
        }

        private string vName;
    }
}
