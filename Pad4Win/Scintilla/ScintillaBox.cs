﻿using System;
using System.Collections.ObjectModel;
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
        private StyleCollection _styles;

        public static readonly DependencyProperty TechnologyProperty =
            DependencyProperty.Register("Technology", typeof(Technology), typeof(ScintillaBox),
            new FrameworkPropertyMetadata(Technology.Default, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ScintillaBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public static readonly DependencyProperty LexerProperty =
            DependencyProperty.Register("Lexer", typeof(Lexer), typeof(ScintillaBox),
            new FrameworkPropertyMetadata(Lexer.Null, FrameworkPropertyMetadataOptions.AffectsRender, AnyPropertyChanged));

        public ScintillaBox()
        {
            _styles = new StyleCollection(this);
        }

        public StyleCollection Styles
        {
            get
            {
                return _styles;
            }
        }

        public IntPtr DirectPointer { get; private set; }

        public Lexer Lexer
        {
            get { return (Lexer)GetValue(LexerProperty); }
            set { SetValue(LexerProperty, value); }
        }

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

        public long TextLength
        {
            get
            {
                return CallScintilla(SCI_GETTEXTLENGTH, 0).ToInt64();
            }
        }

        public void ClearAll()
        {
            CallScintilla(SCI_CLEARALL);
        }

        public void Clear()
        {
            CallScintilla(SCI_CLEAR);
        }

        public void Paste()
        {
            CallScintilla(SCI_PASTE);
        }

        public void Copy()
        {
            CallScintilla(SCI_COPY);
        }

        public void Cut()
        {
            CallScintilla(SCI_CUT);
        }

        public void Undo()
        {
            CallScintilla(SCI_UNDO);
        }

        public void Redo()
        {
            CallScintilla(SCI_REDO);
        }

        public void SelectAll()
        {
            CallScintilla(SCI_SELECTALL);
        }

        public bool CanRedo()
        {
            return CallScintilla(SCI_CANREDO) != IntPtr.Zero;
        }

        public bool CanUndo()
        {
            return CallScintilla(SCI_CANUNDO) != IntPtr.Zero;
        }

        public bool CanPaste()
        {
            return CallScintilla(SCI_CANPASTE) != IntPtr.Zero;
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

            if (e.Property.Name == LexerProperty.Name)
            {
                CallScintilla(SCI_SETLEXER, (int)e.NewValue);
                return;
            }


            if (e.Property.Name == TechnologyProperty.Name)
            {
                Technology value = (Technology)e.NewValue;
                CallScintilla(SCI_SETTECHNOLOGY, (int)value);
                return;
            }
        }

        private readonly static DateTime LexerDateTime = new DateTime(2015, 9, 13); // arbitrary
        private const string SciLexerDll = "SciLexer.dll";
        private const string ScintillaControlClassName = "Scintilla";

        private const int SC_CP_UTF8 = 65001;

        private const int SCI_CLEARALL = 2004;
        private const int SCI_REDO = 2011;
        private const int SCI_SELECTALL = 2013;
        private const int SCI_CANREDO = 2016;
        private const int SCI_SETCODEPAGE = 2037;
        private const int SCI_STYLERESETDEFAULT = 2058;
        private const int SCI_CANPASTE = 2173;
        private const int SCI_CANUNDO = 2174;
        private const int SCI_UNDO = 2176;
        private const int SCI_CUT = 2177;
        private const int SCI_COPY = 2178;
        private const int SCI_PASTE = 2179;
        private const int SCI_CLEAR = 2180;
        private const int SCI_SETTEXT = 2181;
        private const int SCI_GETTEXTLENGTH = 2183;
        private const int SCI_GETDIRECTPOINTER = 2185;
        private const int SCI_SETTECHNOLOGY = 2630;
        private const int SCI_GETTECHNOLOGY = 2631;
        private const int SCI_SETLEXER = 4001;

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

        public IntPtr CallScintilla(int iMessage, int wParam)
        {
            return CallScintilla(iMessage, new IntPtr(wParam));
        }

        public IntPtr CallScintilla(int iMessage, int wParam, int lParam)
        {
            return CallScintilla(iMessage, new IntPtr(wParam), new IntPtr(lParam));
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

            // initialize various stuff
            CallScintilla(SCI_SETCODEPAGE, SC_CP_UTF8);

            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
            DirectPointer = IntPtr.Zero;
        }

        public void StyleResetDefault()
        {
            CallScintilla(SCI_STYLERESETDEFAULT);
        }
    }
}
