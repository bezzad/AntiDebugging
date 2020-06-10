using System;

namespace AntiDebugging.WinStructs
{
    [Flags]
    public enum DebugObjectInformationClass : int
    {
        DebugObjectFlags = 1,
        MaxDebugObjectInfoClass
    }
}