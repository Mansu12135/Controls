using System.Windows;
using System.Windows.Documents;

namespace ApplicationLayer
{
    public class FlowDocumentWorker
    {
        private FlowDocument Document;
        int CurrentCharacterOffset;
        TextPointer CurrentPosition;
        TextPointer EndOfDocument;

        public FlowDocumentWorker(FlowDocument docuemnt)
        {
            Document = docuemnt;
        }

        public TextPointer GetTextPointer(int charOffset, bool reset = false)
        {

            if (reset) { ResetToStart(); }

            while (CurrentPosition.CompareTo(EndOfDocument) < 0)
            {
                switch (CurrentPosition.GetPointerContext(LogicalDirection.Forward)) {
                    case TextPointerContext.ElementEnd: {
                            DependencyObject d = this.CurrentPosition.GetAdjacentElement(LogicalDirection.Forward);
                            if (d is Paragraph || d is LineBreak) { CurrentCharacterOffset += 2; }
                           if (d is InlineUIContainer) { CurrentCharacterOffset += 1; }
                            break;
                        }
                    case TextPointerContext.Text: {
                            int characterInCurrentRun = CurrentCharacterOffset + CurrentPosition.GetTextRunLength(LogicalDirection.Forward);

                            if (charOffset <= characterInCurrentRun) {
                                int offset = charOffset - CurrentCharacterOffset;

                                return CurrentPosition.GetPositionAtOffset(offset, LogicalDirection.Forward);
                            }

                            CurrentCharacterOffset = characterInCurrentRun;
                            break;
                        }
                }
                CurrentPosition = CurrentPosition.GetNextContextPosition(LogicalDirection.Forward);
            }

            if (CurrentCharacterOffset != 0 && charOffset <= CurrentCharacterOffset) { return EndOfDocument.GetNextInsertionPosition(LogicalDirection.Backward); }

            return EndOfDocument;
        }

        private void ResetToStart()
        {
            CurrentCharacterOffset = 0;
            EndOfDocument = Document.ContentEnd;
            CurrentPosition = Document.ContentStart;
        }

    }
}
