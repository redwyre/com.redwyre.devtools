using System;
using System.Collections.Generic;
using redwyre.Core.MVVM;
using Unity.Properties;

namespace redwyre.DevTools.Editor.UnsavedChanges
{
    public class UnsavedChangesViewModel : ObservableObject
    {
        private readonly Dictionary<AssetKind, List<DirtyEntry>> entriesByKind = new()
        {
            { AssetKind.Scene, new() },
            { AssetKind.Prefab, new() },
            { AssetKind.Settings, new() },
            { AssetKind.Other, new() }
        };

        private readonly List<DirtyEntry> allEntries = new();
        private int entriesVersion;
        private bool isBusy;

        public event Action RefreshRequested;
        public event Action SaveAllRequested;
        public event Action RevertAllRequested;
        public event Action<DirtyEntry> SaveRequested;
        public event Action<DirtyEntry> RevertRequested;
        public event Action<DirtyEntry> PingRequested;

        public IReadOnlyList<DirtyEntry> AllEntries => allEntries;
        public IReadOnlyList<DirtyEntry> Scenes => entriesByKind[AssetKind.Scene];
        public IReadOnlyList<DirtyEntry> Prefabs => entriesByKind[AssetKind.Prefab];
        public IReadOnlyList<DirtyEntry> Settings => entriesByKind[AssetKind.Settings];
        public IReadOnlyList<DirtyEntry> Other => entriesByKind[AssetKind.Other];

        public int TotalCount => allEntries.Count;
        public int ScenesCount => Scenes.Count;
        public int PrefabsCount => Prefabs.Count;
        public int SettingsCount => Settings.Count;
        public int OtherCount => Other.Count;
        public int EntriesVersion => entriesVersion;

        [CreateProperty]
        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
        }

        public void RequestRefresh() => RefreshRequested?.Invoke();
        public void RequestSaveAll() => SaveAllRequested?.Invoke();
        public void RequestRevertAll() => RevertAllRequested?.Invoke();
        public void RequestSave(DirtyEntry entry) => SaveRequested?.Invoke(entry);
        public void RequestRevert(DirtyEntry entry) => RevertRequested?.Invoke(entry);
        public void RequestPing(DirtyEntry entry) => PingRequested?.Invoke(entry);

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

        public void SetBusy(bool busy)
        {
            IsBusy = busy;
        }
    }
}