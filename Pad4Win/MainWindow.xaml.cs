using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Pad4Win
{
    public partial class MainWindow : Window
    {
        private Encoding _encoding;

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
            AboutWindow dlg = new AboutWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
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

            _encoding = Extensions.DetectEncoding(dlg.FileName);
            string text = File.ReadAllText(dlg.FileName, _encoding);
            SB.Text = text;
        }

        private void MenuProperties_Click(object sender, RoutedEventArgs e)
        {
            if (_encoding == null)
                return;

            var props = new Properties();
            props.Encoding = _encoding.WebName;
            props.TextSize = SB.TextLength;
            PropertiesWindow dlg = new PropertiesWindow(props);
            dlg.PG.GroupByCategory = false;
            dlg.PG.IsReadOnly = true;
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private class Properties
        {
            public string Encoding { get; set; }
            public long TextSize { get; set; }
        }

        private void MenuFile_Opened(object sender, RoutedEventArgs e)
        {
            FilePropertiesMenu.IsEnabled = _encoding != null;
        }

        private void MenuFileType_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
