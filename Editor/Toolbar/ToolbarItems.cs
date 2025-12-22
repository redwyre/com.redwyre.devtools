using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Toolbars;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ToolbarItems
{
    [MainToolbarElement("DevTools/Project Settings")]//[ToolbarItem(Icon = "Settings", ToolTip = "Project Settings")]
    public static MainToolbarElement OpenProjectSettings()
    {
        var icon = EditorGUIUtility.IconContent("Settings").image as Texture2D;
        var content = new MainToolbarContent(icon, "Open Project Settings");
        return new MainToolbarButton(content, () => { EditorApplication.ExecuteMenuItem("Edit/Project Settings..."); });
    }

    //[MainToolbarElement("")]//[ToolbarItem(Icon = "Package Manager", ToolTip = "Package Manager")]
    //public static void OpenPackageManager()
    //{
    //    UnityEditor.PackageManager.UI.Window.Open("");
    //}

    [MainToolbarElement("DevTools/Project Folder")]//[ToolbarItem(Icon = "Folder Icon", ToolTip = "Open Folder")]
    public static MainToolbarElement OpenFolder()
    {
        var icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
        var content = new MainToolbarContent(icon, "Open Project Folder");
        return new MainToolbarButton(content, () => { EditorUtility.RevealInFinder(Application.dataPath); });
    }

    [MainToolbarElement("DevTools/Open Terminal")]//[ToolbarItem(Icon = "d_winbtn_win_max", ToolTip = "Open Terminal")]
    public static MainToolbarElement OpenTerminal()
    {
        var icon = EditorGUIUtility.IconContent("d_winbtn_win_max").image as Texture2D;
        var content = new MainToolbarContent(icon, "Open Terminal");
        return new MainToolbarButton(content, () => { Process.Start("wt"); });
    }

    [MainToolbarElement("DevTools/Script Recompile")]//[ToolbarItem(Icon = "cs Script Icon", ToolTip = "Script Recompile")]
    public static MainToolbarElement ScriptRecompile()
    {
        var icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
        var content = new MainToolbarContent(icon, "Recompile Scripts");
        return new MainToolbarButton(content, () => { CompilationPipeline.RequestScriptCompilation(); });
    }

    [MainToolbarElement("DevTools/Domain Reload")]//[ToolbarItem(Icon = "P4_Updating", ToolTip = "Domain Reload")]
    public static MainToolbarElement DomainReload()
    {
        var icon = EditorGUIUtility.IconContent("P4_Updating").image as Texture2D;
        var content = new MainToolbarContent(icon, "Reload Domain");
        return new MainToolbarButton(content, () => { EditorUtility.RequestScriptReload(); });
    }

    [MainToolbarElement("DevTools/Clear PlayerPrefs")]//[ToolbarItem(Icon = "Cancel", ToolTip = "Clear PlayerPrefs")]
    public static MainToolbarElement ClearPlayerPrefs()
    {
        var icon = EditorGUIUtility.IconContent("Cancel").image as Texture2D;
        var content = new MainToolbarContent(icon, "Clear PlayerPrefs");
        return new MainToolbarButton(content, () => { PlayerPrefs.DeleteAll(); });
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
        menu.AddItem(new GUIContent(NativeLeakDetectionMode.Enabled.ToString()), false, () =>
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
            MainToolbar.Refresh("DevTools/Memory Leak Detection");
        });
        menu.AddItem(new GUIContent(NativeLeakDetectionMode.EnabledWithStackTrace.ToString()), false, () =>
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
            MainToolbar.Refresh("DevTools/Memory Leak Detection");
        });
        menu.AddItem(new GUIContent(NativeLeakDetectionMode.Disabled.ToString()), false, () =>
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
            MainToolbar.Refresh("DevTools/Memory Leak Detection");
        });
        menu.DropDown(dropDownRect);
    }
}
