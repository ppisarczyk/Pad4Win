using System.Windows;
using System.Windows.Input;

namespace Pad4Win
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow(object properties)
        {
            InitializeComponent();
            PG.SelectedObject = properties;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
