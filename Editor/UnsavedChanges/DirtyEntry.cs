using Unity.Properties;
using UnityEngine;

namespace redwyre.DevTools.Editor.UnsavedChanges
{
    public class DirtyEntry
    {
        [CreateProperty]
        public string Guid { get; set; }

        [CreateProperty]
        public string Path { get; set; }

        [CreateProperty]
        public string Name { get; set; }

        [CreateProperty]
        public Texture2D Icon { get; set; }

        [CreateProperty]
        public AssetKind Kind { get; set; }

        [CreateProperty]
        public bool IsDirty { get; set; }

        [CreateProperty]
        public bool IsScene { get; set; }

        [CreateProperty]
        public string TooltipGuid { get; set; }

        [CreateProperty]
        public bool IsPrefab { get; set; }

        [CreateProperty]
        public bool IsSettings { get; set; }
    }
}