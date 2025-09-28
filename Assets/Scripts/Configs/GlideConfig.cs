// Assets/Scripts/Configs/GlideConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/GlideConfig")]
public class GlideConfig : ScriptableObject
{
    public float gravityScaleWhileGlide = 0.35f;
    public float horizontalBoost = 0.5f;
}
