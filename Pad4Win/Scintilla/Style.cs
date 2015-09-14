using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pad4Win.Scintilla
{
    public class Style : DependencyObject
    {
        private const int STYLE_DEFAULT = 32;

        private const int SCI_STYLESETFONT = 2056;
        private const int SCI_STYLESETSIZE = 2055;

        internal ScintillaBox _scintilla;

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(Style),
            new FrameworkPropertyMetadata(STYLE_DEFAULT, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AnyPropertyChanged)));
        //public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AnyPropertyChanged)));
        //public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AnyPropertyChanged)));
        //public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AnyPropertyChanged)));

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        private static void AnyPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Style style = (Style)source;
            if (style._scintilla == null) // not initialized yet, we can't send it now.
            {
                style.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    style.OnPropertyChanged(source, e);
                }));
                return;
            }
            style.OnPropertyChanged(source, e);
        }

        private void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == FontFamilyProperty.Name)
            {
                FontFamily ff = (FontFamily)e.NewValue;
                using (var us = new Utf8String(ff.Source))
                {
                    _scintilla.CallScintilla(SCI_STYLESETFONT, new IntPtr(Index), us.Pointer);
                }
                return;
            }

            if (e.Property.Name == FontSizeProperty.Name)
            {
                double size = (double)e.NewValue;
                _scintilla.CallScintilla(SCI_STYLESETSIZE, Index, (int)size);
                return;
            }
        }
    }
}
