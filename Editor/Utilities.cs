using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.CodeEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace redwyre.DevTools
{
    public static class Utilities
    {
        public static void CleanProjectsAndSolutions()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);

            try
            {
                var sb = new StringBuilder(1000);

                var files = Directory.GetFiles(projectRoot, "*.csproj");
                foreach (var file in files)
                {
                    sb.AppendLine($"Deleting project file '{file}'");
                    File.Delete(file);
                }

                if (sb.Length > 0)
                {
                    Debug.Log(sb.ToString());
                    sb.Clear();
                }

                files = Directory.GetFiles(projectRoot, "*.sln");
                foreach (var file in files)
                {
                    sb.AppendLine($"Deleting solution file '{file}'");
                    File.Delete(file);
                }

                files = Directory.GetFiles(projectRoot, "*.slnx");
                foreach (var file in files)
                {
                    sb.AppendLine($"Deleting solution file '{file}'");
                    File.Delete(file);
                }

                if (sb.Length > 0)
                {
                    Debug.Log(sb.ToString());
                    sb.Clear();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to delete files, stopping {e.Message}");
                throw e;
            }
        }

        public static void BuildProjectsAndSolution()
        {
            var codeEditor = CodeEditor.Editor.CurrentCodeEditor;

            if (codeEditor == null)
                return;

            codeEditor.SyncAll();
        }
    }
}
