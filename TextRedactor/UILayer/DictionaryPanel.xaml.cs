using System.Windows;
using System.Windows.Controls;
using ApplicationLayer;
using System.Windows.Media.Animation;
using System;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для DictionaryPanel.xaml
    /// </summary>
    public partial class DictionaryPanel : UserControl
    {
        public DictionaryPanel(SuperTextRedactor Parent)
        {
            InitializeComponent();
            ParentControl = Parent;
        }
        private SuperTextRedactor ParentControl;
        public string defenition = "";
        public string synonimus = "";
        private void ModeDictionary_Checked(object sender, RoutedEventArgs e)
        {
            var but = sender as RadioButton;
            if (but == null) return;
            if (but.Name == "ModeDictionary")
            {
                DictionaryResult.Text = defenition;
            }
            else if (but.Name == "ModeThesaurus")
            {
                DictionaryResult.Text = synonimus;
            }
            // but.t
        }

        public void Show()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(ParentControl.RedactorConteiner.Margin.Left, ParentControl.RedactorConteiner.Margin.Top, 0, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            ParentControl.RedactorConteiner.BeginAnimation(Border.MarginProperty, myThicknessAnimation);
            ParentControl.ShowNotes.Visibility = Visibility.Hidden;
        }

        public void Hide()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(ParentControl.RedactorConteiner.Margin.Left, ParentControl.RedactorConteiner.Margin.Top, -ActualWidth, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            myThicknessAnimation.Completed += MyThicknessAnimation_Completed;
            ((Border)ParentControl.RedactorConteiner).BeginAnimation(Border.MarginProperty, myThicknessAnimation);
        }

        private void MyThicknessAnimation_Completed(object sender, EventArgs e)
        {
            ParentControl.ShowNotes.Visibility = Visibility.Visible;
            ParentControl.MainContainer.Children.Remove(this);
        }
    }
}
