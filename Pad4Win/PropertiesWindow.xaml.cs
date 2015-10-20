using System.Windows;

namespace Pad4Win
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow(object properties)
        {
            InitializeComponent();
            PG.SelectedObject = properties;
        }
    }
}
