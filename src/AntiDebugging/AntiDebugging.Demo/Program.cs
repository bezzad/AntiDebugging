using System;
using System.Diagnostics;

namespace AntiDebugging.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Process {Process.GetCurrentProcess().Id} is running...");
            var ip = HardwareHelper.Ip();
            Console.WriteLine($"PC is from Internet Protocol Address: {ip}");

            if (PerformChecks())
            {
                Environment.FailFast("Debugger Detected");
            }
            else
            {
                AntiDump.ProtectDump();
                Scanner.ScanAndKill(() => Console.WriteLine("Scan and kill system any malware"));

                Console.WriteLine("Test: Attaching to process in runtime...");
                var ppid = 0;
                if (args != null && args.Length > 0)
                {
                    int.TryParse(args[0], out ppid);
                }

                SelfDebugger.DebugSelf(ppid);

                PerformChecks();
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Perform basic checks, method 1
        /// Checks are very fast, there is no CPU overhead.
        /// </summary>
        public static bool PerformChecks()
        {
            var isProcessRemote = AntiDebug.CheckRemoteDebugger();
            var isManagedCodesAttached = AntiDebug.CheckDebuggerManagedPresent();
            var isUnManagedCodesAttached = AntiDebug.CheckDebuggerUnmanagedPresent();
            var checkDebugPort = AntiDebug.CheckDebugPort();
            var checkKernelDebugInformation = AntiDebug.CheckKernelDebugInformation();
            var detectEmulation = ProtectionHelper.DetectEmulation();
            var detectSandbox = ProtectionHelper.DetectSandbox();
            var detectVirtualMachine = ProtectionHelper.DetectVirtualMachine();
            var detachFromDebuggerProcess = AntiDebug.DetachFromDebuggerProcess();
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckRemoteDebugger)}: {isProcessRemote}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerManagedPresent)}: {isManagedCodesAttached}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerUnmanagedPresent)}: {isUnManagedCodesAttached}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebugPort)}: {checkDebugPort}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckKernelDebugInformation)}: {checkKernelDebugInformation}");
            Console.WriteLine($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectEmulation)}: {detectEmulation}");
            Console.WriteLine($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectSandbox)}: {detectSandbox}");
            Console.WriteLine($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectVirtualMachine)}: {detectVirtualMachine}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.DetachFromDebuggerProcess)}: {detachFromDebuggerProcess}");
            AntiDebug.HideOsThreads();

            if (isProcessRemote || isManagedCodesAttached || isUnManagedCodesAttached)
            {
                ProtectionHelper.FreezeMouse();
                ProtectionHelper.ShowCmd("Protector", "Active debugger found!", "C");
                return true;
            }

            return false;
        }
    }
}
