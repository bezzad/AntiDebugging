using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
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
                        if (managementBaseObject["Manufacturer"]?.ToString()?.ToLower() == "microsoft corporation" &&
                            managementBaseObject["Model"]?.ToString()?.ToUpperInvariant().Contains("VIRTUAL") == true ||
                            managementBaseObject["Manufacturer"]?.ToString()?.ToLower().Contains("vmware") == true ||
                            managementBaseObject["Model"]?.ToString() == "VirtualBox")
                        {
                            return true;
                        }
                    }
                }
            }
            foreach (var managementBaseObject2 in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController").Get())
            {
                if (managementBaseObject2.GetPropertyValue("Name")?.ToString()?.Contains("VMware") == true &&
                    managementBaseObject2.GetPropertyValue("Name")?.ToString()?.Contains("VBox") == true)
                {
                    return true;
                }
            }

            return false;
        }


        private static void FreezeWindowsProcess()
        {
            while (TurnedOnFreezeMouse)
            {
                BlockInput(TurnedOnFreezeMouse);
            }
        }

        internal static void PerformDetach()
        {
            Parallel.Invoke(() => AntiDebug.DetachFromDebuggerProcess(),
                AntiDebug.HideOsThreads,
                Scanner.ScanAndKill);
        }

        internal static void PerformChecks()
        {
            if (AntiDebug.CheckRemoteDebugger())
                throw new Exception(Constants.ActiveRemoteDebuggerFound);

            if (AntiDebug.CheckDebuggerManagedPresent() || AntiDebug.CheckDebugPort())
                throw new Exception(Constants.ActiveDebuggerFound);

            if (AntiDebug.CheckDebuggerUnmanagedPresent())
                throw new Exception(Constants.ActiveUnmanagedDebuggerFound);

            if (AntiDebug.CheckKernelDebugInformation())
                throw new Exception(Constants.ActiveKernelDebuggerFound);

            if (DetectEmulation())
                throw new Exception(Constants.ApplicationRunningOnEmulation);

            if (DetectSandbox())
                throw new Exception(Constants.ApplicationRunningOnSandbox);

            if (DetectVirtualMachine())
                throw new Exception(Constants.ApplicationRunningOnVirtualMachine);
        }

        public static void ShowCmd(string title, string text, string color, int timeoutSec = 10)
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + $"START CMD /C \"COLOR {color} && TITLE {title} && ECHO {text} && TIMEOUT {timeoutSec}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }    
}