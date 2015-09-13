using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Pad4Win.Scintilla
{
    public class ScintillaBox : HwndHost
    {
        public static readonly DependencyProperty TechnologyProperty =
            DependencyProperty.Register("Technology", typeof(Technology), typeof(ScintillaBox),
            new FrameworkPropertyMetadata(Technology.Default, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ScintillaBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public ScintillaBox()
        {
        }

        public IntPtr DirectPointer { get; private set; }

        public Technology Technology
        {
            get { return (Technology)GetValue(TechnologyProperty); }
            set { SetValue(TechnologyProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void AnyPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ScintillaBox sb = (ScintillaBox)source;
            if (sb.Handle == IntPtr.Zero) // not initialized yet, we can't send it now.
            {
                sb.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    sb.OnPropertyChanged(source, e);
                }));
                return;
            }
            sb.OnPropertyChanged(source, e);
        }

        private void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == TextProperty.Name)
            {
                string text = (string)e.NewValue;
                using (var us = new Utf8String(text))
                {
                    CallScintilla(SCI_SETTEXT, IntPtr.Zero, us.Pointer);
                }
                return;
            }

            if (e.Property.Name == TechnologyProperty.Name)
            {
                Technology value = (Technology)e.NewValue;
                CallScintilla(SCI_SETTECHNOLOGY, (IntPtr)value);
                return;
            }
        }

        private static void TechnologyPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ScintillaBox sb = (ScintillaBox)source;
            Technology value = (Technology)e.NewValue;
            sb.CallScintilla(SCI_SETTECHNOLOGY, (IntPtr)value);
        }

        private readonly static DateTime LexerDateTime = new DateTime(2015, 9, 13); // arbitrary
        private const string SciLexerDll = "SciLexer.dll";
        private const string ScintillaControlClassName = "Scintilla";

        private const int SCI_SETTEXT = 2181;
        private const int SCI_GETDIRECTPOINTER = 2185;
        private const int SCI_SETTECHNOLOGY = 2630;
        private const int SCI_GETTECHNOLOGY = 2631;

        [Flags]
        private enum WS_EX : uint
        {
            ACCEPTFILES = 0x10,
            APPWINDOW = 0x40000,
            CLIENTEDGE = 0x200,
            COMPOSITED = 0x2000000,
            CONTEXTHELP = 0x400,
            CONTROLPARENT = 0x10000,
            DLGMODALFRAME = 1,
            LAYERED = 0x80000,
            LAYOUTRTL = 0x400000,
            LEFT = 0,
            LEFTSCROLLBAR = 0x4000,
            LTRREADING = 0,
            MDICHILD = 0x40,
            NOACTIVATE = 0x8000000,
            NOINHERITLAYOUT = 0x100000,
            None = 0,
            NOPARENTNOTIFY = 4,
            OVERLAPPEDWINDOW = 0x300,
            PALETTEWINDOW = 0x188,
            RIGHT = 0x1000,
            RIGHTSCROLLBAR = 0,
            RTLREADING = 0x2000,
            STATICEDGE = 0x20000,
            TOOLWINDOW = 0x80,
            TOPMOST = 8,
            TRANSPARENT = 0x20,
            WINDOWEDGE = 0x100
        }

        [Flags]
        private enum WS : uint
        {
            BORDER = 0x800000,
            CAPTION = 0xc00000,
            CHILD = 0x40000000,
            CHILDWINDOW = 0x40000000,
            CLIPCHILDREN = 0x2000000,
            CLIPSIBLINGS = 0x4000000,
            DISABLED = 0x8000000,
            DLGFRAME = 0x400000,
            GROUP = 0x20000,
            HSCROLL = 0x100000,
            ICONIC = 0x20000000,
            MAXIMIZE = 0x1000000,
            MAXIMIZEBOX = 0x10000,
            MINIMIZE = 0x20000000,
            MINIMIZEBOX = 0x20000,
            OVERLAPPED = 0,
            OVERLAPPEDWINDOW = 0xcf0000,
            POPUP = 0x80000000,
            POPUPWINDOW = 0x80880000,
            SIZEBOX = 0x40000,
            SYSMENU = 0x80000,
            TABSTOP = 0x10000,
            THICKFRAME = 0x40000,
            TILED = 0,
            TILEDWINDOW = 0xcf0000,
            VISIBLE = 0x10000000,
            VSCROLL = 0x200000
        }

        [DllImport(SciLexerDll)]
        private static extern IntPtr Scintilla_DirectFunction(IntPtr ptr, int iMessage, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(WS_EX dwExStyle, string lpszClassName, string lpszWindowName, WS style, int x, int y, int width, int height, IntPtr hWndParent, IntPtr hMenu, IntPtr hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void EnsureLexerLibrary()
        {
            string path = Path.Combine(Path.GetTempPath(), typeof(ScintillaBox).Namespace, SciLexerDll);
            if (!Extensions.FileExists(path) || Extensions.FileGetLastWriteTime(path) != LexerDateTime)
            {
                Extensions.FileCreateDirectory(path);

                // SciLexer.dll.gz must be an embedded resource aside this file
                var streamName = typeof(ScintillaBox).Namespace + "." + SciLexerDll + ".gz";
                using (var stream = typeof(ScintillaBox).Assembly.GetManifestResourceStream(streamName))
                {
                    using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            gzip.CopyTo(file);
                        }
                        File.SetLastWriteTime(path, LexerDateTime);
                    }
                }
            }
            LoadLibrary(path);
        }

        public IntPtr CallScintilla(int iMessage)
        {
            return CallScintilla(iMessage, IntPtr.Zero, IntPtr.Zero);
        }

        public IntPtr CallScintilla(int iMessage, IntPtr wParam)
        {
            return CallScintilla(iMessage, wParam, IntPtr.Zero);
        }

        public IntPtr CallScintilla(int iMessage, IntPtr wParam, IntPtr lParam)
        {
            if (DirectPointer == null)
                return IntPtr.Zero;

            return Scintilla_DirectFunction(DirectPointer, iMessage, wParam, lParam);
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            EnsureLexerLibrary();

            var hwnd = CreateWindowEx(WS_EX.None, ScintillaControlClassName, "Pad4Win", WS.CHILD | WS.VISIBLE | WS.VSCROLL | WS.BORDER,
                0, 0, (int)Width, (int)Height, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, null);
            if (hwnd == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            DirectPointer = SendMessage(hwnd, SCI_GETDIRECTPOINTER, IntPtr.Zero, IntPtr.Zero);
            var technology = (Technology)CallScintilla(SCI_GETTECHNOLOGY);

            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
            DirectPointer = IntPtr.Zero;
        }
    }
}
