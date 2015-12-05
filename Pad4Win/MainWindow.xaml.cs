using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SoftFluent.Windows;

namespace Pad4Win
{
    public partial class MainWindow : Window
    {
        private Encoding _encoding = Encoding.UTF8;
        private string _currentFilePath;

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
            _currentFilePath = dlg.FileName;
        }

        private void MenuProperties_Click(object sender, RoutedEventArgs e)
        {
            var props = new Properties();
            props.Encoding = _encoding;
            props.TextSize = SB.TextLength;
            PropertiesWindow dlg = new PropertiesWindow(props);
            dlg.PG.GroupByCategory = false;
            dlg.PG.IsReadOnly = true;
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            _encoding = props.Encoding;
        }

        private class Properties
        {
            [PropertyGridOptions(EditorDataTemplateResourceKey = "EncodingEditor")]
            public Encoding Encoding { get; set; }
            public long TextSize { get; set; }
        }

        private void MenuFile_Opened(object sender, RoutedEventArgs e)
        {
            //FilePropertiesMenu.IsEnabled = _encoding != null;
        }

        private void MenuFileType_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFilePath == null)
            {
                MenuSaveAs_Click(sender, e);
                return;
            }
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.RestoreDirectory = true;
            dlg.CheckPathExists = true;
            dlg.DereferenceLinks = true;
            dlg.ValidateNames = true;
            if (!dlg.ShowDialog(this).GetValueOrDefault())
                return;

            File.WriteAllText(dlg.FileName, SB.Text, _encoding);
            _currentFilePath = dlg.FileName;
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuUndo_Click(object sender, RoutedEventArgs e)
        {
            SB.Undo();
        }

        private void MenuRedo_Click(object sender, RoutedEventArgs e)
        {
            SB.Redo();
        }

        private void MenuCut_Click(object sender, RoutedEventArgs e)
        {
            SB.Cut();
        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            SB.Copy();
        }

        private void MenuPaste_Click(object sender, RoutedEventArgs e)
        {
            SB.Paste();
        }

        private void MenuClear_Click(object sender, RoutedEventArgs e)
        {
            SB.Clear();
        }

        private void MenuEdit_Opened(object sender, RoutedEventArgs e)
        {
            MenuUndo.IsEnabled = SB.CanUndo();
            MenuRedo.IsEnabled = SB.CanRedo();
            MenuPaste.IsEnabled = SB.CanPaste();
        }

        private void MenuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SB.SelectAll();
        }
    }
}
