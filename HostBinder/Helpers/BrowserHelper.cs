using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace HostBinder.Helpers
{
    public class BrowserInfo
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public bool IsDefault { get; set; }
        public ImageSource Icon { get; set; }
    }

    public static class BrowserHelper
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        
        public static ImageSource GetMainApplicationIcon(string command)
        {
            Icon icon;
            try
            {
                icon = Icon.ExtractAssociatedIcon(command);
            }
            catch (Exception)
            {
                icon = null;
            }

            if (icon == null) return null;

            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        private static string GetDefaultBrowserCommand()
        {
            var regKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

            //get rid of the enclosing quotes
            var path = regKey.GetValue(null).ToString();

            path = CleanCommandPath(path);

            return path;
        }

        private static string CleanCommandPath(string path)
        {
            path = path.ToLower().Replace("" + (char) 34, "");
            //check to see if the value ends with .exe (this way we can remove any command line arguments)
            if (!path.EndsWith("exe"))
                //get rid of all command line arguments (anything after the .exe must go)
                path = path.Substring(0, path.LastIndexOf(".exe") + 4);
            return path;
        }

        public static BrowserInfo[] GetInstalledBrowsers()
        {
            var root = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet",false);
            var keyNames = root.GetSubKeyNames();

            var list = new List<BrowserInfo>();

            var defaultBrowserCommand = GetDefaultBrowserCommand();

            foreach (var keyName in keyNames)
            {
                var key = root.OpenSubKey(keyName);
                var name = (string)key.GetValue(null);

                var commandKey = key.OpenSubKey(@"shell\open\command");
                var command = CleanCommandPath((string) commandKey.GetValue(null));
                commandKey.Close();
                key.Close();

                var isDefault = (command == defaultBrowserCommand);

                var icon = GetMainApplicationIcon(command);

                list.Add(new BrowserInfo(){Name = name, Command = command, IsDefault = isDefault, Icon = icon});
            }
            root.Close();

            return list.ToArray();
        }
    }
}
