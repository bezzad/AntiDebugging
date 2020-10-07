using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AntiDebugging.WinStructs;

namespace AntiDebugging
{
    public static class AntiDebug
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool DebugActiveProcess(int dwProcessId);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool WaitForDebugEvent([Out] out DebugEvent lpDebugEvent, int dwMilliseconds);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool ContinueDebugEvent(int dwProcessId, int dwThreadId, int dwContinueStatus);

        [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsDebuggerPresent();

        [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CheckRemoteDebuggerPresent(SafeHandle hProcess, [MarshalAs(UnmanagedType.Bool)] ref bool isDebuggerPresent);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtQueryInformationProcess([In] IntPtr processHandle, [In] ProcessInfoClass processInformationClass, out IntPtr processInformation, [In] int processInformationLength, out int returnLength);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtClose([In] IntPtr handle);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtRemoveProcessDebug(IntPtr processHandle, IntPtr debugObjectHandle);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtSetInformationDebugObject([In] IntPtr debugObjectHandle, [In] DebugObjectInformationClass debugObjectInformationClass, [In] IntPtr debugObjectInformation, [In] int debugObjectInformationLength, [Out] out int returnLength);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtQuerySystemInformation([In] SystemInformationClass systemInformationClass, IntPtr systemInformation, [In] int systemInformationLength, [Out] out int returnLength);

        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtSetInformationThread(IntPtr threadHandle, ThreadInformationClass threadInformationClass, IntPtr threadInformation, int threadInformationLength);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        static readonly IntPtr InvalidHandleValue = new IntPtr(-1);


        /// <summary>
        /// Asks the CLR for the presence of an attached managed debugger, 
        /// and never even bothers to check for the presence of a native debugger.
        /// </summary>
        public static bool CheckDebuggerManagedPresent()
        {
            return System.Diagnostics.Debugger.IsAttached;
        }

        /// <summary>
        /// Asks the kernel for the presence of an attached native debugger, and has no knowledge of managed debuggers.
        /// </summary>
        public static bool CheckDebuggerUnmanagedPresent()
        {
            return IsDebuggerPresent();
        }

        /// <summary>
        /// Checks whether a process is being debugged.
        /// </summary>
        /// <remarks>
        /// The "remote" in CheckRemoteDebuggerPresent does not imply that the debugger
        /// necessarily resides on a different computer; instead, it indicates that the 
        /// debugger resides in a separate and parallel process.
        /// </remarks>
        public static bool CheckRemoteDebugger()
        {
            var isDebuggerPresent = false;
            var bApiRet = CheckRemoteDebuggerPresent(System.Diagnostics.Process.GetCurrentProcess().SafeHandle, ref isDebuggerPresent);

            return bApiRet && isDebuggerPresent;
        }

        public static bool CheckDebugPort()
        {
            var debugPort = new IntPtr(0);
            var status = NtQueryInformationProcess(Process.GetCurrentProcess().Handle,
                ProcessInfoClass.ProcessDebugPort, out debugPort,
                Marshal.SizeOf(debugPort), out var returnLength);

            if (status == NtStatus.Success)
            {
                if (debugPort == new IntPtr(-1))
                {
                    Debug.WriteLine("DebugPort : {0:X}", debugPort);
                    return true;
                }
            }

            return false;
        }

        public static bool DetachFromDebuggerProcess()
        {
            var dwFlags = 0U;

            unsafe
            {
                var ntStatus = NtQueryInformationProcess(Process.GetCurrentProcess().Handle, ProcessInfoClass.ProcessDebugObjectHandle, out var hDebugObject, IntPtr.Size, out _);

                if (ntStatus != NtStatus.Success)
                {
                    return false;
                }

                ntStatus = NtSetInformationDebugObject(hDebugObject, DebugObjectInformationClass.DebugObjectFlags, new IntPtr(&dwFlags), Marshal.SizeOf(dwFlags), out _);

                if (ntStatus != NtStatus.Success)
                {
                    return false;
                }

                ntStatus = NtRemoveProcessDebug(Process.GetCurrentProcess().Handle, hDebugObject);

                if (ntStatus != NtStatus.Success)
                {
                    return false;
                }

                ntStatus = NtClose(hDebugObject);

                if (ntStatus != NtStatus.Success)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckKernelDebugInformation()
        {
            SystemKernelDebuggerInformation pSkdi;

            int retLength;
            NtStatus ntStatus;

            unsafe
            {
                ntStatus = NtQuerySystemInformation(SystemInformationClass.SystemKernelDebuggerInformation, new IntPtr(&pSkdi),
                    Marshal.SizeOf(pSkdi), out retLength);

                if (ntStatus == NtStatus.Success)
                {
                    if (pSkdi.KernelDebuggerEnabled && !pSkdi.KernelDebuggerNotPresent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void HideOsThreads()
        {
            var currentThreads = Process.GetCurrentProcess().Threads;

            foreach (ProcessThread thread in currentThreads)
            {
                Debug.WriteLine("[GetOSThreads]: thread.Id {0:X}", thread.Id);

                var pOpenThread = OpenThread(ThreadAccess.SetInformation, false, (uint)thread.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    Debug.WriteLine("[GetOSThreads]: skipped thread.Id {0:X}", thread.Id);
                    continue;
                }

                if (HideThreadFromDebugger(pOpenThread))
                {
                    Debug.WriteLine("[GetOSThreads]: thread.Id {0:X} hidden from debugger.", thread.Id);
                }

                CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// Hide the thread from debug events.
        /// </summary>
        public static bool HideThreadFromDebugger(IntPtr handle)
        {
            var nStatus = NtSetInformationThread(handle, ThreadInformationClass.ThreadHideFromDebugger, IntPtr.Zero, 0);

            return nStatus == NtStatus.Success;
        }
    }
}
