using System;
using System.Runtime.InteropServices;

namespace MoonShell
{
    internal static class Imports
    {
        [DllImport("Kernel32.dll")]
        public static extern bool GenerateConsoleCtrlEvent(CTRL_EVENT dwCtrlEvent, UInt32 dwProcessGroupId);
    }

    internal enum CTRL_EVENT : uint
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1
    }
}