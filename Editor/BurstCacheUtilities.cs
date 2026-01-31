using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Burst;
using UnityEditor;
using UnityEngine;

namespace redwyre.devtools.Editor
{
    [InitializeOnLoad]
    public static class BurstCacheUtilities
    {
        public static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
        public static string BurstCachePath => Path.Combine(ProjectPath, "Library", "BurstCache");

        public static string DeleteMePath => BurstCachePath + ".deleteme";

        static BurstCacheUtilities()
        {
            try
            {
                DeleteOldCache(DeleteMePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error trying to delete old BurstCache delete me folder: {ex.Message}");
            }
        }

        public static void ClearBurstCache()
        {
            var temp = DeleteMePath;

            var wasEnabled = BurstCompiler.Options.EnableBurstCompilation;

            BurstCompiler.Options.EnableBurstCompilation = false;

            try
            {
                DeleteOldCache(DeleteMePath);

                var dir = new DirectoryInfo(BurstCachePath);
                Directory.Move(BurstCachePath, DeleteMePath);
                Directory.CreateDirectory(BurstCachePath);

                DeleteOldCache(DeleteMePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                BurstCompiler.Options.EnableBurstCompilation = wasEnabled;
            }
        }

        private static void DeleteOldCache(string temp)
        {
            if (Directory.Exists(temp))
            {
                try
                {
                    Directory.Delete(temp, true);
                    return;
                }
                catch
                {
                }

                try
                {
                    if (Directory.Exists(temp))
                    {
                        var files = Directory.GetFiles(temp, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try { File.Delete(file); }
                            catch
                            {
                                Debug.Log($"Unable to delete file: {file}");
                            }
                        }
                    }
                }
                catch
                {
                    throw new InvalidOperationException("Temporary delete directory already exists with locked files. Please try again later.");
                }
            }
        }
    }
}
