using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CoreGUI;

public class CoreGUIBindToUIExample : MonoBehaviour
{
    public float padding = 4;

    RectTransform rectTransform => (RectTransform)transform;

    private void OnGUI()
    {
        var rect = Utility.BindFromUIOverlay(rectTransform, padding);
        BeginGUI(this, rect);
        using (Scoped.LabelOption(Side.Top))
        {
            Label(C("This GUI is Binded from Rect Transform!"));
            FlexibleSpace();
            RectField(C("Screen position:"), rect);           
        }
        EndGUI();
    }
}

