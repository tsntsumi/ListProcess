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
using System.Text;
using System.Text.RegularExpressions;

namespace ProcessList
{
    /// <summary>
    /// List processes by sevral way.
    /// </summary>
    public static class ProcessLister
    {
        /// <summary>
        /// Lists all processes of given machine.
        /// </summary>
        /// <returns>The all processes.</returns>
        /// <param name="machineName">Machine name. List current machine process if null.</param>
        public static IEnumerable<Process> ListAllProcesses(string machineName = null)
        {
            if (string.IsNullOrEmpty(machineName) || machineName == @"\\.")
            {
                return Process.GetProcesses();
            }
            return Process.GetProcesses(machineName);
        }

        /// <summary>
        /// Lists by process id.
        /// </summary>
        /// <returns>The process of given identifier.</returns>
        /// <param name="id">Process id.</param>
        /// <param name="machineName">Machine name.</param>
        public static IEnumerable<Process> ListByProcessId(int id, string machineName = null)
        {
            Process p = null;
            if (string.IsNullOrEmpty(machineName) || machineName == @"\\.")
            {
                p = Process.GetProcessById(id);
            }
            else
            {
                p = Process.GetProcessById(id, machineName);
            }
            return new Process[] { p };
        }

        /// <summary>
        /// Lists by the name of process.
        /// </summary>
        /// <returns>The processes of given name.</returns>
        /// <param name="processName">Process name.</param>
        /// <param name="machineName">Machine name.</param>
        public static IEnumerable<Process> ListByProcessName(string processName, string machineName = null)
        {
            if (string.IsNullOrEmpty(machineName) || machineName == @"\\.")
            {
                return Process.GetProcessesByName(processName);
            }
            return Process.GetProcessesByName(processName, machineName);
        }

        /// <summary>
        /// Lists by the regex pattern of process name.
        /// </summary>
        /// <returns>The processes of given name regex.</returns>
        /// <param name="pattern">The regex pattern of process name.</param>
        /// <param name="options">The regex options.</param>
        /// <param name="machineName">Machine name.</param>
        public static IEnumerable<Process> ListByProcessNameRegex(string pattern, RegexOptions options = RegexOptions.IgnoreCase, string machineName = null)
        {
            var re = new Regex(pattern, options);
            var processes = ListAllProcesses(machineName);
            return from p in processes where p.Modules.Count > 0 && re.IsMatch(p.ProcessName) select p;
        }

        /// <summary>
        /// Lists by the file name.
        /// </summary>
        /// <returns>The processes of given file name.</returns>
        /// <param name="fileName">File name.</param>
        /// <param name="machineName">Machine name.</param>
        public static IEnumerable<Process> ListByFileName(string fileName, string machineName = null)
        {
            var processes = ListAllProcesses(machineName);
            return from p in processes where p.Modules.Count > 0 && p.MainModule.FileName == fileName select p;
        }

        /// <summary>
        /// Lists by the regex pattern of file name.
        /// </summary>
        /// <returns>The processes of given file name regex.</returns>
        /// <param name="pattern">The regex pattern of file name.</param>
        /// <param name="options">The regex options.</param>
        /// <param name="machineName">Machine name.</param>
        public static IEnumerable<Process> ListByFileNameRegex(string pattern, RegexOptions options = RegexOptions.IgnoreCase, string machineName = null)
        {
            var re = new Regex(pattern, options);
            var processes = ListAllProcesses(machineName);
            return from p in processes where p.Modules.Count > 0 && re.IsMatch(p.MainModule.FileName) select p;
        }
    }
}
