using redwyre.Core.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace redwyre.DevTools.Editor.UnsavedChanges
{
    [EditorWindowTitle(title = Title)]
    public class UnsavedChangesWindow : EditorWindow
    {
        public const string Title = "Unsaved Changes";
        public const string UxmlPath = "Packages/com.redwyre.devtools/Editor/UnsavedChanges/UnsavedChangesWindow.uxml";
        public const string UssPath = "Packages/com.redwyre.devtools/Editor/UnsavedChanges/UnsavedChangesWindow.uss";
        public const string RowUxmlPath = "Packages/com.redwyre.devtools/Editor/UnsavedChanges/UnsavedChangesRow.uxml";

        private UnsavedChangesViewModel viewModel = new UnsavedChangesViewModel();
        private VisualTreeAsset? rowTemplate;

        private Button? refreshButton;
        private Button? saveAllButton;
        private Button? revertAllButton;
        private Label? statusLabel;
        private Label? emptyStateLabel;

        private readonly Dictionary<AssetKind, SectionElements> sections = new Dictionary<AssetKind, SectionElements>();

        [MenuItem("Window/Tools/Unsaved Changes", priority = 10002)]
        public static void ShowUnsavedChangesWindow()
        {
            GetWindow<UnsavedChangesWindow>(Title);
        }

        private void OnEnable()
        {
            viewModel.propertyChanged += OnViewModelPropertyChanged;

            BuildUI();
            RefreshData();
        }

        private void OnDisable()
        {
            viewModel.propertyChanged -= OnViewModelPropertyChanged;
        }

        private void BuildUI()
        {
            rootVisualElement.Clear();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            visualTree.CloneTree(rootVisualElement);

            rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RowUxmlPath);

            refreshButton = rootVisualElement.Q<Button>("RefreshButton");
            saveAllButton = rootVisualElement.Q<Button>("SaveAllButton");
            revertAllButton = rootVisualElement.Q<Button>("RevertAllButton");
            statusLabel = rootVisualElement.Q<Label>("StatusLabel");
            emptyStateLabel = rootVisualElement.Q<Label>("EmptyStateLabel");

            sections[AssetKind.Scene] = new SectionElements
            {
                Root = rootVisualElement.Q<VisualElement>("ScenesSection"),
                HeaderLabel = rootVisualElement.Q<Label>("ScenesHeader"),
                ListContainer = rootVisualElement.Q<VisualElement>("ScenesList")
            };

            sections[AssetKind.Prefab] = new SectionElements
            {
                Root = rootVisualElement.Q<VisualElement>("PrefabsSection"),
                HeaderLabel = rootVisualElement.Q<Label>("PrefabsHeader"),
                ListContainer = rootVisualElement.Q<VisualElement>("PrefabsList")
            };

            sections[AssetKind.Settings] = new SectionElements
            {
                Root = rootVisualElement.Q<VisualElement>("SettingsSection"),
                HeaderLabel = rootVisualElement.Q<Label>("SettingsHeader"),
                ListContainer = rootVisualElement.Q<VisualElement>("SettingsList")
            };

            sections[AssetKind.Other] = new SectionElements
            {
                Root = rootVisualElement.Q<VisualElement>("OtherSection"),
                HeaderLabel = rootVisualElement.Q<Label>("OtherHeader"),
                ListContainer = rootVisualElement.Q<VisualElement>("OtherList")
            };

            viewModel.RefreshCommand = new RelayCommand(OnRefreshData);
            viewModel.SaveAllCommand = new RelayCommand(OnSaveAll);
            viewModel.RevertAllCommand = new RelayCommand(OnRevertAll);

            refreshButton.clicked += () => viewModel.RefreshCommand.Execute(null);
            saveAllButton.clicked += () => viewModel.SaveAllCommand.Execute(null);
            revertAllButton.clicked += () => viewModel.RevertAllCommand.Execute(null);

            UpdateStatusBar();
        }

        private void OnViewModelPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(UnsavedChangesViewModel.TotalCount) ||
                e.propertyName == nameof(UnsavedChangesViewModel.ScenesCount) ||
                e.propertyName == nameof(UnsavedChangesViewModel.PrefabsCount) ||
                e.propertyName == nameof(UnsavedChangesViewModel.SettingsCount) ||
                e.propertyName == nameof(UnsavedChangesViewModel.OtherCount))
            {
                UpdateStatusBar();
                UpdateHeaders();
            }
            else if (e.propertyName == nameof(UnsavedChangesViewModel.EntriesVersion))
            {
                RebuildSections();
            }
        }

        private void RefreshData()
        {
            var entries = CollectDirtyEntries();
            viewModel.SetEntries(entries);
            RebuildSections();
            UpdateStatusBar();
            UpdateHeaders();
        }

        private List<DirtyEntry> CollectDirtyEntries()
        {
            var entries = new Dictionary<string, DirtyEntry>();

            CollectDirtyScenes(entries);
            CollectDirtyAssets(entries);

            return entries.Values
                .OrderBy(e => e.Kind)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void CollectDirtyScenes(Dictionary<string, DirtyEntry> entries)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                {
                    continue;
                }

                if (!scene.isDirty)
                {
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(scene.path);
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(scene.path);
                var icon = AssetDatabase.GetCachedIcon(scene.path) as Texture2D;

                var entry = new DirtyEntry
                {
                    Guid = new GUID(guid),
                    Path = scene.path,
                    Name = name,
                    Icon = icon,
                    Kind = AssetKind.Scene,
                    IsDirty = true,
                    TooltipGuid = guid,
                };

                var key = guid + scene.path;
                if (!entries.ContainsKey(key))
                {
                    entries.Add(key, entry);
                }
            }
        }

        private void CollectDirtyAssets(Dictionary<string, DirtyEntry> entries)
        {
            var objects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                if (!AssetDatabase.Contains(obj))
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (!(path.StartsWith("Assets/", StringComparison.Ordinal) ||
                      path.StartsWith("ProjectSettings/", StringComparison.Ordinal) ||
                      path.StartsWith("UserSettings/", StringComparison.Ordinal)))
                {
                    continue;
                }

                if (!EditorUtility.IsDirty(obj))
                {
                    continue;
                }

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long _))
                {
                    continue;
                }

                if (entries.ContainsKey(guid))
                {
                    continue;
                }

                var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
                var kind = DetermineAssetKind(path, obj, mainType);
                var entry = new DirtyEntry
                {
                    Guid = new GUID(guid),
                    Path = path,
                    Name = Path.GetFileNameWithoutExtension(path),
                    Icon = AssetDatabase.GetCachedIcon(path) as Texture2D,
                    Kind = kind,
                    IsDirty = true,
                    TooltipGuid = guid
                };

                entries.Add(guid, entry);
            }
        }

        private AssetKind DetermineAssetKind(string path, UnityEngine.Object obj, Type mainType)
        {
            if (obj is SceneAsset)
            {
                return AssetKind.Scene;
            }

            if (path.StartsWith("ProjectSettings/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("UserSettings/", StringComparison.OrdinalIgnoreCase))
            {
                return AssetKind.Settings;
            }

            var prefabType = PrefabUtility.GetPrefabAssetType(obj);
            if (prefabType != PrefabAssetType.NotAPrefab)
            {
                return AssetKind.Prefab;
            }

            return AssetKind.Other;
        }

        private void UpdateHeaders()
        {
            UpdateHeaderForKind(AssetKind.Scene, "Scenes");
            UpdateHeaderForKind(AssetKind.Prefab, "Prefabs");
            UpdateHeaderForKind(AssetKind.Settings, "Settings");
            UpdateHeaderForKind(AssetKind.Other, "Other Assets");
        }

        private void UpdateHeaderForKind(AssetKind kind, string baseLabel)
        {
            if (!sections.TryGetValue(kind, out var section))
            {
                return;
            }

            var count = GetEntries(kind).Count;
            section.HeaderLabel.text = count > 0 ? $"{baseLabel} ({count})" : baseLabel;
            if (section.Root != null)
            {
                section.Root.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RebuildSections()
        {
            foreach (var pair in sections)
            {
                if (pair.Value.ListContainer != null)
                {
                    pair.Value.ListContainer.Clear();
                    var entries = GetEntries(pair.Key);
                    foreach (var entry in entries)
                    {
                        pair.Value.ListContainer.Add(CreateRow(entry));
                    }
                }
            }

            UpdateHeaders();
            UpdateStatusBar();
        }

        private IReadOnlyList<DirtyEntry> GetEntries(AssetKind kind)
        {
            return kind switch
            {
                AssetKind.Scene => viewModel.Scenes,
                AssetKind.Prefab => viewModel.Prefabs,
                AssetKind.Settings => viewModel.Settings,
                _ => viewModel.Other,
            };
        }

        private VisualElement CreateRow(DirtyEntry entry)
        {
            var row = (rowTemplate ?? throw new InvalidOperationException("Row template not loaded")).CloneTree();
            row.tooltip = entry.TooltipGuid;
            row.dataSource = entry;

            return row;
        }

        private void UpdateStatusBar()
        {
            if (statusLabel != null)
            {
                statusLabel.text = $"Scenes: {viewModel.ScenesCount} | Prefabs: {viewModel.PrefabsCount} | Settings: {viewModel.SettingsCount} | Other: {viewModel.OtherCount} | Total: {viewModel.TotalCount}";
            }

            if (emptyStateLabel != null)
            {
                emptyStateLabel.style.display = viewModel.TotalCount == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnRefreshData(object? parameter)
        {
            RefreshData();
        }

        private void OnSaveAll(object? parameter)
        {
            foreach (var entry in viewModel.AllEntries.ToList())
            {
                entry.Save();
            }

            RefreshData();
        }

        private void OnRevertAll(object? parameter)
        {
            if (!EditorUtility.DisplayDialog("Revert All", "Revert all dirty assets? This cannot be undone.", "Revert All", "Cancel"))
            {
                return;
            }

            foreach (var entry in viewModel.AllEntries.ToList())
            {
                entry.Revert();
            }

            RefreshData();
        }

        private struct SectionElements
        {
            public VisualElement Root;
            public Label HeaderLabel;
            public VisualElement ListContainer;
        }
    }
}