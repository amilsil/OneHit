using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Windows.Forms;

namespace OneHit.Util
{
    public class WindowsShell
    {
        #region fields
        public static int MOD_ALT = 0x1;
        public static int MOD_CONTROL = 0x2;
        public static int MOD_SHIFT = 0x4;
        public static int MOD_WIN = 0x8;
        public static int WM_HOTKEY = 0x312;
        #endregion

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int keyId;
        public static void RegisterHotKey(HwndSource f, Keys key)
        {
            int modifiers = 0;

            if ((key & Keys.Alt) == Keys.Alt)
                modifiers = modifiers | WindowsShell.MOD_ALT;

            if ((key & Keys.Control) == Keys.Control)
                modifiers = modifiers | WindowsShell.MOD_CONTROL;

            if ((key & Keys.Shift) == Keys.Shift)
                modifiers = modifiers | WindowsShell.MOD_SHIFT;

            Keys k = key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
            keyId = f.GetHashCode(); // this should be a key unique ID, modify this if you want more than one hotkey
            RegisterHotKey((IntPtr)f.Handle, keyId, (int)modifiers, (int)k);
        }

        private delegate void Func();

        public static void UnregisterHotKey(HwndSource f)
        {
            try
            {
                // modify this if you want more than one hotkey
                UnregisterHotKey(f.Handle, keyId); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void RegisterInStartup(bool isChecked, string applicationName)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (isChecked)
            {
                registryKey.SetValue(applicationName, Application.ExecutablePath);
            }
            else
            {
                if (registryKey.GetValue(applicationName) != null)
                {
                    registryKey.DeleteValue(applicationName);
                }
            }
        }
    }
}
