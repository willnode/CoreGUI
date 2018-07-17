﻿using System;
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

    public static LayoutOption[] layoutOptions = new LayoutOption[] { };

    static Stack<GUIContent> prefixLabels = new Stack<GUIContent>();

    static void PrefixLabelStart(GUIContent label)
    {
        prefixLabels.Push(label);
        if (label != null)
        {
            LayoutGroup g = LayoutUtility.BeginLayoutGroup(GUIStyle.none, layoutOptions, typeof(LayoutGroup));
            g.isVertical = prefixLabelSide >= Side.Top;

            if (prefixLabelSide == Side.Left)
            {
                BeginLayoutOption(Layout.Width(prefixLabelWidth));
                PrefixLabel(Reserve(), label);
                EndLayoutOption();
            }
            else if (prefixLabelSide == Side.Top)
            {
                BeginLayoutOption(Layout.Height(0));
                PrefixLabel(Reserve(), label);
                EndLayoutOption();
            }           
        }
    }

    static void PrefixLabelEnd()
    {
        var label = prefixLabels.Pop();
        if (label != null)
        {
            if (prefixLabelSide == Side.Right)
            {
                BeginLayoutOption(Layout.Width(prefixLabelWidth));
                PrefixLabel(Reserve(), label);
                EndLayoutOption();
            }
            else if (prefixLabelSide == Side.Bottom)
            {
                BeginLayoutOption(Layout.Height(0));
                PrefixLabel(Reserve(), label);
                EndLayoutOption();
            }

            LayoutUtility.EndLayoutGroup();            
        }
    }

    public static Rect PrefixLabel(Rect totalPosition, GUIContent label)
    {
        if (label != null && prefixLabelWidth > 0)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Rect r = CalculatePrefixedRect(totalPosition, label);
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

    private static Rect CalculatePrefixedRect(Rect totalPosition, GUIContent label)
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
        return r;
    }

    public static Rect PrefixFoldout(Rect totalPosition, GUIContent label, ref bool expanded)
    {
        var r2 = CalculatePrefixedRect(totalPosition, label);
        var r = PrefixLabel(totalPosition, label);
        r2.x -= 16;
        r2.width = 16;
        expanded = GUI.Toggle(r2, expanded, GUIContent.none);

        return r;
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
        return LayoutUtility.GetRect(content ?? GUIContent.none, style ?? GUI.skin.label, layoutOptions);
    }

    public static Rect Reserve(Vector2 size)
    {
        return LayoutUtility.GetRect(size.x, size.y, layoutOptions);
    }

    public static Rect Reserve(Rect minMax)
    {
        return LayoutUtility.GetRect(minMax.xMin, minMax.xMax, minMax.yMin, minMax.yMax, layoutOptions);
    }

    static GUIContent _cachedTextGUI = new GUIContent();
    static GUIContent _cachedTextTooltipGUI = new GUIContent();
    static GUIContent _cachedTextureGUI = new GUIContent();
    static GUIContent _cachedTextTooltipTextureGUI = new GUIContent();

    public static GUIContent C(string text)
    {
        _cachedTextGUI.text = text;
        return _cachedTextGUI;
    }

    public static GUIContent C(string text, string tooltip)
    {
        _cachedTextTooltipGUI.text = text;
        _cachedTextTooltipGUI.tooltip = tooltip;
        return _cachedTextTooltipGUI;
    }

    public static GUIContent C(Texture image)
    {
        _cachedTextureGUI.image = image;
        return _cachedTextureGUI;
    }

    public static GUIContent C(Texture image, string text, string tooltip = "")
    {
        _cachedTextTooltipTextureGUI.image = image;
        _cachedTextTooltipTextureGUI.text = text;
        _cachedTextTooltipTextureGUI.tooltip = tooltip;
        return _cachedTextGUI;
    }

    public static Rect BeginHorizontal(GUIContent label = null)
    {
        PrefixLabelStart(label);
        if (label != null)
            BeginIndent(IndentPolicy.None);
        {
            LayoutGroup g = LayoutUtility.BeginLayoutGroup(GUIStyle.none, layoutOptions, typeof(LayoutGroup));
            g.isVertical = false;
            return g.rect;
        }
    }

    public static void EndHorizontal()
    {
        LayoutUtility.EndLayoutGroup();
        if (prefixLabels.Peek() != null)
            EndIndent();
        PrefixLabelEnd();
    }

    public static Rect BeginVertical(GUIContent label = null)
    {
        PrefixLabelStart(label);
        if (label != null)
            BeginIndent(IndentPolicy.None);
        {
            LayoutGroup g = LayoutUtility.BeginLayoutGroup(GUIStyle.none, layoutOptions, typeof(LayoutGroup));
            g.isVertical = true;
            return g.rect;
        }
    }

    public static void EndVertical()
    {
        LayoutUtility.EndLayoutGroup();
        if (prefixLabels.Peek() != null)
            EndIndent();
        PrefixLabelEnd();
    }

    public static void BeginScrollView(ref Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
    {
        scroll = BeginScrollView(scroll, alwaysShowHorizontal, alwaysShowVertical);
    }

    public static Vector2 BeginScrollView(Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
    {
        ScrollGroup g = (ScrollGroup)LayoutUtility.BeginLayoutGroup(GUIStyle.none, null, typeof(ScrollGroup));
        switch (Event.current.type)
        {
            case EventType.Layout:
                g.resetCoords = true;
                g.isVertical = true;
                g.stretchWidth = 1;
                g.stretchHeight = 1;
                g.verticalScrollbar = Styles.VerticalScrollbar;
                g.horizontalScrollbar = Styles.HorizontalScrollbar;
                g.needsVerticalScrollbar = alwaysShowVertical;
                g.needsHorizontalScrollbar = alwaysShowHorizontal;
                g.ApplyOptions(layoutOptions);
                break;
            default:
                break;
        }
        return GUI.BeginScrollView(g.rect, scroll, new Rect(0, 0, g.clientWidth, g.clientHeight), alwaysShowHorizontal, alwaysShowVertical);
    }
    
    public static void EndScrollView()
    {
        LayoutUtility.EndLayoutGroup();
        GUI.EndScrollView();
    }

    public static void BeginGUI(UnityEngine.Object obj)
    {
        BeginGUI(obj.GetInstanceID(), new Rect(0, 0, Screen.width, Screen.height));
    }

    public static void BeginGUI(int id, Rect position)
    {
        LayoutUtility.Begin(id);
        BeginArea(position);
    }

    public static void EndGUI()
    {
        EndArea();
        if (guiIndentPolicies.Count > 0)
        {
            Debug.LogError("WARNING: You're pushing more indent stacks than popping it");
            while (guiIndentPolicies.Count > 0)
            {
                EndIndent();
            }
        }
        if (LayoutUtility.current.layoutGroups.Count > 1)
        {
            if (ev.type != EventType.Used)
                Debug.LogError("WARNING: You're pushing more layout stacks than popping it");

            while (LayoutUtility.current.layoutGroups.Count > 1)
            {
                LayoutUtility.current.layoutGroups.Pop();
            }
        }
        if (ev.type == EventType.Layout)
            LayoutUtility.Layout();

    }


    public static void BeginArea(Rect screenRect)
    {
        LayoutGroup g = LayoutUtility.BeginLayoutArea(GUIStyle.none, typeof(LayoutGroup));
        if (Event.current.type == EventType.Layout)
        {
            g.resetCoords = true;
            g.minWidth = g.maxWidth = screenRect.width;
            g.minHeight = g.maxHeight = screenRect.height;
            g.rect = Rect.MinMaxRect(screenRect.xMin, screenRect.yMin, g.rect.xMax, g.rect.yMax);
        }

        GUI.BeginGroup(g.rect, GUIContent.none, GUIStyle.none);
    }

    public static void EndArea()
    {
        if (Event.current.type == EventType.Used)
            return;
        LayoutUtility.current.layoutGroups.Pop();
        LayoutUtility.current.topLevel = (LayoutGroup)LayoutUtility.current.layoutGroups.Peek();
        GUI.EndGroup();
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

    static Stack<LayoutOption[]> layoutStacks = new Stack<LayoutOption[]>();

    public static void BeginLayoutOption(params LayoutOption[] options)
    {
        layoutStacks.Push(layoutOptions);
        layoutOptions = options;
    }

    public static void EndLayoutOption()
    {
        layoutOptions = layoutStacks.Pop();
    }

    static Dictionary<int, float> _fadeTimes = new Dictionary<int, float>();

    public static bool BeginFadeGroup(bool val)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive);
        var fade = _fadeTimes.ContainsKey(id) ? _fadeTimes[id] : _fadeTimes[id] = (val ? 1 : 0);

        if (!val && fade <= 0)
        {
            BeginFadeGroup(0);
            return false;
        }
        else if (val && fade >= 1)
        {
            BeginFadeGroup(1);
            return true;
        }
        else
        {
            fade += (val ? 1 : -1) * Time.unscaledDeltaTime;
            fade = Mathf.Clamp(fade, 0, 1);
            _fadeTimes[id] = fade;
            BeginFadeGroup(fade);
            return fade > 0;
        }
    }

    public static bool BeginFadeGroup(float fade)
    {
        return fade > 0;
    }

    public static void EndFadeGroup()
    {

    }

    static Stack<ValueTuple<int, IndentPolicy>> guiIndentPolicies = new Stack<ValueTuple<int, IndentPolicy>>();

    public static void BeginIndent(IndentPolicy policy)
    {
        BeginIndent(-1, policy);
    }

    public static void BeginIndent(int indent = -1, IndentPolicy policy = IndentPolicy.Inherit)
    {
        guiIndentPolicies.Push(new ValueTuple<int, IndentPolicy>(guiIndent, guiIndentPolicy));

        if (indent >= 0)
            guiIndent = indent;
        else
            guiIndent++;

        if (policy != IndentPolicy.Inherit)
            guiIndentPolicy = policy;
    }

    public static void EndIndent()
    {
        var val = guiIndentPolicies.Pop();
        guiIndent = val.item1;
        guiIndentPolicy = val.item2;
    }

    static Stack<ValueTuple<float, Side>> labelStacks = new Stack<ValueTuple<float, Side>>();

    public static void BeginLabelOption(Side side)
    {
        BeginLabelOption(-1, side);
    }

    public static void BeginLabelOption(float width = -1, Side side = Side.Inherit)
    {
        labelStacks.Push(new ValueTuple<float, Side>(prefixLabelWidth, prefixLabelSide));
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
        prefixLabelWidth = o.item1;
        prefixLabelSide = o.item2;
    }

    public static void DrawTooltips(float maxWidth = 300f)
    {
        if (ev.type != EventType.Repaint) return;

        var text = GUI.tooltip;

        if (!string.IsNullOrEmpty(text))
        {
            Vector2 offset = new Vector2(16, 0);
            var style = GUI.skin.label;
            var gui = C(text);
            var size = style.CalcSize(gui);
            if (size.x > maxWidth)
                size = new Vector2(maxWidth, style.CalcHeight(gui, maxWidth));
            var pos = ev.mousePosition + offset;
            var limit = new Vector2(Screen.width, Screen.height) - size;

            if (pos.x > limit.x)
                pos.x -= size.x + offset.x * 2;
            if (pos.y > limit.y)
                pos.y -= size.y + offset.y * 2;

            GUI.Label(new Rect(pos, size), gui);
        }
    }
}
