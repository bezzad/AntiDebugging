using System.Runtime.InteropServices;

namespace AntiDebugging
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DebugEvent
    {
        [MarshalAs(UnmanagedType.I4)]
        public DebugEventType dwDebugEventCode;
        public int dwProcessId;
        public int dwThreadId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] bytes;
    }
}
