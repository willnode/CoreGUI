﻿using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class CoreGUI
{

    public static bool Button(GUIContent label)
    {
        return GUI.Button(Indent(Reserve(label, GUI.skin.button)), label);
    }

    public static bool Button(GUIContent label, GUIStyle style)
    {
        return GUI.Button(Indent(Reserve(label, style)), label, style);
    }

    public static bool Button(GUIContent label, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(label, GUI.skin.button), prefix);
        return GUI.Button(r, label);
    }

    public static bool RepeatButton(GUIContent label)
    {
        return GUI.RepeatButton(Indent(Reserve(label, GUI.skin.button)), label);
    }

    public static bool RepeatButton(GUIContent label, GUIStyle style)
    {
        return GUI.RepeatButton(Indent(Reserve(label, style)), label, style);
    }

    public static bool RepeatButton(GUIContent label, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(label, GUI.skin.button), prefix);
        return GUI.RepeatButton(r, label);
    }

    public static bool Toggle(GUIContent label, bool value)
    {
        return GUI.Toggle(Indent(Reserve(label, GUI.skin.toggle)), value, label);
    }

    public static bool Toggle(GUIContent label, bool value, GUIStyle style)
    {
        return GUI.Toggle(Indent(Reserve(label, style)), value, label, style);
    }

    public static bool Toggle(GUIContent label, bool value, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(label, GUI.skin.toggle), prefix);
        return GUI.Toggle(r, value, label);
    }

    public static bool Checkbox(GUIContent label, bool value)
    {
        return Toggle(GUIContent.none, value, label);
    }

    public static void Label(GUIContent label)
    {
        GUI.Label(Indent(Reserve(label), true), label);
    }

    public static void Label(GUIContent label, GUIStyle style)
    {
        GUI.Label(Indent(Reserve(label), true), label, style);
    }

    public static void Label(GUIContent label, GUIContent prefix)
    {
        var r = PrefixLabel(Reserve(), prefix);
        GUI.Label(r, label);
    }

}
