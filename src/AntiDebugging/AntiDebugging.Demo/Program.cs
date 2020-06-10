using System;
using System.Diagnostics;

namespace AntiDebugging.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Process {Process.GetCurrentProcess().Id} is running");
            if (PerformChecks())
            {
                Environment.FailFast("Debugger Detected");
            }
            else
            {
                AntiDump.ProtectDump();

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
            var detachFromDebuggerProcess = AntiDebug.DetachFromDebuggerProcess();
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckRemoteDebugger)}: {isProcessRemote}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerManagedPresent)}: {isManagedCodesAttached}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerUnmanagedPresent)}: {isUnManagedCodesAttached}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebugPort)}: {checkDebugPort}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckKernelDebugInformation)}: {checkKernelDebugInformation}");
            Console.WriteLine($"{nameof(AntiDebug)}.{nameof(AntiDebug.DetachFromDebuggerProcess)}: {detachFromDebuggerProcess}");
            AntiDebug.HideOsThreads();

            if (isProcessRemote || isManagedCodesAttached || isUnManagedCodesAttached)
            {
                Console.WriteLine("Debugger detected!");
                return true;
            }

            return false;
        }
    }
}
