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
        TextElement? output;
        TextField? input;
        ScrollView? scrollView;

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
            output = root.Q<TextElement>("Output");
            scrollView = root.Q<ScrollView>("OutputScrollView");

            clear.clicked += () =>
            {
                var oldText = output.text;
                var lastNewLine = oldText.LastIndexOf(Environment.NewLine);
                output.text = lastNewLine >= 0 ? oldText.Substring(lastNewLine + Environment.NewLine.Length) : "";
            };

            help.clicked += () => SubmitCommand("help");

            //root.RegisterCallback<NavigationSubmitEvent>(evt =>
            //{
            //    root.focusController.IgnoreEvent(evt);
            //    evt.StopImmediatePropagation();
            //}, TrickleDown.TrickleDown);

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

            output.selection.OnCursorIndexChange += ScrollToCursor;

            output.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                MoveCursorToEnd();
            });

            //input.RegisterCallback<KeyDownEvent>(evt =>
            //{
            //    switch (evt.keyCode)
            //    {
            //        case KeyCode.Return:
            //        case KeyCode.KeypadEnter:
            //            SubmitCommand();
            //            root.focusController.IgnoreEvent(evt);
            //            evt.StopImmediatePropagation();
            //            break;
            //    }
            //}, TrickleDown.TrickleDown);

            var processHost = new CmdProcessTerminalHost();
            host = processHost;

            input.Focus();
        }

        private void SubmitCommand()
        {
            if (input is null)
            {
                return;
            }

            var cmd = input.value.Trim();
            SubmitCommand(cmd);

            input.value = string.Empty;
            input.Focus();
        }

        private void SubmitCommand(string cmd)
        {
            host?.ConsoleInputStream.WriteLine(cmd);
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
                // if using memory stream, need to get buffer and reset the stream to re-use it
                var length = ms.Length;
                var str = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)length);
                ms.SetLength(0);
                AppendOutputBuffer(str);
            }
            else if (host.ConsoleOutputStream.Peek() > 0)
            {
                // for normal stream, read in chunks
                char[] buffer = new char[256];
                do
                {
                    var length = host.ConsoleOutputStream.Read(buffer);
                    var str = new string(buffer, 0, length);
                    AppendOutputBuffer(str);
                } while (host.ConsoleOutputStream.Peek() > 0);
            }

            ScrollToCursor();
        }

        private void MoveCursorToEnd()
        {
            var end = output!.text.Length;
            output.selection.cursorIndex = end;
            output.selection.selectIndex = end;
        }

        private void ScrollToCursor()
        {
            var selection = (ITextSelection)output!;
            var pos = selection.cursorPosition;

            const int paddingX = 20;

            var size = scrollView!.contentViewport.contentRect.size;

            var maxScrollX = scrollView.horizontalScroller.highValue;
            var maxScrollY = scrollView.verticalScroller.highValue;

            var min = scrollView.scrollOffset;
            var max = scrollView.scrollOffset + size;

            var scroll = min;
            if (pos.x < min.x)
            {
                scroll.x = Math.Max(0, pos.x - paddingX);
            }
            else if (pos.x > max.x)
            {
                scroll.x = Math.Min(maxScrollX, pos.x - size.x + paddingX);
            }

            if (pos.y < min.y)
            {
                scroll.y = Math.Max(0, pos.y - paddingX);
            }
            else if (pos.y > max.y)
            {
                scroll.y = Math.Min(maxScrollY, pos.y - size.y + paddingX);
            }

            if (scroll != min)
            {
                //Debug.Log($"Scrolling to cursor {scroll}");
                scrollView.scrollOffset = scroll;
            }
        }

        private void AppendOutputBuffer(string str)
        {
            if (output!.text.Length + str.Length < MaxOutputLength)
            {
                // total length is within limit
                output.text += str;
            }
            else if (str.Length > MaxOutputLength)
            {
                // new string by itself exceeds limit, take last part
                var searchIndex = str.Length - MaxOutputLength + Environment.NewLine.Length;
                var startIndex = str.IndexOf(Environment.NewLine, searchIndex);
                startIndex = startIndex < 0 ? searchIndex : startIndex;

                output.text = str.Substring(startIndex);
            }
            else
            {
                // trim part of existing and add to new
                var old = output.text;
                var limit = MaxOutputLength - str.Length;

                var searchIndex = old.Length - limit + Environment.NewLine.Length;
                var startIndex = old.IndexOf(Environment.NewLine, searchIndex);
                startIndex = startIndex < 0 ? searchIndex : startIndex;

                output.text = old.Substring(startIndex) + str;
            }
        }
    }
}
