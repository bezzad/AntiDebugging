using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AntiDebugging
{
    public class Scanner
    {
        private static HashSet<string> BadProcessNameList { get; set; }
        private static HashSet<string> BadWindowTextList { get; set; }


        public static void ScanAndKill(Action continueWith = null)
        {
            try
            {
                if (Scan(true) != 0)
                {
                    continueWith?.Invoke();
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Simple scanner for "bad" processes (debuggers) using .NET code only. (for now)
        /// </summary>
        private static int Scan(bool killBadProcess)
        {
            var isBadProcess = 0;

            if (BadProcessNameList.Any() != true ||
                BadWindowTextList.Any() != true)
            {
                Init();
            }

            var processList = Process.GetProcesses();

            foreach (var process in processList)
            {
                if (BadProcessNameList.Contains(process.ProcessName.ToLower()) ||
                    BadWindowTextList.Contains(process.MainWindowTitle.ToLower()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("BAD PROCESS FOUND: " + process.ProcessName);

                    isBadProcess++;

                    if (killBadProcess)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine(ex);
                            break;
                        }
                    }
                }
            }

            return isBadProcess;
        }

        /// <summary>
        /// Populate "database" with process names/window names.
        /// Using HashSet for maximum performance
        /// </summary>
        private static void Init()
        {
            if (BadProcessNameList?.Any() == true && BadWindowTextList?.Any() == true)
            {
                return;
            }

            BadProcessNameList = new HashSet<string>
            {
                "procmon64",
                "codecracker",
                "ida",
                "idag",
                "idaw",
                "idaq",
                "idau",
                "scylla",
                "de4dot",
                "de4dotmodded",
                "protection_id",
                "ollydbg",
                "x64dbg",
                "x32dbg",
                "x96dbg",
                "x64netdumper",
                "petools",
                "dnspy",
                "windbg",
                "reshacker",
                "simpleassembly",
                "process hacker",
                "process monitor",
                "qt5core",
                "importREC",
                "immunitydebugger",
                "megadumper",
                "dump",
                "dbgclr",
                "wireshark",
                "hxd"
            };

            BadWindowTextList = new HashSet<string>
            {
                "ollydbg",
                "ida",
                "disassembly",
                "scylla",
                "debug",
                "[cpu",
                "immunity",
                "windbg",
                "x32dbg",
                "x64dbg",
                "x96dbg",
                "import reconstructor",
                "dumper"
            };
        }
    }
}
