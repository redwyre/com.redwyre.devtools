using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System;
using Unity.Burst;

public class DevTools : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private Button buttonNukeLibrary;
    private Button buttonCalculateBurstCacheSize;
    private Button buttonClearBurstCache;
    private Button buttonCloseError;
    private Label labelBurstCacheSize;
    private VisualElement elementErrorMessageContainer;
    private Label labelErrorMessage;

    EditorCoroutine burstCacheSizeCalculator;

    public string ProjectPath => Path.GetDirectoryName(Application.dataPath);
    public string BurstCachePath => Path.Combine(ProjectPath, "Library", "BurstCache");

    [MenuItem("Window/UI Toolkit/DevTools")]
    public static void ShowWindow()
    {
        DevTools wnd = GetWindow<DevTools>();
        wnd.titleContent = new GUIContent("DevTools");
    }

    public void CreateGUI()
    {
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        rootVisualElement.Add(labelFromUXML);

        var children = rootVisualElement.Children().ToList();

        buttonNukeLibrary = rootVisualElement.Q<Button>("NukeLibrary");
        buttonCalculateBurstCacheSize = rootVisualElement.Q<Button>("CalculateBurstCacheSize");
        buttonClearBurstCache = rootVisualElement.Q<Button>("ClearBurstCache");
        buttonCloseError = rootVisualElement.Q<Button>("CloseError");
        labelBurstCacheSize = rootVisualElement.Q<Label>("labelBurstCacheSize");
        elementErrorMessageContainer = rootVisualElement.Q<VisualElement>("ErrorMessageContainer");
        labelErrorMessage = rootVisualElement.Q<Label>("ErrorMessage");

        buttonNukeLibrary.clicked += ButtonNukeLibrary_clicked;
        buttonCalculateBurstCacheSize.clicked += ButtonCalculateBurstCacheSize_clicked;
        buttonClearBurstCache.clicked += ButtonClearBurstCache_clicked;
        buttonCloseError.clicked += ButtonCloseError_clicked;
    }

    private void ButtonCloseError_clicked()
    {
        SetErrorMessage(null);
    }

    private void ButtonClearBurstCache_clicked()
    {
        buttonClearBurstCache.SetEnabled(false);

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
        buttonClearBurstCache.SetEnabled(true);
    }

    private void ButtonCalculateBurstCacheSize_clicked()
    {
        if (burstCacheSizeCalculator == null)
        {
            burstCacheSizeCalculator = EditorCoroutineUtility.StartCoroutine(CalculateBurstCacheSize(), this);
        }
    }

    private IEnumerator CalculateBurstCacheSize()
    {
        buttonCalculateBurstCacheSize.SetEnabled(false);

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
        buttonCalculateBurstCacheSize.SetEnabled(true);
    }

    private void ButtonNukeLibrary_clicked()
    {
        RunScript("Packages/com.redwyre.devtools/Scripts/NukeLibrary.ps1");
        EditorApplication.Exit(0);
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

    public void SetErrorMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            elementErrorMessageContainer.style.display = DisplayStyle.Flex;
            labelErrorMessage.text = message;
        }
        else
        {
            labelErrorMessage.text = "";
            elementErrorMessageContainer.style.display = DisplayStyle.None;
        }
    }
}
