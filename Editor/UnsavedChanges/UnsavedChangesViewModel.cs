using System;
using System.Collections.Generic;
using System.Windows.Input;
using redwyre.Core.MVVM;
using Unity.Properties;

namespace redwyre.DevTools.Editor.UnsavedChanges
{
    public class UnsavedChangesViewModel : ObservableObject
    {
        readonly Dictionary<AssetKind, List<DirtyEntry>> entriesByKind = new()
        {
            { AssetKind.Scene, new() },
            { AssetKind.Prefab, new() },
            { AssetKind.Settings, new() },
            { AssetKind.Other, new() }
        };

        readonly List<DirtyEntry> allEntries = new();
        int entriesVersion;

        public ICommand? RefreshCommand { get; set; }

        public ICommand? SaveAllCommand { get; set; }

        public ICommand? RevertAllCommand { get; set; }

        public IReadOnlyList<DirtyEntry> AllEntries => allEntries;
        public IReadOnlyList<DirtyEntry> Scenes => entriesByKind[AssetKind.Scene];
        public IReadOnlyList<DirtyEntry> Prefabs => entriesByKind[AssetKind.Prefab];
        public IReadOnlyList<DirtyEntry> Settings => entriesByKind[AssetKind.Settings];
        public IReadOnlyList<DirtyEntry> Other => entriesByKind[AssetKind.Other];

        [CreateProperty]
        public int TotalCount => allEntries.Count;

        [CreateProperty]
        public int ScenesCount => Scenes.Count;

        [CreateProperty]
        public int PrefabsCount => Prefabs.Count;

        [CreateProperty]
        public int SettingsCount => Settings.Count;

        [CreateProperty]
        public int OtherCount => Other.Count;

        [CreateProperty]
        public int EntriesVersion => entriesVersion;

        public void SetEntries(IEnumerable<DirtyEntry> entries)
        {
            foreach (var pair in entriesByKind)
            {
                pair.Value.Clear();
            }

            allEntries.Clear();

            foreach (var entry in entries)
            {
                allEntries.Add(entry);
                entriesByKind[entry.Kind].Add(entry);
            }

            entriesVersion++;
            Notify(nameof(EntriesVersion));
            Notify(nameof(TotalCount));
            Notify(nameof(ScenesCount));
            Notify(nameof(PrefabsCount));
            Notify(nameof(SettingsCount));
            Notify(nameof(OtherCount));
        }
    }
}