using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Linq;
using System.Diagnostics;
using System.Text;

public class DevTools : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

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

        var buttonNukeLibrary = rootVisualElement.Q<Button>("NukeLibrary");

        buttonNukeLibrary.clicked += ButtonNukeLibrary_clicked;
    }

    private void ButtonNukeLibrary_clicked()
    {
        RunScript("Packages/com.redwyre.devtools/Scripts/NukeLibrary.ps1");
        EditorApplication.Exit(0);
    }

    private static void RunScript(string scriptPath)
    {
        scriptPath = Path.GetFullPath(scriptPath);

        var projectPath = Path.GetDirectoryName(Application.dataPath);
        var unityProcess = Process.GetCurrentProcess();
        var unityPath = unityProcess.MainModule.FileName;
        var unityPid = unityProcess.Id;

        var sb = new StringBuilder();
        //sb.Append(" -NonInteractive");
        sb.Append(" -NoProfile");
        sb.Append($" -File \"{scriptPath}\"");
        sb.Append($" -UnityPID {unityPid}");
        sb.Append($" -UnityPath \"{unityPath}\"");
        sb.Append($" -ProjectDir \"{projectPath}\"");

        var proc = Process.Start("pwsh", sb.ToString());
    }
}
