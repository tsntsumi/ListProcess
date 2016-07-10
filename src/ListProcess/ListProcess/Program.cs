/*
The MIT License (MIT)

Copyright (c) 2016 tsntsumi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ProcessList;

namespace ListProcess
{
    /// <summary>
    /// Main class.
    /// List process and its information tool.
    /// </summary>
    class MainClass
    {
        /// <summary>
        /// List process delegate.
        /// </summary>
        private delegate IEnumerable<Process> ListProcessDelegate(string process, string machineName);

        /// <summary>
        /// Listing options.
        /// </summary>
        private class ListingOptions
        {
            /// <summary>
            /// Gets or sets the name of the machine to list process.
            /// </summary>
            /// <value>The name of the machine.</value>
            public string MachineName { get; set; }
            /// <summary>
            /// Gets or sets the list process delegate.
            /// </summary>
            /// <value>The list process delegate.</value>
            public ListProcessDelegate ListProcess { get; set; }
            /// <summary>
            /// Gets or sets the listing process id, name or pattern.
            /// </summary>
            /// <value>The listing process.</value>
            public string ListingProcess { get; set; }
            /// <summary>
            /// Gets or sets the show memory info flag.
            /// </summary>
            /// <value>The memory info enabled.</value>
            public bool IsMemoryInfoEnabled { get; set; }
            /// <summary>
            /// Gets or sets the help requirement flag.
            /// </summary>
            /// <value>The help required.</value>
            public bool IsHelpRequired { get; set; }
            /// <summary>
            /// Gets or sets the duration of the snapshot.
            /// </summary>
            /// <value>The duration of the snapshot.</value>
            public int SnapshotDuration { get; set; } = 0;
            /// <summary>
            /// Gets or sets the snapshot interval.
            /// </summary>
            /// <value>The snapshot interval.</value>
            public int SnapshotInterval { get; set; } = 1;
        }

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            ListingOptions options = null;

            try
            {
                options = AnalyzeCommandLine(args);
                if (options == null || options.IsHelpRequired)
                {
                    string help = @"List Process Infomations.";
                    Console.WriteLine("{0}", help);
                    Usage();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Usage();
                Environment.Exit(1);
            }

            IEnumerable<Process> processes = null;

            try
            {
                processes = options.ListProcess(options.ListingProcess, options.MachineName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Environment.Exit(2);
            }

            ListProcessInfo(processes, options);
        }

        /// <summary>
        /// Analyzes the command line options and arguments.
        /// </summary>
        /// <returns>The listing options.</returns>
        /// <param name="args">Arguments.</param>
        private static ListingOptions AnalyzeCommandLine(string[] args)
        {
            var options = new ListingOptions();

            if (args.Length == 0)
            {
                options.IsHelpRequired = true;
                return options;
            }

            var processOptions = new List<string>();
            int index = 0;

            for (index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                //
                // process options
                //
                case "/a":
                case "-a":
                    options.ListProcess = ListAllProcesses;
                    processOptions.Add(args[index]);
                    break;
                case "/i":
                case "-i":
                    if (args.Length - index < 2)
                    {
                        throw new ArgumentException("No process id specified.");
                    }
                    else
                    {
                        processOptions.Add(args[index]);
                        options.ListProcess = ListByProcessId;
                        options.ListingProcess = args[++index];
                        int id;
                        if (!int.TryParse(options.ListingProcess, out id))
                        {
                            throw new ArgumentException("Bad process id");
                        }
                    }
                    break;
                case "/n":
                case "-n":
                    if (args.Length - index < 2)
                    {
                        throw new ArgumentException("No process name specified.");
                    }
                    processOptions.Add(args[index]);
                    options.ListProcess = ListByProcessName;
                    options.ListingProcess = args[++index];
                    break;
                case "/N":
                case "-N":
                    if (args.Length - index < 2)
                    {
                        throw new ArgumentException("No process name regex specified.");
                    }
                    processOptions.Add(args[index]);
                    options.ListProcess = ListByProcessNameRegex;
                    options.ListingProcess = args[++index];
                    break;
                //
                // display type option
                //
                case "/m":
                case "-m":
                    options.IsMemoryInfoEnabled = true;
                    break;
                //
                // snapshot options
                //
                case "/s":
                case "-s":
                    if (args.Length - index < 2 || Regex.IsMatch(args[index + 1], @"^[-/]"))
                    {
                        options.SnapshotDuration = int.MaxValue;
                    }
                    else
                    {
                        int duration;
                        if (!int.TryParse(args[++index], out duration))
                        {
                            throw new ArgumentException("Bad snapshot duration.");
                        }
                        if (duration < 1)
                        {
                            throw new ArgumentException("Snapshot duration must be positive.");
                        }
                        options.SnapshotDuration = duration;
                    }
                    break;
                case "/r":
                case "-r":
                    if (args.Length - index < 2)
                    {
                        throw new ArgumentException("No snapshot interval specified.");
                    }
                    else
                    {
                        int interval;
                        if (!int.TryParse(args[++index], out interval))
                        {
                            throw new ArgumentException("Bad snapshot interval.");
                        }
                        if (interval < 1)
                        {
                            throw new ArgumentException("Snapshot interval must be positive.");
                        }
                        options.SnapshotInterval = interval;
                    }
                    break;
                //
                // help option
                //
                case "-?":
                case "/?":
                case "-h":
                case "/h":
                case "--help":
                    options.IsHelpRequired = true;
                    break;
                //
                // argument
                //
                default:
                    if (args[index].StartsWith("/", StringComparison.CurrentCulture) ||
                        args[index].StartsWith("-", StringComparison.CurrentCulture))
                    {
                        throw new ArgumentException(string.Format("Unknown option: {0}.", args[index]));
                    }
                    if (!args[index].StartsWith(@"\\", StringComparison.CurrentCulture))
                    {
                        throw new ArgumentException(string.Format("Bad machine name: {0}.", args[index]));
                    }
                    options.MachineName = args[index];
                    break;
                }
            }
            if (processOptions.Count() > 1)
            {
                throw new ArgumentException("Multiple process options specified.");
            }
            return options;
        }

        /// <summary>
        /// Lists the process info.
        /// </summary>
        /// <param name="processes">Processes.</param>
        /// <param name="options">Options.</param>
        static void ListProcessInfo(IEnumerable<Process> processes, ListingOptions options)
        {
            var etor = processes.GetEnumerator();
            if (!etor.MoveNext())
            {
                Console.WriteLine("No process found");
                return;
            }
            string[] header;

            if (options.IsMemoryInfoEnabled)
            {
                header = new string[] {
                    "Pid", "Name", "VM", "WS", "Priv", /* "Priv Pk", "Faults",*/ "NonP", "Page"
                };
            }
            else
            {
                header = new string[] {
                    "Pid", "Name", "Pri", "Thd", "Hnd", "Priv", "CPU Time", "Elapsed Time"
                };
            }
            Console.WriteLine(string.Join("\t", header));

            DateTime start = DateTime.Now;
            for (;;)
            {
                foreach (var p in processes)
                {
                    if (p.HasExited)
                    {
                        continue;
                    }
                    if (p.Modules.Count <= 0)
                    {
                        Console.WriteLine("{0}", p.Id);
                        continue;
                    }
                    if (options.IsMemoryInfoEnabled)
                    {
                        Console.WriteLine("{0}\t{1}\t{2:F}\t{3:F}\t{4:F}\t{5:F}\t{6:F}",
                                          p.Id,
                                          p.ProcessName,
                                          p.VirtualMemorySize64 / 1000.0,
                                          p.WorkingSet64 / 1000.0,
                                          p.PrivateMemorySize64 / 1000.0,
                                          p.NonpagedSystemMemorySize64 / 1000.0,
                                          p.PagedMemorySize64 / 1000.0);
                    }
                    else
                    {
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5:F}\t{6:F}\t{7}",
                                          p.Id,
                                          p.ProcessName,
                                          p.BasePriority,
                                          p.Threads.Count,
                                          p.HandleCount,
                                          p.PrivateMemorySize64 / 1000.0,
                                          p.TotalProcessorTime.TotalMilliseconds / 1000.0,
                                          (DateTime.Now - p.StartTime));
                    }
                }
                if ((DateTime.Now - start).Seconds >= options.SnapshotDuration)
                {
                    break;
                }
                System.Threading.Thread.Sleep(options.SnapshotInterval * 1000);
            }
        }

        /// <summary>
        /// Lists all processes.
        /// </summary>
        /// <returns>The all processes.</returns>
        /// <param name="dummy">Dummy.</param>
        /// <param name="machineName">Machine name.</param>
        private static IEnumerable<Process> ListAllProcesses(string dummy, string machineName)
        {
            return ProcessLister.ListAllProcesses(machineName);
        }

        /// <summary>
        /// Lists by the process identifier.
        /// </summary>
        /// <returns>The process.</returns>
        /// <param name="processId">Process identifier.</param>
        /// <param name="machineName">Machine name.</param>
        private static IEnumerable<Process> ListByProcessId(string processId, string machineName)
        {
            int id;
            if (!int.TryParse(processId, out id))
            {
                throw new ArgumentException("Bad process id", nameof(id));
            }
            return ProcessLister.ListByProcessId(id, machineName);
        }

        /// <summary>
        /// Lists by the name of the process.
        /// </summary>
        /// <returns>The process.</returns>
        /// <param name="processName">Process name.</param>
        /// <param name="machineName">Machine name.</param>
        private static IEnumerable<Process> ListByProcessName(string processName, string machineName)
        {
            return ProcessLister.ListByProcessName(processName, machineName);
        }

        /// <summary>
        /// Lists by the regex pattern of the process name.
        /// </summary>
        /// <returns>The process.</returns>
        /// <param name="processNameRegex">Process name regex.</param>
        /// <param name="machineName">Machine name.</param>
        private static IEnumerable<Process> ListByProcessNameRegex(string processNameRegex, string machineName)
        {
            return ProcessLister.ListByProcessNameRegex(processNameRegex, RegexOptions.IgnoreCase, machineName);
        }

        /// <summary>
        /// Usage this tool.
        /// </summary>
        public static void Usage()
        {
            string[] usage = {
                @"Usage: ListProcess [/?]",
                @"       ListProcess [\\MACHINE-NAME] PROCESS [/m] [/s [DURATION]] [/r INTERVAL]",
                @"",
                @"  /?                   -- Show this help.",
                @"",
                @"  PROCESS:",
                @"    /a                 -- List all processes.",
                @"    /i PROCESS-ID      -- List by process id.",
                @"    /n PROCESS-NAME    -- List by process name. (Case insensitive)",
                @"    /N PROCESS-NAME-RE -- List by process name regex. (Case insensitive)",
                @"",
                @"  /m                   -- Show memory info instead of CPU.",
                @"  /s [DURATION]        -- Snapshot during DURATION seconds. Default is ÏNT_MAX.",
                @"  /r INTERVAL          -- Set snapshot INTERVAL seconds."
            };

            foreach (var line in usage)
            {
                Console.WriteLine("{0}", line);
            }
        }
    }
}
