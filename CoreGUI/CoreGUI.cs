using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class CoreGUI
{

    public static float prefixLabelWidth = 100f;

    public static Side prefixLabelSide = Side.Left;

    public static int guiIndent = 0;

    public static IndentPolicy guiIndentPolicy = IndentPolicy.Widgets;

    public enum IndentPolicy
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

    public enum Side
    {
        Left, Right, Top, Bottom, Inherit = -1
    }

    public static GUILayoutOption[] layoutOptions = new GUILayoutOption[] { };

    static Stack<GUIContent> prefixLabels = new Stack<GUIContent>();

    static void PrefixLabelStart(GUIContent label)
    {
        prefixLabels.Push(label);
        if (label != null)
        {
            switch (prefixLabelSide)
            {
                case Side.Left:
                    GUILayout.BeginHorizontal();
                    BeginLayoutOption(GUILayout.Width(prefixLabelWidth));
                    PrefixLabel(Reserve(), label);
                    EndLayoutOption();
                    break;
                case Side.Top:
                    GUILayout.BeginVertical();
                    BeginLayoutOption(GUILayout.Height(0));
                    PrefixLabel(Reserve(), label);
                    EndLayoutOption();
                    break;
                case Side.Right:
                    GUILayout.BeginHorizontal();
                    break;
                case Side.Bottom:
                    GUILayout.BeginVertical();
                    break;
            }
        }
    }

    static void PrefixLabelEnd()
    {
        var label = prefixLabels.Pop();
        if (label != null)
        {
            switch (prefixLabelSide)
            {
                case Side.Left:
                    GUILayout.EndHorizontal();
                    break;
                case Side.Top:
                    GUILayout.EndVertical();
                    break;
                case Side.Right:
                    BeginLayoutOption(GUILayout.Width(prefixLabelWidth));
                    PrefixLabel(Reserve(), label);
                    EndLayoutOption();
                    GUILayout.EndHorizontal();
                    break;
                case Side.Bottom:
                    BeginLayoutOption(GUILayout.Height(0));
                    PrefixLabel(Reserve(), label);
                    EndLayoutOption();
                    GUILayout.EndVertical();
                    break;
            }
        }
    }

    public static Rect PrefixLabel(Rect totalPosition, GUIContent label)
    {
        if (label != null && prefixLabelWidth > 0)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var r = totalPosition;
                switch (prefixLabelSide)
                {
                    case Side.Left:
                        r.width = prefixLabelWidth;
                        r.xMin += guiIndentPolicy == IndentPolicy.None ? 0 : guiIndent * 16;
                        break;
                    case Side.Right:
                        r.xMin += r.width - prefixLabelWidth - (guiIndentPolicy == IndentPolicy.None ? 0 : guiIndent * 16);
                        break;
                    case Side.Top:
                        break;
                    case Side.Bottom:
                        r.y += r.height;
                        break;
                }
                r.height = GUI.skin.label.CalcHeight(label, r.width);
                GUI.Label(r, label);
            }
        }

        float width = label != null || guiIndentPolicy == IndentPolicy.Full ? prefixLabelWidth :
            (guiIndentPolicy == IndentPolicy.Widgets ? guiIndent * 16 : 0);

        switch (prefixLabelSide)
        {
            case Side.Left:
                totalPosition.xMin += width;
                break;
            case Side.Right:
                totalPosition.xMax -= width;
                break;
            case Side.Top:
                Reserve(new Vector2(0, width));
                totalPosition.y += width;
                break;
            case Side.Bottom:
                Reserve(new Vector2(0, width));
                break;
        }

        return totalPosition;
    }

    static Rect Indent(Rect r, bool ignorePolicy = false)
    {
        if (guiIndentPolicy == IndentPolicy.Widgets || ignorePolicy)
            r.xMin += guiIndent * 16;
        else if (guiIndentPolicy == IndentPolicy.Full)
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
        PrefixLabelStart(label);
        GUILayout.BeginHorizontal();
        BeginIndent(IndentPolicy.None);
    }

    public static void EndHorizontal()
    {
        EndIndent();
        GUILayout.EndHorizontal();
        PrefixLabelEnd();
    }

    public static void BeginVertical(GUIContent label = null)
    {
        PrefixLabelStart(label);
        GUILayout.BeginVertical();
        BeginIndent(IndentPolicy.None);
    }

    public static void EndVertical()
    {
        EndIndent();
        GUILayout.EndVertical();
        PrefixLabelEnd();
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

    static Stack<IndentPolicy> guiIndentPolicies = new Stack<IndentPolicy>();

    public static void BeginIndent(IndentPolicy policy = IndentPolicy.Inherit)
    {
        guiIndent++;
        guiIndentPolicies.Push(guiIndentPolicy);
        if (policy != IndentPolicy.Inherit)
            guiIndentPolicy = policy;
    }

    public static void EndIndent()
    {
        guiIndent--;
        guiIndentPolicy = guiIndentPolicies.Pop();
    }

    struct LabelOption : IEquatable<LabelOption>
    {

        public Side side;
        public float width;

        public LabelOption(float width, Side edge)
        {
            this.side = edge;
            this.width = width;
        }

        public bool Equals(LabelOption other)
        {
            return side == other.side && width == other.width;
        }
    }

    static Stack<LabelOption> labelStacks = new Stack<LabelOption>();

    public static void BeginLabelOption(float width = -1, Side side = Side.Inherit)
    {
        labelStacks.Push(new LabelOption(prefixLabelWidth, prefixLabelSide));
        if (side != Side.Inherit)
            prefixLabelSide = side;
        if (width >= 0)
            prefixLabelWidth = width;
        else if (side == Side.Bottom || side == Side.Top)
            prefixLabelWidth = GUI.skin.label.CalcHeight(C(" "), 60);
    }

    public static void EndLabelOption()
    {
        var o = labelStacks.Pop();
        prefixLabelSide = o.side;
        prefixLabelWidth = o.width;
    }

}
