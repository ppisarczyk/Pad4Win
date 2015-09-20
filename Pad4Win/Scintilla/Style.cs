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

        private const int SCI_STYLESETFORE = 2051;
        private const int SCI_STYLESETBACK = 2052;
        private const int SCI_STYLESETBOLD = 2053;
        private const int SCI_STYLESETITALIC = 2054;
        private const int SCI_STYLESETSIZE = 2055;
        private const int SCI_STYLESETFONT = 2056;
        private const int SCI_STYLESETUNDERLINE = 2059;
        private const int SCI_STYLESETWEIGHT = 2063;
        private const int SCI_STYLESETVISIBLE = 2074;

        internal ScintillaBox _scintilla;

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(Style),
            new FrameworkPropertyMetadata(STYLE_DEFAULT, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(Style),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty BackgroundProperty = TextElement.BackgroundProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));
        public static readonly DependencyProperty TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(Style), new FrameworkPropertyMetadata(new TextDecorationCollection(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AnyPropertyChanged)));

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
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

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
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

        public static int ToScintillaColor(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 0x10);
        }

        public static int ToScintillaColor(Brush brush, Color defaultValue)
        {
            SolidColorBrush sc = brush as SolidColorBrush;
            if (sc == null)
                return ToScintillaColor(defaultValue);

            return ToScintillaColor(sc.Color);
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

            if (e.Property.Name == ForegroundProperty.Name)
            {
                _scintilla.CallScintilla(SCI_STYLESETFORE, Index, ToScintillaColor(Foreground, Colors.Black));
                return;
            }

            if (e.Property.Name == BackgroundProperty.Name)
            {
                _scintilla.CallScintilla(SCI_STYLESETBACK, Index, ToScintillaColor(Background, Colors.White));
                return;
            }

            if (e.Property.Name == FontWeightProperty.Name)
            {
                FontWeight w = (FontWeight)e.NewValue;
                int h = w.GetHashCode(); // yes, it's the value
                _scintilla.CallScintilla(SCI_STYLESETWEIGHT, Index, h);
                return;
            }

            if (e.Property.Name == TextDecorationsProperty.Name)
            {
                bool underline = false;
                foreach (var td in TextDecorations)
                {
                    if (td.Location == TextDecorationLocation.Underline)
                    {
                        underline = true;
                        _scintilla.CallScintilla(SCI_STYLESETUNDERLINE, Index, 1);
                    }
                }

                if (!underline)
                {
                    _scintilla.CallScintilla(SCI_STYLESETUNDERLINE, Index, 0);
                }
                return;
            }

            if (e.Property.Name == IsVisibleProperty.Name)
            {
                _scintilla.CallScintilla(SCI_STYLESETVISIBLE, Index, (bool)e.NewValue ? 1 : 0);
                return;
            }

            if (e.Property.Name == FontStyleProperty.Name)
            {
                FontStyle s = (FontStyle)e.NewValue;
                if (FontStyles.Italic.Equals(s))
                {
                    _scintilla.CallScintilla(SCI_STYLESETITALIC, Index, 1);
                }
                else if (FontStyles.Normal.Equals(s))
                {
                    _scintilla.CallScintilla(SCI_STYLESETITALIC, Index, 0);
                }
                return;
            }
        }
    }
}
