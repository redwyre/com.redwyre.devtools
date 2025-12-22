using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
static class ToolbarItems_BuildProfile
{
    private const string MenuPath_BuildActiveBuildProfile = "DevTools/Build Active BuildProfile";
    private const string MenuPath_SelectBuildProfile = "DevTools/Select Build Profile";

    static BuildProfile activeProfile = null;

    static ToolbarItems_BuildProfile()
    {
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        var newActiveProfile = BuildProfile.GetActiveBuildProfile();
        if (activeProfile != newActiveProfile)
        {
            activeProfile = newActiveProfile;
            Debug.Log($"Build Profile changed to: {(newActiveProfile != null ? newActiveProfile.name : "None")}");
            MainToolbar.Refresh(MenuPath_BuildActiveBuildProfile);
            MainToolbar.Refresh(MenuPath_SelectBuildProfile);
        }
    }

    [MainToolbarElement(MenuPath_BuildActiveBuildProfile)]
    public static MainToolbarElement BuildActiveBuildProfile()
    {
        MainToolbarContent content;
        var icon = EditorGUIUtility.IconContent("BuildSettings.Standalone On").image as Texture2D;

        if (activeProfile == null)
        {
            icon = EditorGUIUtility.IconContent("TestFailed").image as Texture2D;
            content = new MainToolbarContent("", icon, "Build active BuildProfile");
        }
        else
        {
            content = new MainToolbarContent(activeProfile.name, icon, "Build active BuildProfile");
        }

        return new MainToolbarButton(content, OnBuildActiveBuildProfileClicked) { populateContextMenu = PopulateBuildProfileContextMenu };
    }

    private static void PopulateBuildProfileContextMenu(DropdownMenu menu)
    {
        var guids = AssetDatabase.FindAssetGUIDs("t:BuildProfile");
        foreach (var guid in guids)
        {
            var profile = AssetDatabase.LoadAssetByGUID<BuildProfile>(guid);
            menu.AppendAction(profile.name,
                (action) =>
                {
                    var profile = (BuildProfile)action.userData;
                    BuildProfile.SetActiveBuildProfile(profile);
                }, (action) =>
                {
                    return (BuildProfile)action.userData == activeProfile ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                },
                profile);
        }
    }

    private static void OnBuildActiveBuildProfileClicked()
    {
        var options = new BuildPlayerWithProfileOptions
        {
            buildProfile = activeProfile,
            locationPathName = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/", activeProfile.name, $"{Application.productName}.exe")),
            options = BuildOptions.None,
        };
        BuildPipeline.BuildPlayer(options);
    }

    [MainToolbarElement(MenuPath_SelectBuildProfile)]
    public static MainToolbarElement SelectBuildProfile()
    {
        var icon = EditorGUIUtility.IconContent("BuildSettings.Standalone On").image as Texture2D;
        var content = new MainToolbarContent(activeProfile != null ? activeProfile.name : "None", "Build from Build Profile");
        return new MainToolbarDropdown(content, OpenDropdown);
    }

    static void OpenDropdown(Rect dropDownRect)
    {
        var menu = new GenericMenu();
        var guids = AssetDatabase.FindAssetGUIDs("t:BuildProfile");
        foreach (var guid in guids)
        {
            var profile = AssetDatabase.LoadAssetByGUID<BuildProfile>(guid);
            menu.AddItem(new GUIContent(profile.name), profile == activeProfile,
                static (userData) =>
                {
                    var profile = (BuildProfile)userData;
                    BuildProfile.SetActiveBuildProfile(profile);
                },
                profile);
        }
        menu.DropDown(dropDownRect);
    }
}