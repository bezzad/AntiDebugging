using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AntiDebugging
{
    public class Scanner
    {
        private static readonly HashSet<string> BadProcessNameList = new HashSet<string>();
        private static readonly HashSet<string> BadWindowTextList = new HashSet<string>();

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
        private static int Scan(bool killProcess)
        {
            int isBadProcess = 0;

            if (BadProcessNameList.Count == 0 && BadWindowTextList.Count == 0)
            {
                Init();
            }

            var processList = Process.GetProcesses();

            foreach (var process in processList)
            {
                if (BadProcessNameList.Contains(process.ProcessName) || BadWindowTextList.Contains(process.MainWindowTitle))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("BAD PROCESS FOUND: " + process.ProcessName);

                    isBadProcess = 1;

                    if (killProcess)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (System.ComponentModel.Win32Exception w32Ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("Win32Exception: " + w32Ex.Message);

                            break;
                        }
                        catch (System.NotSupportedException nex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("NotSupportedException: " + nex.Message);

                            break;
                        }
                        catch (System.InvalidOperationException ioex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("InvalidOperationException: " + ioex.Message);

                            break;
                        }
                    }

                    break;
                }
            }

            return isBadProcess;
        }

        /// <summary>
        /// Populate "database" with process names/window names.
        /// Using HashSet for maximum performance
        /// </summary>
        private static bool Init()
        {
            if (BadProcessNameList.Count > 0 && BadWindowTextList.Count > 0)
            {
                return true;
            }

            BadProcessNameList.Add("ollydbg");
            BadProcessNameList.Add("ida");
            BadProcessNameList.Add("ida64");
            BadProcessNameList.Add("idag");
            BadProcessNameList.Add("idag64");
            BadProcessNameList.Add("idaw");
            BadProcessNameList.Add("idaw64");
            BadProcessNameList.Add("idaq");
            BadProcessNameList.Add("idaq64");
            BadProcessNameList.Add("idau");
            BadProcessNameList.Add("idau64");
            BadProcessNameList.Add("scylla");
            BadProcessNameList.Add("scylla_x64");
            BadProcessNameList.Add("scylla_x86");
            BadProcessNameList.Add("protection_id");
            BadProcessNameList.Add("x64dbg");
            BadProcessNameList.Add("x32dbg");
            BadProcessNameList.Add("windbg");
            BadProcessNameList.Add("reshacker");
            BadProcessNameList.Add("ImportREC");
            BadProcessNameList.Add("IMMUNITYDEBUGGER");
            BadProcessNameList.Add("MegaDumper");

            BadWindowTextList.Add("OLLYDBG");
            BadWindowTextList.Add("ida");
            BadWindowTextList.Add("disassembly");
            BadWindowTextList.Add("scylla");
            BadWindowTextList.Add("Debug");
            BadWindowTextList.Add("[CPU");
            BadWindowTextList.Add("Immunity");
            BadWindowTextList.Add("WinDbg");
            BadWindowTextList.Add("x32dbg");
            BadWindowTextList.Add("x64dbg");
            BadWindowTextList.Add("Import reconstructor");
            BadWindowTextList.Add("MegaDumper");
            BadWindowTextList.Add("MegaDumper 1.0 by CodeCracker / SnD");

            return false;
        }
    }
}
