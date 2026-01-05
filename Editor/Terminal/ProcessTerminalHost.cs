using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        CancellationTokenSource cancellationTokenSource = new();

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
            cancellationTokenSource.Cancel();
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
                process.Start();
                process.Exited += (sender, args) => cancellationTokenSource.Cancel();
                process.EnableRaisingEvents = true;

                this.ConsoleInputStream = process.StandardInput;

                _ = Function(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error: {ex.Message}");
            }
        }

        private async Task Function(CancellationToken token)
        {
            var stdout = process!.StandardOutput;
            var stderr = process.StandardError;

            var stdOutBuffer = new char[256];
            var stdErrBuffer = new char[256];

            var outTask = stdout.ReadAsync(stdOutBuffer).AsTask();
            var errTask = stderr.ReadAsync(stdErrBuffer).AsTask();

            while (!token.IsCancellationRequested)
            {
                var completedTask = await Task.WhenAny(outTask, errTask);
                if (!completedTask.IsCompletedSuccessfully)
                {
                    break;
                }

                if (completedTask == outTask)
                {
                    var read = outTask.Result;
                    if (read > 0)
                    {
                        consoleOutputWriter.Write(stdOutBuffer, 0, read);
                    }
                    outTask = stdout.ReadAsync(stdOutBuffer, token).AsTask();
                }
                else if (completedTask == errTask)
                {
                    var read = errTask.Result;
                    if (read > 0)
                    {
                        consoleOutputWriter.Write(stdErrBuffer, 0, read);
                    }
                    errTask = stderr.ReadAsync(stdErrBuffer, token).AsTask();
                }
            }

            try
            {
                await outTask;
            }
            catch
            {
            }

            try
            {
                await errTask;
            }
            catch
            {
            }
        }
    }
}
