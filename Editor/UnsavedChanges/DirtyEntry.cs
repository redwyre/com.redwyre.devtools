using redwyre.Core.MVVM;
using System;
using System.Windows.Input;
using Unity.Properties;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace redwyre.DevTools.Editor.UnsavedChanges
{
    public partial class DirtyEntry : ObservableObject
    {
        [CreateProperty] public GUID Guid { get; set; }

        [CreateProperty] public string Path { get; set; } = string.Empty;

        [CreateProperty] public string Name { get; set; } = string.Empty;

        [CreateProperty] public Texture2D? Icon { get; set; }

        [CreateProperty] public AssetKind Kind { get; set; }

        [CreateProperty] public bool IsDirty { get; set; }

        [CreateProperty] public string TooltipGuid { get; set; } = string.Empty;

        [CreateProperty] public bool IsScene => Kind == AssetKind.Scene;

        [CreateProperty] public bool IsPrefab => Kind == AssetKind.Prefab;

        [CreateProperty] public bool IsSettings => Kind == AssetKind.Settings;

        [CreateProperty] public ICommand SaveCommand { get; } = new RelayCommand(OnSave);

        [CreateProperty] public ICommand RevertCommand { get; } = new RelayCommand(OnRevert);

        [CreateProperty] public ICommand PingCommand { get; } = new RelayCommand(OnPing, CanExecutePing);

        private static bool CanExecutePing(object? arg)
        {
            if (arg is not DirtyEntry entry)
                return false;

            return entry.Kind != AssetKind.Settings;
        }

        private static void OnPing(object? obj)
        {
            if (obj is not DirtyEntry entry)
                return;

            entry.Ping();
        }

        public void Ping()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }

        private static void OnRevert(object? obj)
        {
            if (obj is not DirtyEntry entry)
                return;

            entry.Revert();
        }

        public bool Revert()
        {
            AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceUpdate);
            return true;
        }

        private static void OnSave(object? obj)
        {
            if (obj is not DirtyEntry entry)
                return;

            entry.Save();
        }

        public bool Save()
        {
            if (this.IsScene)
            {
                var scene = SceneManager.GetSceneByPath(Path);
                if (!scene.IsValid())
                {
                    Debug.LogError($"Failed to save scene at path '{Path}' because it could not be found in the currently open scenes.");
                    //scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);
                }

                if (scene.IsValid())
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }
            else
            {
                var asset = AssetDatabase.LoadAssetByGUID(Guid, typeof(UnityEngine.Object));
                if (asset != null)
                {
                    AssetDatabase.SaveAssetIfDirty(asset);
                }
            }

            return true;
        }
    }
}
