using System;
using System.Diagnostics;
using System.Reflection;

namespace AntiDebugging.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var mainLibVersion = Assembly.GetAssembly(typeof(AntiDebug))?.GetName().Version?.ToString(3) ?? "1.0.0";
            Console.Title = $"Anti Debugging v{mainLibVersion}";
            Console.WriteLine($"Process {Process.GetCurrentProcess().Id} is running...");

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

                Console.WriteLine();
                WriteColoredResult("Any debugger not found. Application run in a safe environment.", ConsoleColor.DarkGreen);
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
            WriteBooleanResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckRemoteDebugger)}", isProcessRemote);
            WriteBooleanResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerManagedPresent)}", isManagedCodesAttached);
            WriteBooleanResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebuggerUnmanagedPresent)}", isUnManagedCodesAttached);
            WriteBooleanResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckDebugPort)}", checkDebugPort);
            WriteBooleanResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.CheckKernelDebugInformation)}", checkKernelDebugInformation);
            WriteBooleanResult($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectEmulation)}", detectEmulation);
            WriteBooleanResult($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectSandbox)}", detectSandbox);
            WriteBooleanResult($"{nameof(ProtectionHelper)}.{nameof(ProtectionHelper.DetectVirtualMachine)}", detectVirtualMachine);
            WriteBooleanResult($"{nameof(AntiDebug)}.{nameof(AntiDebug.DetachFromDebuggerProcess)}", detachFromDebuggerProcess);
            AntiDebug.HideOsThreads();
            Scanner.ScanAndKill();

            if (isProcessRemote || isManagedCodesAttached || 
                isUnManagedCodesAttached || checkDebugPort || 
                checkKernelDebugInformation || detectEmulation || 
                detectSandbox || detectVirtualMachine)
            {
                ProtectionHelper.FreezeMouse();
                ProtectionHelper.ShowCmd("Protector", "Active debugger found!", "C");
                return true;
            }

            return false;
        }

        protected static void WriteBooleanResult(string prop, bool value)
        {
            Console.Write($"{prop}: ");
            WriteColoredResult(value.ToString(), value ? ConsoleColor.Red : ConsoleColor.Green);
        }
        protected static void WriteColoredResult(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
    }

}
