using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace AntiDebugging
{
    public static class ProtectionHelper
    {
        private static bool TurnedOnFreezeMouse { get; set; }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcessId();
        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr processId);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern void BlockInput([In, MarshalAs(UnmanagedType.Bool)] bool fBlockIt);

        
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool DetectSandbox()
        {
            return GetModuleHandle("SbieDll.dll").ToInt32() != 0;
        }

        public static void FreezeMouse()
        {
            TurnedOnFreezeMouse = true;
            var killDirectory = new Thread(FreezeWindowsProcess);
            killDirectory.Start();
        }
        public static void ReleaseMouse()
        {
            TurnedOnFreezeMouse = false;
            BlockInput(TurnedOnFreezeMouse);
        }

        public static bool DetectEmulation()
        {
            long tickCount = Environment.TickCount;
            Thread.Sleep(500);
            long tickCount2 = Environment.TickCount;
            return tickCount2 - tickCount < 500L;
        }

        public static bool DetectVirtualMachine()
        {
            using (var managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (var managementObjectCollection = managementObjectSearcher.Get())
                {
                    foreach (var managementBaseObject in managementObjectCollection)
                    {
                        if ((managementBaseObject["Manufacturer"].ToString().ToLower() == "microsoft corporation" && 
                             managementBaseObject["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL")) || 
                            managementBaseObject["Manufacturer"].ToString().ToLower().Contains("vmware") || 
                            managementBaseObject["Model"].ToString() == "VirtualBox")
                        {
                            return true;
                        }
                    }
                }
            }
            foreach (var managementBaseObject2 in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController").Get())
            {
                if (managementBaseObject2.GetPropertyValue("Name").ToString().Contains("VMware") && 
                    managementBaseObject2.GetPropertyValue("Name").ToString().Contains("VBox"))
                {
                    return true;
                }
            }

            return false;
        }

        
        public static void ShowCmd(string title, string text, string color, int timeoutSec = 10)
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + $"START CMD /C \"COLOR {color} && TITLE {title} && ECHO {text} && TIMEOUT {timeoutSec}\"")
            {
                CreateNoWindow = true, 
                UseShellExecute = false
            });
        }


        private static void FreezeWindowsProcess()
        {
            while (TurnedOnFreezeMouse)
            {
                BlockInput(TurnedOnFreezeMouse);
            }
        }
    }
}
