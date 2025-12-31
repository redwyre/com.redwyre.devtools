using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UIElements;

[EditorWindowTitle(icon = Icon, title = Title)]
public class TerminalWindow : EditorWindow
{
    public const string Title = "Terminal";
    public const string Icon = "winbtn_win_max";
    public const string WindowUxml = "Packages/com.redwyre.devtools/Editor/Terminal/TerminalWindow.uxml";

    private IHost? host;

    [MenuItem("Window/Tools/Terminal", priority = 10000)]
    public static void ShowTerminalWindow()
    {
        TerminalWindow wnd = GetWindow<TerminalWindow>(Title);
        //var iconContent = EditorGUIUtility.IconContent(Icon);
        //wnd.titleContent = new GUIContent(Title, iconContent.image);
    }

    public void CreateGUI()
    {
        var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WindowUxml);

        VisualElement root = rootVisualElement;
        template.CloneTree(root);

        var clear = root.Q<Button>("Clear");
        var help = root.Q<Button>("Help");
        var input = root.Q<TextField>("Input");
        var output = root.Q<TextField>("Output");
        var submit = root.Q<Button>("Submit");

        clear.clicked += () => host?.Clear();

        help.clicked += () => host?.ExecuteCommand("help");

        input.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                SubmitCommand(input);
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.UpArrow)
            {
                NavigateHistory(-1, input);
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.DownArrow)
            {
                NavigateHistory(+1, input);
                evt.StopImmediatePropagation();
            }
        });

        submit.clicked += () => SubmitCommand(input);

        var processHost = new CmdProcessTerminalHost();
        host = processHost;

        processHost.OutputChanged += s =>
        {
            output.value = s;
            output.cursorIndex = s.Length - 1;
            input.Focus();
        };

        host.WriteLine("Console Terminal ready.");
        host.WriteLine($"Shell: {processHost.ShellDisplayName}");
        host.WriteLine($"Working directory: {processHost.WorkingDirectory}");
        host.WriteLine("Type any console command and press Enter.");
    }

    private readonly List<string> history = new List<string>();
    private int historyIndex = -1;

    private void SubmitCommand(TextField inputField)
    {
        var cmd = inputField.value?.Trim();
        if (!Nullable.IsNullOrEmpty(cmd))
        {
            host?.ExecuteCommand(cmd);
            history.Add(cmd);
            historyIndex = history.Count - 1;
        }

        inputField.value = string.Empty;
        inputField.Focus();
    }

    private void NavigateHistory(int direction, TextField textField)
    {
        if (history.Count == 0)
        {
            return;
        }

        historyIndex += direction;
        if (historyIndex < 0)
        {
            historyIndex = 0;
        }
        else if (historyIndex >= history.Count)
        {
            historyIndex = history.Count - 1;
        }

        if (historyIndex >= 0 && historyIndex < history.Count)
        {
            textField.value = history[historyIndex];
        }
        else
        {
            textField.value = string.Empty;
        }
    }
}
