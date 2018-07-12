using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class CoreGUI {
    
    public static float prefixLabelWidth = 100f;

    public static int guiIndent = 0;

    public static GUILayoutOption[] layoutOptions = new GUILayoutOption[] {  };

    public static Rect PrefixLabel(Rect totalPosition, GUIContent label)
    {
        if (label != null && prefixLabelWidth > 0)
        {
            var r = totalPosition;
            r.width = prefixLabelWidth;
            r.xMin = guiIndent * 16;
            r.height = GUI.skin.label.CalcHeight(label, r.width);
            if (Event.current.type == EventType.Repaint)
                GUI.Label(r, label);

            totalPosition.xMin += prefixLabelWidth;
        }
        return totalPosition;
    }

    static Rect Indent(Rect r)
    {
        r.xMin += guiIndent * 16;
        return r;
    }

    public static Rect Reserve(GUIContent content = null, GUIStyle style = null)
    {
        return GUILayoutUtility.GetRect(content ?? GUIContent.none, style ?? GUI.skin.label, layoutOptions);
    }

    public static Rect Reserve(Vector2 size)
    {
        return GUILayoutUtility.GetRect(size.x, size.y, layoutOptions);
    }

    public static Rect Reserve(Rect minMax)
    {
        return GUILayoutUtility.GetRect(minMax.xMin, minMax.xMax, minMax.yMin, minMax.yMax, layoutOptions);
    }

    static GUIContent _cachedGUI = new GUIContent();
    public static GUIContent C(string s)
    {
        _cachedGUI.text = s;
        return _cachedGUI;
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
    
    public static Vector2 BeginScrollView(Vector2 scroll)
    {
        return GUILayout.BeginScrollView(scroll);
    }

    public static void EndScrollView()
    {
        GUILayout.EndScrollView();
    }

    public static void BeginArea()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
    }

    public static void BeginArea(Rect position)
    {
        GUILayout.BeginArea(position);
    }

    public static void EndArea()
    {
        GUILayout.EndArea();
    }

    static Stack<bool> changeChecks = new Stack<bool>();

    public static void BeginChangeCheck()
    {
        changeChecks.Push(GUI.changed);
        GUI.changed = false;
    }

    public static bool EndChangeCheck()
    {
        bool changed = GUI.changed;
        GUI.changed |= changeChecks.Pop();
        return changed;
    }

    static Stack<GUILayoutOption[]> layoutStacks = new Stack<GUILayoutOption[]>();

    public static void BeginLayoutOption(params GUILayoutOption[] options)
    {
        layoutStacks.Push(layoutOptions);
        layoutOptions = options;
    }

    public static void EndLayoutOption()
    {
        layoutOptions = layoutStacks.Pop();
    }

    public static bool Button(GUIContent label)
    {
        return GUI.Button(Reserve(label), label);
    }

    public static bool RepeatButton(GUIContent label)
    {
        return GUI.RepeatButton(Reserve(label), label);
    }

    public static bool Toggle(GUIContent label, bool value)
    {
        return GUI.Toggle(Indent(Reserve()), value, label, GUI.skin.button);
    }

    public static bool Checkbox(GUIContent label, bool value)
    {
        var r = PrefixLabel(Reserve(), label);
        return GUI.Toggle(r, value, GUIContent.none);
    }

    public static void Label(GUIContent label)
    {
        GUI.Label((Reserve(label)), label);
    }
    
    public enum ScaleUnit
    {
        Pixel = 0,
        Ratio = 1,
    }
}
