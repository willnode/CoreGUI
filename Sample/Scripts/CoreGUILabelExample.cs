
using UnityEngine;
// Use new C# feature to shorten calls to CoreGUI
using static CoreGUI;

public class CoreGUILabelExample : MonoBehaviour
{
    // CoreGUI Logo
    public Texture logo;
    // "Hello, world!"
    public string text;
    // Padding around the screen (20)
    public int padding = 20;
    
    private void OnGUI()
    {
        // Tell CoreGUI to begin GUI calculation
        BeginGUI(this, Utility.screenRect.Shrink(padding));
        // Stretch to the end, fill it with the logo
        GUI.Label(FlexibleSpace(), logo);
        // Draw the text
        Label(C(text));
        // Done!
        EndGUI();
    }
}

