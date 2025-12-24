using System.Diagnostics;
using Unity.CodeEditor;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Toolbars;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ToolbarItems
{
    [MainToolbarElement("DevTools/Open C# Project")]
    public static MainToolbarElement OpenCSProject()
    {
        var editor = CodeEditor.Editor.CurrentInstallation.Name ?? "";

        var icon = EditorGUIUtility.IconContent("d_boo Script Icon").image as Texture2D;
        var tooltip = string.IsNullOrEmpty(editor) ? "Open C# Project" : $"Open C# Project with {editor}";
        var content = new MainToolbarContent(icon, tooltip);
        return new MainToolbarButton(content, static () => { EditorApplication.ExecuteMenuItem("Assets/Open C# Project"); });
    }

    [MainToolbarElement("DevTools/Project Settings")]
    public static MainToolbarElement OpenProjectSettings()
    {
        var icon = EditorGUIUtility.IconContent("Settings").image as Texture2D;
        var content = new MainToolbarContent(icon, "Open Project Settings");
        return new MainToolbarButton(content, static () => { EditorApplication.ExecuteMenuItem("Edit/Project Settings..."); });
    }

    [MainToolbarElement("DevTools/Project Folder")]
    public static MainToolbarElement OpenFolder()
    {
        var icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
        var content = new MainToolbarContent(icon, "Open Project Folder");
        return new MainToolbarButton(content, static () => { EditorUtility.RevealInFinder(Application.dataPath); });
    }

    [MainToolbarElement("DevTools/Open Terminal")]
    public static MainToolbarElement OpenTerminal()
    {
        var icon = EditorGUIUtility.IconContent("d_winbtn_win_max").image as Texture2D;
        var content = new MainToolbarContent(icon, "Open Terminal");
        return new MainToolbarButton(content, static () => { Process.Start("wt"); });
    }

    [MainToolbarElement("DevTools/Script Recompile")]
    public static MainToolbarElement ScriptRecompile()
    {
        var icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
        var content = new MainToolbarContent(icon, "Recompile Scripts");
        return new MainToolbarButton(content, static () => { CompilationPipeline.RequestScriptCompilation(); });
    }

    [MainToolbarElement("DevTools/Domain Reload")]//[ToolbarItem(Icon = "P4_Updating", ToolTip = "Domain Reload")]
    public static MainToolbarElement DomainReload()
    {
        var icon = EditorGUIUtility.IconContent("P4_Updating").image as Texture2D;
        var content = new MainToolbarContent(icon, "Reload Domain");
        return new MainToolbarButton(content, static () => { EditorUtility.RequestScriptReload(); });
    }

    [MainToolbarElement("DevTools/Clear PlayerPrefs")]
    public static MainToolbarElement ClearPlayerPrefs()
    {
        var icon = EditorGUIUtility.IconContent("Cancel").image as Texture2D;
        var content = new MainToolbarContent(icon, "Clear PlayerPrefs");
        return new MainToolbarButton(content, static () => { PlayerPrefs.DeleteAll(); });
    }

    //[ToolbarItem(ToolTip = "Memory Leak Detection", Icons = new[] {
    //        "Packages/com.redwyre.custom-toolbar/Editor Default Resources/Icons/MemoryLeakDetection_0.png",
    //        "Packages/com.redwyre.custom-toolbar/Editor Default Resources/Icons/MemoryLeakDetection_1.png",
    //        "Packages/com.redwyre.custom-toolbar/Editor Default Resources/Icons/MemoryLeakDetection_2.png" })]
    [MainToolbarElement("DevTools/Memory Leak Detection")]
    public static MainToolbarElement MemoryLeakDetection()
    {
        var icon = EditorGUIUtility.IconContent("UnityLogo").image as Texture2D;
        var content = new MainToolbarContent(NativeLeakDetection.Mode.ToString(), icon, "Memory Leak Detection");
        return new MainToolbarDropdown(content, ShowDropdownMenu);
    }

    private static void ShowDropdownMenu(Rect dropDownRect)
    {
        var menu = new GenericMenu();

        var iconEnabled = EditorGUIUtility.IconContent("d_scenevis_visible_hover").image as Texture2D;
        var iconEnabledWithStackTrace = EditorGUIUtility.IconContent("d_scenevis_visible-mixed_hover").image as Texture2D;
        var iconDisabled = EditorGUIUtility.IconContent("d_scenevis_hidden_hover").image as Texture2D;

        menu.AddItem(new GUIContent(NativeLeakDetectionMode.Enabled.ToString(), iconEnabled),
            NativeLeakDetection.Mode == NativeLeakDetectionMode.Enabled, () =>
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
            MainToolbar.Refresh("DevTools/Memory Leak Detection");
        });
        menu.AddItem(new GUIContent(NativeLeakDetectionMode.EnabledWithStackTrace.ToString(), iconEnabledWithStackTrace),
            NativeLeakDetection.Mode == NativeLeakDetectionMode.EnabledWithStackTrace, () =>
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
            MainToolbar.Refresh("DevTools/Memory Leak Detection");
        });
        menu.AddItem(new GUIContent(NativeLeakDetectionMode.Disabled.ToString(), iconDisabled),
            NativeLeakDetection.Mode == NativeLeakDetectionMode.Disabled, () =>
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
            MainToolbar.Refresh("DevTools/Memory Leak Detection");
        });
        menu.DropDown(dropDownRect);
    }
}
