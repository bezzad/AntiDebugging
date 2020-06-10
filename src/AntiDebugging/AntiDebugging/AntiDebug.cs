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
        internal static extern NtStatus NtQueryInformationProcess([In] IntPtr processHandle, [In] ProcessInfoClass processInformationClass, out IntPtr processInformation, [In] int processInformationLength, [Optional] out int returnLength);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtClose([In] IntPtr handle);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtRemoveProcessDebug(IntPtr processHandle, IntPtr debugObjectHandle);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtSetInformationDebugObject([In] IntPtr debugObjectHandle, [In] DebugObjectInformationClass debugObjectInformationClass, [In] IntPtr debugObjectInformation, [In] int debugObjectInformationLength, [Out][Optional] out int returnLength);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtQuerySystemInformation([In] SystemInformationClass systemInformationClass, IntPtr systemInformation, [In] int systemInformationLength, [Out][Optional] out int returnLength);

        static readonly IntPtr InvalidHandleValue = new IntPtr(-1);


        /// <summary>
        /// Asks the CLR for the presence of an attached managed debugger, and never even bothers to check for the presence of a native debugger.
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

        public static int CheckDebugPort()
        {
            NtStatus status;
            IntPtr debugPort = new IntPtr(0);
            int returnLength;

            status = NtQueryInformationProcess(Process.GetCurrentProcess().Handle,
                ProcessInfoClass.ProcessDebugPort, out debugPort,
                Marshal.SizeOf(debugPort), out returnLength);

            if (status == NtStatus.Success)
            {
                if (debugPort == new IntPtr(-1))
                {
                    Console.WriteLine("DebugPort : {0:X}", debugPort);
                    return 1;
                }
            }

            return 0;
        }

        public static bool DetachFromDebuggerProcess()
        {
            IntPtr hDebugObject = InvalidHandleValue;
            var dwFlags = 0U;
            NtStatus ntStatus;
            int retLength1;
            int retLength2;

            unsafe
            {
                ntStatus = NtQueryInformationProcess(Process.GetCurrentProcess().Handle, ProcessInfoClass.ProcessDebugObjectHandle, out hDebugObject, IntPtr.Size, out retLength1);

                if (ntStatus != NtStatus.Success)
                {
                    return false;
                }

                ntStatus = NtSetInformationDebugObject(hDebugObject, DebugObjectInformationClass.DebugObjectFlags, new IntPtr(&dwFlags), Marshal.SizeOf(dwFlags), out retLength2);

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
                ntStatus = NtQuerySystemInformation(SystemInformationClass.SystemKernelDebuggerInformation, new IntPtr(&pSkdi), Marshal.SizeOf(pSkdi), out retLength);

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

    }
}
