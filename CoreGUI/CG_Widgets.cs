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
        return ButtonInternal(Indent(Reserve(content, GUI.skin.button)), content, Styles.Button);
    }

    public static bool Button(GUIContent content, GUIStyle style)
    {
        return ButtonInternal(Indent(Reserve(content, style)), content, style);
    }

    public static bool Button(GUIContent content, GUIContent prefix)
    {
        var id = GUIUtility.GetControlID(FocusType.Keyboard);
        var r = PrefixLabel(Reserve(content, GUI.skin.button), prefix, id);
        return ButtonInternal(r, content, Styles.Button, id);
    }
    
    public static bool Button(GUIContent content, GUIContent prefix, GUIStyle style)
    {
        var id = GUIUtility.GetControlID(FocusType.Keyboard);
        var r = PrefixLabel(Reserve(content, style), prefix, id);
        return ButtonInternal(r, content, style, id);
    }

    static bool ButtonInternal(Rect r, GUIContent content, GUIStyle style, int id = 0)
    {
        if (id == 0)
        {
            id = GUIUtility.GetControlID(FocusType.Keyboard, r);
        }
        switch (ev.GetTypeForControl(id))
        {
            case EventType.MouseDown:
               
                if (r.Contains(ev.mousePosition) && ev.button == 0)
                {
                    GUIUtility.hotControl = id;
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = id;

                    if (r.Contains(ev.mousePosition))
                        return true;
                }
                break;
            case EventType.KeyDown:
                if (GUIUtility.keyboardControl == id && (ev.keyCode == KeyCode.Space))
                {
                    GUIUtility.hotControl = id;
                }
                break;
            case EventType.KeyUp:
                if (GUIUtility.hotControl == id && GUIUtility.keyboardControl == id)
                {
                    GUIUtility.hotControl = 0;
                    return true;
                }
                break;
            case EventType.Repaint:
                DrawStyle(r, content, style, id);
                break;
        }
        return false;
    }

    private static void DrawStyle(Rect r, GUIContent content, GUIStyle style, int id, bool on = false)
    {
        // Primary reason why not simply giveaway ControlID:
        // The button is not appear to be clicked (active) when clicked via keyboard and mouse is not hovering them.
        style.Draw(r, content ?? GUIContent.none, r.Contains(ev.mousePosition) || GUIUtility.hotControl == id,
            GUIUtility.hotControl == id, on, GUIUtility.keyboardControl == id);
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
        return ToggleInternal(Indent(Reserve(content, GUI.skin.toggle)), value, content, Styles.Toggle);
    }

    public static bool Toggle(GUIContent content, bool value, GUIStyle style)
    {
        return ToggleInternal(Indent(Reserve(content, style)), value, content, style);
    }

    public static bool Toggle(GUIContent content, bool value, GUIContent prefix)
    {
        var id = GUIUtility.GetControlID(FocusType.Keyboard);
        var r = PrefixLabel(Reserve(content, GUI.skin.toggle), prefix, id);
        return ToggleInternal(r, value, content, Styles.Toggle);
    }
    
    static bool ToggleInternal(Rect r, bool value, GUIContent content, GUIStyle style, int id = 0)
    {
        if (id == 0)
        {
            id = GUIUtility.GetControlID(FocusType.Keyboard, r);
        }
        switch (ev.GetTypeForControl(id))
        {
            case EventType.MouseDown:

                if (r.Contains(ev.mousePosition) && ev.button == 0)
                {
                    GUIUtility.hotControl = id;
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    
                    if (r.Contains(ev.mousePosition))
                    {
                        ev.Use();
                        GUI.changed = true;
                        GUIUtility.keyboardControl = id;
                        return !value;
                    }
                }
                break;
            case EventType.KeyDown:
                if (GUIUtility.keyboardControl == id && (ev.keyCode == KeyCode.Space))
                {
                    GUIUtility.hotControl = id;
                }
                break;
            case EventType.KeyUp:
                if (GUIUtility.hotControl == id && GUIUtility.keyboardControl == id)
                {
                    GUIUtility.hotControl = 0;
                    GUI.changed = true;
                    ev.Use();
                    return !value;
                }
                break;
            case EventType.Repaint:
                DrawStyle(r, content, style, id, value);
                break;
        }
        return value;
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
        return ToggleInternal(Indent(Reserve(label, Styles.Foldout), true), expanded, label, Styles.Foldout);
    }

    public static bool Foldout(GUIContent label, ref bool expanded)
    {
        expanded = Foldout(label, expanded);
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

