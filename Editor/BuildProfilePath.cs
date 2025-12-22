using System;
using UnityEngine;

public class BuildProfilePath : ScriptableObject
{
    public string path;

#if UNITY_EDITOR
    // Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
    private void OnValidate()
    {
    }

    // Reset to default values.
    private void Reset()
    {
    }
#endif
}
