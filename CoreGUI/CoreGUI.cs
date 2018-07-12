using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class CoreGUI {
    
    public static float prefixLabelWidth = 80f;

    public static int guiIndent = 0;

    public static GUILayoutOption[] layoutOptions = new GUILayoutOption[] { GUILayout.Height(16), GUILayout.ExpandWidth(true) };

    public static Rect PrefixLabel(Rect totalPosition, GUIContent label)
    {
        if (label != null && prefixLabelWidth > 0)
        {
            var w = totalPosition.width;
            totalPosition.width = prefixLabelWidth - guiIndent * 16;
            if (Event.current.type == EventType.Repaint)
                GUI.Label(totalPosition, label);
            totalPosition.x += totalPosition.width;
            totalPosition.width = w - totalPosition.width;
        }
        return totalPosition;
    }

    static Rect Indent(Rect r)
    {
        r.xMin += guiIndent * 16;
        return r;
        
    }

    public static Rect Reserve()
    {
        return GUILayoutUtility.GetRect(1, 1, layoutOptions);
    }
    
    public static GUIContent C(string s)
    {
        return new GUIContent(s);
    }

    public static void BeginHorizontal()
    {
        GUILayout.BeginHorizontal();
    }

    public static void EndHorizontal()
    {
        GUILayout.EndHorizontal();
    }
    
    public static void BeginVertical()
    {
        GUILayout.BeginVertical();
    }

    public static void EndVertical()
    {
        GUILayout.EndVertical();
    }

    public static void BeginChangeCheck()
    {
        
    }

    public static bool EndChangeCheck()
    {
        return false;
    }

    public static bool Button(GUIContent label)
    {
        return GUI.Button(Reserve(), label);
    }

    public static bool RepeatButton(GUIContent label)
    {
        return GUI.RepeatButton(Reserve(), label);
    }

    public static bool Toggle(GUIContent label, bool value)
    {
        return GUI.Toggle(Indent(Reserve()), value, label, GUI.skin.button);
    }

    public static bool Checkbox(GUIContent label, bool value)
    {
        var r = PrefixLabel(Indent(Reserve()), label);
        return GUI.Toggle(r, value, GUIContent.none);
    }

}
