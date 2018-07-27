using UnityEngine;
using static CoreGUI;

public class CoreGUILabelExample : MonoBehaviour
{
    
    public string text;
    public float padding;

    private void OnGUI()
    {
        BeginGUI(this, Utility.screenRect.Shrink(padding).PixelPerfect());
        FlexibleSpace();
        Label(C(text));
        EndGUI();
    }
}

