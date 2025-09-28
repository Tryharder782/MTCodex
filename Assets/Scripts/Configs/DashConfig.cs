// Assets/Scripts/Configs/DashConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/DashConfig")]
public class DashConfig : ScriptableObject
{
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    [Tooltip("I-frames окно в секундах от старта даша")]
    public Vector2 iFramesWindow = new Vector2(0f, 0.18f);

    [Header("I-frames реализация")]
    public bool useLayerSwitch = false;
    public string normalLayer = "Player";
    public string invulnLayer = "PlayerInvulnerable";
    [Tooltip("Если задано, будет отключаться на время i-frames")]
    public Collider[] hurtboxColliders;
}
