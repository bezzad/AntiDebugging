namespace AntiDebugging
{
    public static class Constants
    {
        public static string ActiveRemoteDebuggerFound { get; } = "ActiveRemoteDebuggerFound";
        public static string ActiveDebuggerFound { get; } = "ActiveDebuggerFound";
        public static string ActiveUnmanagedDebuggerFound { get; } = "ActiveUnmanagedDebuggerFound";
        public static string ActiveKernelDebuggerFound { get; } = "ActiveKernelDebuggerFound";
        public static string ApplicationRunningOnEmulation { get; } = "ApplicationRunningOnEmulation";
        public static string ApplicationRunningOnSandbox { get; } = "ApplicationRunningOnSandbox";
        public static string ApplicationRunningOnVirtualMachine { get; } = "ApplicationRunningOnVirtualMachine";
    }
}
