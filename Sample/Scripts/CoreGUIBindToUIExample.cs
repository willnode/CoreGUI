using UnityEngine;
// Use new C# feature to shorten calls to CoreGUI
using static CoreGUI;

public class CoreGUIBindToUIExample : MonoBehaviour
{
    // Padding around RectTransform
    public float padding = 4;

    // Shortcut to rect transform
    RectTransform rectTransform => GetComponent<RectTransform>();

    private void OnGUI()
    {
        // Convert this RectTransform to Screen coordinate
        // (CoreGUI built-in utility)
        var rect = Utility.BindFromUIOverlay(rectTransform, padding);
        // Tell CoreGUI to begin GUI calculation
        BeginGUI(this, rect.PixelPerfect());
        // Tell to draw the label on top (instead of left, which is the default)
        using (Scoped.LabelOption(Side.Top))
        {
            // Draw a message
            Label(C("This GUI is Binded from Rect Transform!"));
            // Stretch to the end
            FlexibleSpace();
            // Draw read-only rectangle field
            RectField(C("Screen position:"), rect);           
        }
        // Done!
        EndGUI();
    }
}

