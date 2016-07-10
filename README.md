# ListProcess
List processes and its information tool and library.

## Usage of tool

    ListProcess [/?]
    ListProcess [\\MACHINE-NAME] PROCESS [/m] [/s [DURATION]] [/r INTERVAL]

    /?                   -- Show this help.

    PROCESS:
      /a                 -- List all processes.
      /i PROCESS-ID      -- List by process id.
      /n PROCESS-NAME    -- List by process name. (Case insensitive)
      /N PROCESS-NAME-RE -- List by process name regex. (Case insensitive)

    /m                   -- Show memory info instead of CPU.
    /s [DURATION]        -- Snapshot during DURATION seconds. Default is ÏNT_MAX.
    /r INTERVAL          -- Set snapshot INTERVAL seconds.

## Usage of library

```c#
namespace ProcessList

public static class ProcessLister
    List processes by several ways.

public static IEnumerable<Process> ListAllProcesses(string machineName = null)
    Lists all processes of given machine.

public static IEnumerable<Process> ListByProcessId(int id, string machineName = null)
    Lists by process id.

public static IEnumerable<Process> ListByProcessName(string processName, string machineName = null)
    Lists by the name of process.

public static IEnumerable<Process> ListByProcessNameRegex(
		string pattern,
		RegexOptions options = RegexOptions.IgnoreCase,
		string machineName = null)
    Lists by the regex pattern of process name.

public static IEnumerable<Process> ListByFileName(string fileName, string machineName = null)
    Lists by the file name.

public static IEnumerable<Process> ListByFileNameRegex(
		string pattern,
		RegexOptions options = RegexOptions.IgnoreCase,
		string machineName = null)
    Lists by the regex pattern of file name.
```
