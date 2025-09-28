// Assets/Scripts/Utils/LayersAndTags.cs
using UnityEngine;

public static class LayersAndTags
{
    public static int Layer(string name)
    {
        int l = LayerMask.NameToLayer(name);
        if (l < 0) Debug.LogWarning($"Layer '{name}' not found. Check Project Settings > Tags and Layers.");
        return l;
    }
}
