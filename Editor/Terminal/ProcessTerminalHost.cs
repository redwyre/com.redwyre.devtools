using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace redwyre.DevTools.Editor.Terminal
{
    public abstract class ProcessTerminalHostBase : IConsole, IDisposable
    {
        readonly string workingDirectory;

        public string WorkingDirectory => workingDirectory;

        public abstract string DisplayName { get; }

        Process? process = null;

        public bool SupportsVT100 => false;

        public bool HasTerminated => process?.HasExited ?? true;

        public StreamReader ConsoleOutputStream { get; private set; }

        public StreamWriter ConsoleInputStream { get; private set; }

        MemoryStream consoleOutputMerged = new MemoryStream(1024);
        MemoryStream consoleInputDummy = new MemoryStream();

        StreamWriter consoleOutputWriter;

        protected ProcessTerminalHostBase(string command)
        {
            workingDirectory = ResolveWorkingDirectory();

            consoleOutputWriter = new StreamWriter(consoleOutputMerged) { AutoFlush = true };

            ConsoleOutputStream = new StreamReader(consoleOutputMerged);
            ConsoleInputStream = new StreamWriter(consoleInputDummy);

            RunProcess(command);
        }

        public void Dispose()
        {
            process?.Dispose();
            consoleOutputWriter.Dispose();
            ConsoleOutputStream.Dispose();
            ConsoleInputStream.Dispose();
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
            if (process != null)
            {
                Debug.LogWarning("Process is already running.");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo(command)
                {
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        consoleOutputWriter.WriteLine(args.Data);
                    }
                };

                process.ErrorDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        consoleOutputWriter.WriteLine(args.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                this.ConsoleInputStream = process.StandardInput;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error: {ex.Message}");
            }
        }
    }
}