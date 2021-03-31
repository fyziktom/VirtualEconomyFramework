using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Common
{
    public static class BrowserHelpers
    {
        /// <summary>
        /// function will start browser with some preset url
        /// it will detect automaticaly browser based on platform
        /// </summary>
        /// <param name="url"></param>
        public static void OpenBrowser(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);  // Works ok on linux
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url); // Not tested
                }
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    Console.WriteLine(noBrowser.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot Open Browser Window");
            }
        }
    }
}
