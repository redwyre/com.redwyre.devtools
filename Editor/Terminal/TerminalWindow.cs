using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UIElements;

namespace redwyre.DevTools.Editor.Terminal
{
    [EditorWindowTitle(icon = Icon, title = Title)]
    public class TerminalWindow : EditorWindow
    {
        public const string Title = "Terminal";
        public const string Icon = "winbtn_win_max";
        public const string WindowUxml = "Packages/com.redwyre.devtools/Editor/Terminal/TerminalWindow.uxml";

        public const int MaxOutputLength = 10000;

        readonly List<string> history = new List<string>();
        int historyIndex = -1;

        IConsole? host;
        TextField? output;
        TextField? input;
        Button? submit;

        [MenuItem("Window/Tools/Terminal", priority = 10000)]
        public static void ShowTerminalWindow()
        {
            TerminalWindow wnd = GetWindow<TerminalWindow>(Title);
            //var iconContent = EditorGUIUtility.IconContent(Icon);
            //wnd.titleContent = new GUIContent(Title, iconContent.image);
        }

        public void OnDestroy()
        {
            CloseTerminal();
        }

        void CloseTerminal()
        {
            output?.SetEnabled(false);
            input?.SetEnabled(false);
            submit?.SetEnabled(false);

            if (host is IDisposable disposable)
            {
                disposable.Dispose();
            }

            host = null;
        }

        public void CreateGUI()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WindowUxml);

            VisualElement root = rootVisualElement;
            template.CloneTree(root);

            var clear = root.Q<Button>("Clear");
            var help = root.Q<Button>("Help");
            input = root.Q<TextField>("Input");
            output = root.Q<TextField>("Output");
            submit = root.Q<Button>("Submit");

            //clear.clicked += () => host?.Clear();

            //help.clicked += () => host?.ExecuteCommand("help");

            root.RegisterCallback<NavigationSubmitEvent>(evt =>
            {
                root.focusController.IgnoreEvent(evt);
                evt.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);

            input.RegisterCallback<NavigationSubmitEvent>(evt =>
            {
                SubmitCommand();
                evt.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);

            input.RegisterCallback<KeyDownEvent>(evt =>
            {
                switch (evt.keyCode)
                {
                    case KeyCode.UpArrow:
                        NavigateHistory(-1, input);
                        evt.StopImmediatePropagation();
                        break;

                    case KeyCode.DownArrow:
                        NavigateHistory(+1, input);
                        evt.StopImmediatePropagation();
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        SubmitCommand();
                        root.focusController.IgnoreEvent(evt);
                        evt.StopImmediatePropagation();
                        break;
                }
            }, TrickleDown.TrickleDown);

            submit.clicked += () =>
            {
                SubmitCommand();
            };

            var processHost = new CmdProcessTerminalHost();
            host = processHost;

            //host.WriteLine("Console Terminal ready.");
            //host.WriteLine($"Shell: {processHost.DisplayName}");
            //host.WriteLine($"Working directory: {processHost.WorkingDirectory}");
            //host.WriteLine("Type any console command and press Enter.");
        }

        private void SubmitCommand()
        {
            var cmd = input!.value?.Trim();
            if (!Nullable.IsNullOrEmpty(cmd))
            {
                host?.ConsoleInputStream.WriteLine(cmd);
                history.Add(cmd);
                historyIndex = history.Count - 1;
            }

            input.value = string.Empty;
            input.Focus();
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

        public void Update()
        {
            if (host is null || output is null)
            {
                return;
            }

            if (host.HasTerminated)
            {
                CloseTerminal();
                return;
            }

            if (host.ConsoleOutputStream.BaseStream is MemoryStream ms && ms.Position > 0)
            {
                var length = ms.Length;

                var str = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)length);

                ms.SetLength(0);

                if (output.value.Length + str.Length < MaxOutputLength)
                {
                    // total length is within limit
                    output.value += str;
                }
                else if (str.Length > MaxOutputLength)
                {
                    // new string by itself exceeds limit, take last part

                    var searchIndex = str.Length - MaxOutputLength;
                    var startIndex = str.IndexOf(Environment.NewLine, searchIndex);
                    startIndex = startIndex < 0 ? searchIndex : startIndex;

                    output.value = str.Substring(startIndex);
                }
                else
                {
                    // trim part of existing and add to new
                    var old = output.value;
                    var limit = MaxOutputLength - str.Length;

                    var searchIndex = old.Length - limit;
                    var startIndex = old.IndexOf(Environment.NewLine, searchIndex);
                    startIndex = startIndex < 0 ? searchIndex : startIndex;

                    output.value = old.Substring(startIndex) + str;
                }
            }
            else if (host.ConsoleOutputStream.Peek() > 0)
            {
                char[] buffer = new char[256];
                var length = host.ConsoleOutputStream.Read(buffer);
                //var span = new Span<char>(buffer, 0, length);

                var str = new string(buffer, 0, length);
                output.value += str;
            }


            var end = output.value.Length - 1;
            output.selectIndex = output.cursorIndex = end;
            output.SelectRange(end, end);
        }
    }
}