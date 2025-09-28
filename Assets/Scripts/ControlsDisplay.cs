using UnityEngine;

public class ControlsDisplay : MonoBehaviour
{
    [Header("UI Settings")]
    public bool showControls = true;
    public KeyCode toggleControlsKey = KeyCode.H;
    
    private string controlsText = 
        "WASD Movement Controls:\n\n" +
        "W - Move Forward\n" +
        "A - Move Left\n" +
        "S - Move Backward\n" +
        "D - Move Right\n" +
        "Space - Jump\n" +
        "Mouse - Look Around\n" +
        "Escape - Lock/Unlock Cursor\n" +
        "H - Toggle This Help";
    
    void Update()
    {
        if (Input.GetKeyDown(toggleControlsKey))
        {
            showControls = !showControls;
        }
    }
    
    void OnGUI()
    {
        if (!showControls) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
        
        GUI.Box(new Rect(10, 10, 250, 200), controlsText, style);
    }
    
    private Texture2D MakeTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}