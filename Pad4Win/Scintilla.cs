using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Pad4Win
{
    public class Scintilla : HwndHost
    {
        private readonly static DateTime LexerDateTime = new DateTime(2015, 9, 13); // arbitrary

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

        [DllImport("scilexer.dll")]
        private static extern IntPtr ScintillaDirectFunction(IntPtr ptr, int iMessage, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(WS_EX dwExStyle, string lpszClassName, string lpszWindowName, WS style, int x, int y, int width, int height, IntPtr hWndParent, IntPtr hMenu, IntPtr hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        public static void EnsureLexerLibrary()
        {
            string path = Path.Combine(Path.GetTempPath(), typeof(Scintilla).Namespace, "SciLexer.dll");
            if (!Extensions.FileExists(path) || Extensions.FileGetLastWriteTime(path) != LexerDateTime)
            {
                Extensions.FileCreateDirectory(path);
                var streamName = typeof(Scintilla).Namespace + ".x64.SciLexer.dll.gz";
                using (var stream = typeof(Scintilla).Assembly.GetManifestResourceStream(streamName))
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

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            EnsureLexerLibrary();

            var hwnd = CreateWindowEx(WS_EX.None, "Scintilla", "Pad4Win", WS.CHILD | WS.VISIBLE | WS.VSCROLL | WS.BORDER,
                0, 0, (int)Width, (int)Height, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, null);
            if (hwnd == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }
    }
}
