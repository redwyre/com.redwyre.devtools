using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public abstract class ProcessTerminalHostBase : IHost
{
    readonly StringBuilder buffer = new();
    readonly object syncRoot = new();
    readonly string workingDirectory;
    bool publishScheduled;

    public event Action<string>? OutputChanged;

    public string WorkingDirectory => workingDirectory;
    public abstract string ShellDisplayName { get; }

    protected ProcessTerminalHostBase()
    {
        workingDirectory = ResolveWorkingDirectory();
    }

    public void Write(string text)
    {
        lock (syncRoot)
        {
            buffer.Append(text);
        }

        SchedulePublish();
    }

    public void WriteLine(string line)
    {
        lock (syncRoot)
        {
            buffer.AppendLine(line);
        }

        SchedulePublish();
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            buffer.Clear();
        }

        SchedulePublish();
    }

    public void ExecuteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        WriteLine($"> {command}");
        Task.Run(() => RunProcess(command));
    }

    protected abstract ProcessStartInfo CreateStartInfo(string command);

    protected ProcessStartInfo Configure(ProcessStartInfo startInfo)
    {
        startInfo.WorkingDirectory = workingDirectory;
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        return startInfo;
    }

    private static string ResolveWorkingDirectory()
    {
        if (!string.IsNullOrEmpty(Application.dataPath))
        {
            var directoryInfo = Directory.GetParent(Application.dataPath);
            if (directoryInfo != null)
            {
                return directoryInfo.FullName;
            }
        }

        return Directory.GetCurrentDirectory();
    }

    private void RunProcess(string command)
    {
        try
        {
            var startInfo = CreateStartInfo(command);

            using (var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                process.OutputDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        WriteLine(args.Data);
                    }
                };

                process.ErrorDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        WriteLine(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                WriteLine($"(exit code {process.ExitCode})");
            }
        }
        catch (Exception ex)
        {
            WriteLine($"Error: {ex.Message}");
        }
    }

    private void SchedulePublish()
    {
        bool shouldSchedule;
        lock (syncRoot)
        {
            shouldSchedule = !publishScheduled;
            if (shouldSchedule)
            {
                publishScheduled = true;
            }
        }

        if (shouldSchedule)
        {
            EditorApplication.delayCall += Publish;
        }
    }

    private void Publish()
    {
        string snapshot;
        lock (syncRoot)
        {
            snapshot = buffer.ToString();
            publishScheduled = false;
        }

        OutputChanged?.Invoke(snapshot);
    }
}
