using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Pad4Win
{
    public static class Extensions
    {
        public const int DefaultWrapSharingViolationsRetryCount = 10;
        public const int DefaultWrapSharingViolationsWaitTime = 100;

        public delegate void WrapSharingViolationsCallback();

        public delegate bool WrapSharingViolationsExceptionsCallback(Exception ioe, int retry, int retryCount, int waitTime);

        public static T WrapSharingViolations<T>(Func<T> action, bool throwOnError)
        {
            return WrapSharingViolations(action, DefaultWrapSharingViolationsRetryCount, DefaultWrapSharingViolationsWaitTime, throwOnError);
        }

        public static T WrapSharingViolations<T>(Func<T> action, int retryCount, int waitTime, bool throwOnError)
        {
            return WrapSharingViolations(action, null, retryCount, waitTime, throwOnError);
        }

        public static T WrapSharingViolations<T>(Func<T> action, WrapSharingViolationsExceptionsCallback exceptionsCallback, int retryCount, int waitTime, bool throwOnError)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    if ((IsSharingViolation(e as IOException) || e is UnauthorizedAccessException) &&
                        i < (retryCount - 1))
                    {
                        bool wait = true;
                        if (exceptionsCallback != null)
                        {
                            wait = exceptionsCallback(e, i, retryCount, waitTime);
                        }

                        if (wait)
                        {
                            Thread.Sleep(waitTime);
                        }
                    }
                    else
                    {
                        if (throwOnError)
                            throw;

                        // else continue
                    }
                }
            }
            return default(T);
        }

        public static void WrapSharingViolations(WrapSharingViolationsCallback action)
        {
            WrapSharingViolations(action, true);
        }

        public static void WrapSharingViolations(WrapSharingViolationsCallback action, bool throwOnError)
        {
            WrapSharingViolations(action, DefaultWrapSharingViolationsRetryCount, DefaultWrapSharingViolationsWaitTime, throwOnError);
        }

        public static void WrapSharingViolations(WrapSharingViolationsCallback action, int retryCount, int waitTime, bool throwOnError)
        {
            WrapSharingViolations(action, null, retryCount, waitTime, throwOnError);
        }

        public static void WrapSharingViolations(WrapSharingViolationsCallback action, WrapSharingViolationsExceptionsCallback exceptionsCallback, int retryCount, int waitTime, bool throwOnError)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception e)
                {
                    if ((IsSharingViolation(e as IOException) || e is UnauthorizedAccessException) &&
                        i < (retryCount - 1))
                    {
                        bool wait = true;
                        if (exceptionsCallback != null)
                        {
                            wait = exceptionsCallback(e, i, retryCount, waitTime);
                        }

                        if (wait)
                        {
                            Thread.Sleep(waitTime);
                        }
                    }
                    else
                    {
                        if (throwOnError)
                            throw;

                        // else continue
                    }
                }
            }
        }

        public static bool IsSharingViolation(IOException exception)
        {
            if (exception == null)
                return false;

            const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
            return exception.HResult == ERROR_SHARING_VIOLATION;
        }

        public static bool DirectoryExists(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            try
            {
                return Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        public static bool FileExists(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            try
            {
                return File.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        public static long? FileGetLength(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (!FileExists(path))
                return null;

            try
            {

                return new FileInfo(path).Length;
            }
            catch
            {
                return null;
            }
        }

        public static DateTime? FileGetLastWriteTime(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (!FileExists(path))
                return null;

            try
            {
                return new FileInfo(path).LastWriteTime;
            }
            catch
            {
                return null;
            }
        }

        public static void FileCreateDirectory(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.GetFullPath(filePath);
            }

            string dir = Path.GetDirectoryName(filePath);
            if (DirectoryExists(dir))
                return;

            Directory.CreateDirectory(dir);
        }

        public static bool IsSameFile(string source, string target)
        {
            if (!FileExists(target))
                return false;

            if (FileGetLastWriteTime(source) != FileGetLastWriteTime(target))
                return false;

            if (FileGetLength(source) != FileGetLength(target))
                return false;

            return true;
        }

        public static IntPtr ToUtf8(this string text)
        {
            if (text == null)
                return IntPtr.Zero;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Marshal.AllocCoTaskMem(bytes.Length);
        }

        public static Encoding DetectEncoding(string filePath)
        {
            return DetectEncoding(filePath, null);
        }

        public static Encoding DetectEncoding(string filePath, Encoding defaultEncodingIfNoBom)
        {
            if (filePath == null)
                throw new ArgumentOutOfRangeException("filePath");

            if (defaultEncodingIfNoBom == null)
            {
                defaultEncodingIfNoBom = Encoding.Default;
            }

            using (StreamReader reader = new StreamReader(filePath, defaultEncodingIfNoBom, true))
            {
                reader.Peek();
                return reader.CurrentEncoding;
            }
        }
    }
}