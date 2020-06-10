using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using AntiDebugging.WinStructs;

namespace AntiDebugging
{
    public static class SelfDebugger
    {
        public const int DbgContinue = 0x00010002;
        public const int DbgExceptionNotHandled = unchecked((int)0x80010001);

        // Debugging thread main loop
        static void DebuggerThread(object arg)
        {
            // Attach to the process we provided the thread as an argument
            if (!AntiDebug.DebugActiveProcess((int)arg))
                throw new Win32Exception();

            while (true)
            {
                // wait for a debug event
                if (!AntiDebug.WaitForDebugEvent(out var evt, -1))
                    throw new Win32Exception();
                // return DBG_CONTINUE for all events but the exception type
                var continueFlag = SelfDebugger.DbgContinue;
                if (evt.dwDebugEventCode == DebugEventType.ExceptionDebugEvent)
                    continueFlag = SelfDebugger.DbgExceptionNotHandled;
                // continue running the debug
                AntiDebug.ContinueDebugEvent(evt.dwProcessId, evt.dwThreadId, continueFlag);
            }
        }

        public static void DebugSelf(int ppid)
        {
            Console.WriteLine("Debugging {0}", ppid);
            var self = Process.GetCurrentProcess();
            // Child process?
            if (ppid != 0)
            {
                var pdbg = Process.GetProcessById(ppid);
                new Thread(KillOnExit) { IsBackground = true, Name = "KillOnExit" }.Start(pdbg);
                //Wait for our parent to debug us
                WaitForDebugger();
                //Start debugging our parent process
                DebuggerThread(ppid);
                //Now is a good time to die.
                Environment.Exit(1);
            }
            else // else we are the Parent process...
            {
                var procName = Environment.GetCommandLineArgs()[0];
                procName = procName.Replace(".dll", ".exe");
                var psi = new ProcessStartInfo(procName, self.Id.ToString())
                {
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    ErrorDialog = false,
                    //WindowStyle = ProcessWindowStyle.Hidden
                };
                // Start the child process
                var pdbg = Process.Start(psi);
                if (pdbg == null)
                    throw new ApplicationException("Unable to debug");
                // Monitor the child process
                new Thread(KillOnExit) { IsBackground = true, Name = "KillOnExit" }.Start(pdbg);
                // Debug the child process
                new Thread(DebuggerThread) { IsBackground = true, Name = "DebuggerThread" }.Start(pdbg.Id);
                // Wait for the child to debug us
                WaitForDebugger();
            }
        }
        static void WaitForDebugger()
        {
            var start = DateTime.Now;
            while (!AntiDebug.CheckDebuggerUnmanagedPresent() && 
                   !AntiDebug.CheckDebuggerManagedPresent() && 
                   !AntiDebug.CheckRemoteDebugger())
            {
                Console.WriteLine("Application working by self debugging...");
                if ((DateTime.Now - start).TotalMinutes > 1)
                    throw new TimeoutException("Debug operation timeout.");
                Thread.Sleep(1);
            }
        }
        static void KillOnExit(object process)
        {
            ((Process)process).WaitForExit();
            Environment.Exit(1);
        }
    }
}
