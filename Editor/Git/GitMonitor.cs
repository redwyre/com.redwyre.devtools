using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GitMonitor
{
    private const string IndexLock = "index.lock";
    static GitMonitor Instance;

    public FileSystemWatcher watcher;
    public bool isGitOperationInProgress = false;
    public bool wasGitOperationInProgressLastFrame = false;
    public int progressId = -1;

    static GitMonitor()
    {
        Debug.Log("GitMonitor initialized");

        Instance = new GitMonitor();
    }

    public GitMonitor()
    {
        var gitPath = Path.Combine(Application.dataPath, "..", ".git");
        watcher = new FileSystemWatcher(gitPath, IndexLock);
        watcher.IncludeSubdirectories = false;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.EnableRaisingEvents = true;
        Debug.Log("GitMonitor watching path: " + gitPath);

        EditorApplication.update += OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (isGitOperationInProgress != wasGitOperationInProgressLastFrame)
        {
            wasGitOperationInProgressLastFrame = isGitOperationInProgress;

            if (isGitOperationInProgress)
            {
                AssetDatabase.StartAssetEditing();
                progressId = Progress.Start("Git operation in progress, pausing AssetDatabase updates...", options: Progress.Options.Indefinite);
            }
            else
            {
                AssetDatabase.StopAssetEditing();
                Progress.Remove(progressId);
                progressId = -1;
            }
        }

        if (isGitOperationInProgress)
        {
            Progress.Report(progressId, 0.0f);
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name != IndexLock)
        {
            Debug.Log($"Ignored change: {e.Name}");
            return;
        }

        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Created:
                Debug.Log("Git operation started...");
                isGitOperationInProgress = true;
                break;
            case WatcherChangeTypes.Deleted:
                Debug.Log("Git operation finished.");
                isGitOperationInProgress = false;
                break;
        }
    }
}
