using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
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
            {
                Debug.LogWarning("No code editor is currently set, cannot build projects and solution.");
                return;
            }

            codeEditor.SyncAll();
        }

        public static string SerializeUnityObjectToAssetString(UnityEngine.Object unityObject)
        {
            string tempPath = string.Empty;

            try
            {
                tempPath = Path.GetTempFileName();
                if (SerializeUnityObjectToFile(unityObject, tempPath))
                {
                    return File.ReadAllText(tempPath);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            return string.Empty;
        }

        public static bool SerializeUnityObjectToFile(UnityEngine.Object unityObject, string path)
        {
            if (unityObject == null)
            {
                Debug.LogWarning("SerializeUnityObjectToFile called with null object.");
                return false;
            }

            try
            {
                InternalEditorUtility.SaveToSerializedFileAndForget(new[] { unityObject }, path, true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize '{unityObject.name}' ({unityObject.GetType().Name}) to .asset format: {e.Message}");
                return false;
            }
        }
    }
}
