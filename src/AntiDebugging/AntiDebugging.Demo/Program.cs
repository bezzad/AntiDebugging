using System;
using System.Diagnostics;
using System.Reflection;

namespace AntiDebugging.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var mainLibVersion = Assembly.GetAssembly(typeof(AntiDebugging.AntiDebug))?.GetName()?.Version.ToString(3) ?? "1.0.0";
            Console.Title = $"Anti Debugging v{mainLibVersion}";
            Console.WriteLine($"Process {Process.GetCurrentProcess().Id} is running...");
            //            var ip = HardwareHelper.Ip();
            //          Console.WriteLine($"PC is from Internet Protocol Address: {ip}");

            if (PerformChecks())
            {
                Environment.FailFast("Debugger Detected");
            }
            else
            {
                // AntiDump.ProtectDump();
                //
                // Console.WriteLine("Test: Attaching to process in runtime...");
                // var ppid = 0;
                // if (args != null && args.Length > 0)
                // {
                //     int.TryParse(args[0], out ppid);
                // }
                //
                // SelfDebugger.DebugSelf(ppid);
                //
                // PerformChecks();
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
            WriteResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckRemoteDebugger)}", isProcessRemote);
            WriteResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerManagedPresent)}", isManagedCodesAttached);
            WriteResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerUnmanagedPresent)}", isUnManagedCodesAttached);
            WriteResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebugPort)}", checkDebugPort);
            WriteResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckKernelDebugInformation)}", checkKernelDebugInformation);
            WriteResult($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectEmulation)}", detectEmulation);
            WriteResult($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectSandbox)}", detectSandbox);
            WriteResult($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectVirtualMachine)}", detectVirtualMachine);
            WriteResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.DetachFromDebuggerProcess)}", detachFromDebuggerProcess);
            AntiDebug.HideOsThreads();
            Scanner.ScanAndKill(() => Console.WriteLine("Scan and kill system any malware"));

            if (isProcessRemote || isManagedCodesAttached || isUnManagedCodesAttached)
            {
                ProtectionHelper.FreezeMouse();
                ProtectionHelper.ShowCmd("Protector", "Active debugger found!", "C");
                return true;
            }

            return false;
        }

        protected static void WriteResult(string prop, bool value)
        {
            Console.Write($"{prop}: ");
            var originalColor = Console.ForegroundColor;
            if (value)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(value);
            Console.ForegroundColor = originalColor;
        }
    }

}
