using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Pad4Win
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Background = SystemColors.WindowColor
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            MM.RaiseMenuItemClickOnKeyGesture(e);
        }

        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.RestoreDirectory = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.DereferenceLinks = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            if (!dlg.ShowDialog(this).GetValueOrDefault())
                return;
        }
    }
}
