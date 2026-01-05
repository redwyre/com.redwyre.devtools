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

        IConsole? host;
        TextField? output;
        TextField? input;

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

            //output.RegisterCallback<KeyDownEvent>(evt =>
            //{
            //    if (evt.character != 0)
            //    {
            //        host?.ConsoleInputStream.Write(evt.character);
            //    }
            //}, TrickleDown.TrickleDown);

            input.RegisterCallback<KeyDownEvent>(evt =>
            {
                switch (evt.keyCode)
                {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    SubmitCommand();
                    root.focusController.IgnoreEvent(evt);
                    evt.StopImmediatePropagation();
                    break;
                }
            }, TrickleDown.TrickleDown);

            var processHost = new CmdProcessTerminalHost();
            host = processHost;
        }

        private void SubmitCommand()
        {
            var cmd = input!.value?.Trim();
            if (!Nullable.IsNullOrEmpty(cmd))
            {
                host?.ConsoleInputStream.WriteLine(cmd);
            }

            input.value = string.Empty;
            input.Focus();
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
                    var searchIndex = str.Length - MaxOutputLength + Environment.NewLine.Length;
                    var startIndex = str.IndexOf(Environment.NewLine, searchIndex);
                    startIndex = startIndex < 0 ? searchIndex : startIndex;

                    output.value = str.Substring(startIndex);
                }
                else
                {
                    // trim part of existing and add to new
                    var old = output.value;
                    var limit = MaxOutputLength - str.Length;

                    var searchIndex = old.Length - limit + Environment.NewLine.Length;
                    var startIndex = old.IndexOf(Environment.NewLine, searchIndex);
                    startIndex = startIndex < 0 ? searchIndex : startIndex;

                    output.value = old.Substring(startIndex) + str;
                }
            }
            else if (host.ConsoleOutputStream.Peek() > 0)
            {
                char[] buffer = new char[256];
                var length = host.ConsoleOutputStream.Read(buffer);

                var str = new string(buffer, 0, length);
                output.value += str;
            }

            var end = output.value.Length - 1;
            output.selectIndex = output.cursorIndex = end;
        }
    }
}