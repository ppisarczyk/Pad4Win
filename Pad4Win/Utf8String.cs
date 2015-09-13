using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pad4Win
{
    public class Utf8String : IDisposable
    {
        public Utf8String(string text)
        {
            if (text == null)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(text + '\0');
            Pointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, Pointer, bytes.Length);
        }

        public IntPtr Pointer { get; private set; }

        public void Dispose()
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
                Pointer = IntPtr.Zero;
            }
        }
    }
}
