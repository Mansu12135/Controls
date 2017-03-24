using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для PropertiesForm.xaml
    /// </summary>
    public partial class PropertiesForm : Window
    {
        public List<Task> Tasks = new List<Task>();
        internal Item value
        {
            get;
            private set;
        }
        public PropertiesForm(Item item)
        {
            InitializeComponent();
            value = item;
        }
        public void Init()
        {
            if (value != null)
            {
                ProjectProperties prop = new ProjectProperties();
                prop.propertiesForm = this;
                prop.Init();
                Element.Child = prop;
                Title = "Properties";
            }
            else
            {
                Options opt = new Options();
                opt.HorizontalAlignment = HorizontalAlignment.Center;
                opt.VerticalAlignment = VerticalAlignment.Center;
                opt.ParentControl = this;
                Element.Child = opt;
                Width /= 1.5;
                Title = "Options";
            }

        }
        public IBasicPanel<Item> CalledControl;
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ApplyTask();
            Close();
        }
        private void ApplyTask()
        {
            Tasks.ForEach(task =>
            {
                task.Start();
                task.Wait();
            });
            Tasks.Clear();
            if (CalledControl != null && value != null)
            {
                CalledControl.Save(value.Name);
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            ApplyTask();
        }

        public void AddTask(Task task)
        {
            Tasks.Add(task);
        }

        private void CanselBut_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }

}
