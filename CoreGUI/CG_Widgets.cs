using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class CoreGUI
{

    public static void Space(float pixels)
    {
        if (LayoutUtility.current.topLevel.isVertical)
            LayoutUtility.GetRect(0, pixels, LayoutUtility.spaceStyle, Layout.Height(pixels));
        else
            LayoutUtility.GetRect(pixels, 0, LayoutUtility.spaceStyle, Layout.Width(pixels));
    }

    public static void Space()
    {
        if (LayoutUtility.current.topLevel.isVertical)
            LayoutUtility.GetRect(0, 0, GUIStyle.none, Layout.ExpandHeight(true));
        else
            LayoutUtility.GetRect(0, 0, GUIStyle.none, Layout.ExpandWidth(true));
    }

    public static bool Button(GUIContent content)
    {
        return GUI.Button(Indent(Reserve(content, GUI.skin.button)), content);
    }

    public static bool Button(GUIContent content, GUIStyle style)
    {
        return GUI.Button(Indent(Reserve(content, style)), content, style);
    }

    public static bool Button(GUIContent content, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(content, GUI.skin.button), prefix);
        return GUI.Button(r, content);
    }
    
    public static bool Button(GUIContent content, GUIContent prefix, GUIStyle style)
    {
        var r = PrefixLabel(Reserve(content, style), prefix);
        return GUI.Button(r, content, style);
    }

    public static bool RepeatButton(GUIContent content)
    {
        return GUI.RepeatButton(Indent(Reserve(content, GUI.skin.button)), content);
    }

    public static bool RepeatButton(GUIContent content, GUIStyle style)
    {
        return GUI.RepeatButton(Indent(Reserve(content, style)), content, style);
    }

    public static bool RepeatButton(GUIContent content, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(content, GUI.skin.button), prefix);
        return GUI.RepeatButton(r, content);
    }

    public static bool Toggle(GUIContent content, bool value)
    {
        return GUI.Toggle(Indent(Reserve(content, GUI.skin.toggle)), value, content ?? GUIContent.none);
    }

    public static bool Toggle(GUIContent content, bool value, GUIStyle style)
    {
        return GUI.Toggle(Indent(Reserve(content, style)), value, content ?? GUIContent.none, style);
    }

    public static bool Toggle(GUIContent content, bool value, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(content, GUI.skin.toggle), prefix);
        return GUI.Toggle(r, value, content ?? GUIContent.none);
    }

    public static bool Checkbox(GUIContent label, bool value)
    {
        return Toggle(GUIContent.none, value, label);
    }

    public static void Label(GUIContent content)
    {
        GUI.Label(Indent(Reserve(content), true), content);
    }

    public static void Label(GUIContent content, GUIStyle style)
    {
        GUI.Label(Indent(Reserve(content), true), content, style);
    }

    public static void Label(GUIContent content, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(content), prefix);
        GUI.Label(r, content);
    }
    
    public static bool Foldout(GUIContent label, bool expanded)
    {
        PrefixFoldout(Reserve(label), label, ref expanded);
        return expanded;
    }

    public static bool Foldout(GUIContent label, ref bool expanded)
    {
        PrefixFoldout(Reserve(label), label, ref expanded);
        return expanded;
    }
    
    static Dictionary<int, bool> _propExpanded = new Dictionary<int, bool>();

    private static bool Foldout(GUIContent label)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive);
        bool expanded;
        _propExpanded.TryGetValue(id, out expanded);
        bool expanded2 = Foldout(label, expanded);
        if (expanded != expanded2)
            _propExpanded[id] = expanded2;
        return expanded2;
    }

}

