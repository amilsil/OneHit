using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows;

namespace OneHit.Util
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    public enum FolderType
    {
        Closed,
        Open
    }

    public enum IconSize
    {
        Large,
        Small
    }

    public static class IconHelper 
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyIcon(IntPtr hIcon);

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_OPENICON = 0x000000002;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        static Dictionary<string, BitmapSource> _bmSources = new Dictionary<string, BitmapSource>();
        public static BitmapSource GetProxyBitmapSource(string path)
        {
            string type = string.Empty;

            if (path.StartsWith("http"))
            {
                type = "http";
            }
            else if (Directory.Exists(path) && IsFolder(path))
            {
                type = "folder";
            }

            if (string.IsNullOrEmpty(type))
            {
                Icon icon = GetIcon(path);

                var oldImage = icon.ToBitmap();

                var oldBitmap = oldImage as System.Drawing.Bitmap ?? new System.Drawing.Bitmap(oldImage);

                BitmapSource bitmapSource =
                    System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        oldBitmap.GetHbitmap(System.Drawing.Color.Transparent),
                        IntPtr.Zero,
                        new Int32Rect(0, 0, oldBitmap.Width, oldBitmap.Height),
                        null);

                return bitmapSource;
            }
            else if (!_bmSources.ContainsKey(type))
            {
                Icon icon = GetIcon(path);

                var oldImage = icon.ToBitmap();

                var oldBitmap = oldImage as System.Drawing.Bitmap ?? new System.Drawing.Bitmap(oldImage);

                BitmapSource bitmapSource =
                    System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        oldBitmap.GetHbitmap(System.Drawing.Color.Transparent),
                        IntPtr.Zero,
                        new Int32Rect(0, 0, oldBitmap.Width, oldBitmap.Height),
                        null);

                _bmSources[type] = bitmapSource;
            }

            return _bmSources[type];
        }

        public static Icon GetIcon(string path)
        {
            Icon icon;

            // If it's a URL, Get the default browser icon
            if (path.StartsWith("http", false, System.Globalization.CultureInfo.CurrentCulture))
            {
                icon = GetFileIcon(_getDefaultBrowser(), IconSize.Small);
                return icon;
            }

            if (Directory.Exists(path) && IsFolder(path))
            {
                icon = GetFolderIcon(path, IconSize.Small, FolderType.Closed);
            }
            else if (File.Exists(path))
            {
                icon = GetFileIcon(path, IconSize.Small);
            }
            else
            {
                icon = null;
            }

            return icon;
        }

        public static string _getDefaultBrowser()
        {
            string browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                //trim off quotes
                browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
                if (!browser.EndsWith("exe"))
                {
                    //get rid of everything after the ".exe"
                    browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
                }
            }
            finally
            {
                if (key != null) key.Close();
            }
            return browser;
        }

        public static bool IsFolder(string path)
        {
            return ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory);
        }

        public static Icon GetFileIcon(string path, IconSize size)
        {
            // Need to add size check, although errors generated at present!    
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
           
            if (IconSize.Small == size)
            {
                flags += SHGFI_SMALLICON;
            }
            else
            {
                flags += SHGFI_LARGEICON;
            }
            // Get the folder icon    
            var shfi = new SHFILEINFO();

            var res = SHGetFileInfo(@path,
                0,
                out shfi,
                (uint)Marshal.SizeOf(shfi),
                flags);

            if (res == IntPtr.Zero)
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());

            // Load the icon from an HICON handle  
            Icon.FromHandle(shfi.hIcon);

            // Now clone the icon, so that it can be successfully stored in an ImageList
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            DestroyIcon(shfi.hIcon);        // Cleanup    

            return icon;
        }

        public static Icon GetFolderIcon(string path, IconSize size, FolderType folderType)
        {
            // Need to add size check, although errors generated at present!    
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (FolderType.Open == folderType)
            {
                flags += SHGFI_OPENICON;
            }
            if (IconSize.Small == size)
            {
                flags += SHGFI_SMALLICON;
            }
            else
            {
                flags += SHGFI_LARGEICON;
            }
            // Get the folder icon    
            var shfi = new SHFILEINFO();

            var res = SHGetFileInfo(@path,
                FILE_ATTRIBUTE_DIRECTORY,
                out shfi,
                (uint)Marshal.SizeOf(shfi),
                flags);

            if (res == IntPtr.Zero)
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());

            // Load the icon from an HICON handle  
            Icon.FromHandle(shfi.hIcon);

            // Now clone the icon, so that it can be successfully stored in an ImageList
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            DestroyIcon(shfi.hIcon);        // Cleanup    
            
            return icon;
        }
        
    }
}