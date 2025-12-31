using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEditor.Compilation;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System;
using Unity.Burst;

public partial class DevTools : EditorWindow
{
    EditorCoroutine? burstCacheSizeCalculator;

    public string ProjectPath => Path.GetDirectoryName(Application.dataPath);
    public string BurstCachePath => Path.Combine(ProjectPath, "Library", "BurstCache");

    [MenuItem("Window/UI Toolkit/DevTools")]
    public static void ShowWindow()
    {
        DevTools wnd = GetWindow<DevTools>();
        wnd.titleContent = new GUIContent("DevTools");
    }

    partial void OnCreateGUI()
    {
        ForceRecompile.clicked += ForceRecompile_clicked;
        ReloadDomain.clicked += ReloadDomain_clicked;
        NukeScripts.clicked += NukeScripts_clicked;
        NukeLibrary.clicked += NukeLibrary_clicked;
        RestartBurst.clicked += RestartBurst_clicked;
        CalculateBurstCacheSize.clicked += CalculateBurstCacheSize_clicked;
        ClearBurstCache.clicked += ClearBurstCache_clicked;
        CloseError.clicked += CloseError_clicked;
    }

    private void ForceRecompile_clicked()
    {
        CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
    }

    private void ReloadDomain_clicked()
    {
        EditorUtility.RequestScriptReload();
    }

    private void RestartBurst_clicked()
    {
        var wasEnabled = BurstCompiler.Options.EnableBurstCompilation;

        BurstCompiler.Options.EnableBurstCompilation = false;
        BurstCompiler.Options.EnableBurstCompilation = wasEnabled;
    }

    private void NukeScripts_clicked()
    {
        var proceed = EditorUtility.DisplayDialog("Nuke Scripts", "This will shut down Unity, delete the script related contents of the library folder, and then restart Unity. It will take a while to reimport all the assets. Do you wish to proceed?", "Yes", "No");

        if (!proceed) { return; }

        RunScript("Packages/com.redwyre.devtools/Scripts/NukeScripts.ps1");
        EditorApplication.Exit(0);
    }

    private void NukeLibrary_clicked()
    {
        var proceed = EditorUtility.DisplayDialog("Nuke Library", "This will shut down Unity, delete the contents of the library folder, and then restart Unity. It will take a while to reimport all the assets. Do you wish to proceed?", "Yes", "No");

        if (!proceed) { return; }

        RunScript("Packages/com.redwyre.devtools/Scripts/NukeLibrary.ps1");
        EditorApplication.Exit(0);
    }

    private void CloseError_clicked()
    {
        SetErrorMessage(null);
    }

    private void ClearBurstCache_clicked()
    {
        ClearBurstCache.SetEnabled(false);

        var temp = BurstCachePath + ".deleteme";

        var wasEnabled = BurstCompiler.Options.EnableBurstCompilation;

        BurstCompiler.Options.EnableBurstCompilation = false;

        try
        {
            var dir = new DirectoryInfo(BurstCachePath);
            Directory.Move(BurstCachePath, temp);
            Directory.CreateDirectory(BurstCachePath);
            Directory.Delete(temp, true);

            labelBurstCacheSize.text = PrettyBytes(0);
        }
        catch (Exception ex)
        {
            SetErrorMessage(ex.Message);
        }

        BurstCompiler.Options.EnableBurstCompilation = wasEnabled;
        ClearBurstCache.SetEnabled(true);
    }

    private void CalculateBurstCacheSize_clicked()
    {
        if (burstCacheSizeCalculator == null)
        {
            burstCacheSizeCalculator = EditorCoroutineUtility.StartCoroutine(CalculateBurstCacheSizeFunc(), this);
        }
    }

    private IEnumerator CalculateBurstCacheSizeFunc()
    {
        CalculateBurstCacheSize.SetEnabled(false);

        var fileNames = Directory.EnumerateFiles(BurstCachePath, "*", SearchOption.AllDirectories);

        var totalFiles = 0;
        var totalSize = 0L;

        foreach (var fileName in fileNames)
        {
            var fileInfo = new FileInfo(fileName);
            totalSize += fileInfo.Length;
            ++totalFiles;

            if (totalFiles % 100 == 0)
            {
                labelBurstCacheSize.text = PrettyBytes(totalSize);
                yield return null;
            }
        }

        labelBurstCacheSize.text = PrettyBytes(totalSize);
        burstCacheSizeCalculator = null;
        CalculateBurstCacheSize.SetEnabled(true);
    }

    private void RunScript(string scriptPath)
    {
        scriptPath = Path.GetFullPath(scriptPath);

        var unityProcess = Process.GetCurrentProcess();
        var unityPath = unityProcess.MainModule.FileName;
        var unityPid = unityProcess.Id;

        var sb = new StringBuilder();
        //sb.Append(" -NonInteractive");
        sb.Append(" -NoProfile");
        sb.Append($" -File \"{scriptPath}\"");
        sb.Append($" -UnityPID {unityPid}");
        sb.Append($" -UnityPath \"{unityPath}\"");
        sb.Append($" -ProjectDir \"{ProjectPath}\"");

        var proc = Process.Start("pwsh", sb.ToString());
    }

    public string PrettyBytes(long bytes)
    {
        string postfix = "bytes";
        float number = bytes;

        if (number > 1024) { number /= 1024; postfix = "KiB"; }
        if (number > 1024) { number /= 1024; postfix = "MiB"; }
        if (number > 1024) { number /= 1024; postfix = "GiB"; }
        return $"{number:G03}{postfix}";
    }

    public void SetErrorMessage(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            ErrorMessageContainer.style.display = DisplayStyle.Flex;
            ErrorMessage.text = message;
        }
        else
        {
            ErrorMessage.text = "";
            ErrorMessageContainer.style.display = DisplayStyle.None;
        }
    }
}
