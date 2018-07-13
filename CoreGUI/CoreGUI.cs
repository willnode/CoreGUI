using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class CoreGUI {
    
    public static float prefixLabelWidth = 100f;

    public static int guiIndent = 0;

    public static GUIIndentPolicy guiIndentPolicy = GUIIndentPolicy.Widgets;

    public enum GUIIndentPolicy
    {
        /// <summary>
        /// Apply indentation only to labelled widgets
        /// </summary>
        Label = 0,
        /// <summary>
        /// Apply indentation to widgets both labelled and unlabelled widgets
        /// </summary>
        Widgets = 1,
        /// <summary>
        /// Always prefix widgets even if there's no label before it
        /// </summary>
        Full = 2,
        /// <summary>
        /// Disable indenting
        /// </summary>
        None = 3,
        /// <summary>
        /// Used only for BeginIndent. Use previous setting.
        /// </summary>
        Inherit = -1,
    }

    public static GUILayoutOption[] layoutOptions = new GUILayoutOption[] {  };

    public static Rect PrefixLabel(Rect totalPosition, GUIContent label)
    {
        if (label != null && prefixLabelWidth > 0)
        {
            var r = totalPosition;
            r.width = prefixLabelWidth;
            r.xMin = guiIndentPolicy == GUIIndentPolicy.None ? 0 : guiIndent * 16;
            r.height = GUI.skin.label.CalcHeight(label, r.width);
            if (Event.current.type == EventType.Repaint)
                GUI.Label(r, label);

            totalPosition.xMin += prefixLabelWidth;
        }
        else if (guiIndentPolicy == GUIIndentPolicy.Full)
            totalPosition.xMin += prefixLabelWidth;
        else if (guiIndentPolicy == GUIIndentPolicy.Widgets)
            totalPosition.xMin += guiIndent * 16;

        return totalPosition;
    }

    static Rect Indent(Rect r, bool ignorePolicy = false)
    {
        if (guiIndentPolicy == GUIIndentPolicy.Widgets || ignorePolicy)
            r.xMin += guiIndent * 16;
        else if (guiIndentPolicy == GUIIndentPolicy.Full)
            r.xMin += prefixLabelWidth;
         
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

    public static void BeginHorizontal(GUIContent label = null)
    {
        BeginIndent(label == null ? GUIIndentPolicy.Inherit : GUIIndentPolicy.None);
        if (label != null)
        {
            var r = PrefixLabel(Reserve(Vector2.zero), label);
            GUILayout.BeginHorizontal();
            GUILayoutUtility.GetRect(0, 0, GUILayout.Width(r.xMin));
        } else
            GUILayout.BeginHorizontal();
    }

    public static void EndHorizontal()
    {
        GUILayout.EndHorizontal();
        EndIndent();
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

    static Stack<GUIIndentPolicy> guiIndentPolicies = new Stack<GUIIndentPolicy>();

    public static void BeginIndent(GUIIndentPolicy policy = GUIIndentPolicy.Inherit)
    {
        guiIndent++;
        guiIndentPolicies.Push(guiIndentPolicy);
        if (policy != GUIIndentPolicy.Inherit)
            guiIndentPolicy = policy;
    }
    
    public static void EndIndent()
    {
        guiIndent--;
        guiIndentPolicy = guiIndentPolicies.Pop();
    }

    public static bool Button(GUIContent label)
    {
        return GUI.Button(Indent(Reserve(label)), label);
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
