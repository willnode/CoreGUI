using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class CoreGUI
{

    #region Prefix and Indentation

    public static float prefixLabelWidth = 100f;

    public static Side prefixLabelSide = Side.Left;

    static Stack<GUIContent> prefixLabels = new Stack<GUIContent>();

    static void PrefixLabelStart(GUIContent label)
    {
        if (label != null)
        {
            bool wasVertical = LayoutUtility.isVertical;
            LayoutGroup g = LayoutUtility.BeginLayoutGroup(GUIStyle.none, layoutOptions, typeof(LayoutGroup));
            g.isVertical = prefixLabelSide >= Side.Top;

            if (prefixLabelSide == Side.Left)
            {
                PrefixLabel(Space(wasVertical ? 0 : prefixLabelWidth), label);
            }
            else if (prefixLabelSide == Side.Top)
            {
                PrefixLabel(Space(wasVertical ? 0 : prefixLabelWidth), label);
            }
            else if (ev.type == EventType.Repaint)
            {
                // in case it's copied from C, we need to copy it out from cache.
                label = new GUIContent(label);
            }

            if ((indentPolicy != IndentPolicy.None && indentPolicy != IndentPolicy.Label && indentLevel > 0) &&
                (prefixLabelSide == Side.Bottom || prefixLabelSide == Side.Top))
            {
                LayoutUtility.BeginLayoutGroup(GUIStyle.none, layoutOptions, typeof(LayoutGroup)).isVertical = false;
                Space(indentLevel * 16);
            }

        }
        prefixLabels.Push(label);
    }

    static void PrefixLabelEnd()
    {
        var label = prefixLabels.Pop();
        if (label != null)
        {
            if ((indentPolicy != IndentPolicy.None && indentPolicy != IndentPolicy.Label && indentLevel > 0) &&
               (prefixLabelSide == Side.Bottom || prefixLabelSide == Side.Top))
            {
                LayoutUtility.EndLayoutGroup();
            }
            if (prefixLabelSide == Side.Right)
            {
                PrefixLabel(Space(0), label);
            }
            else if (prefixLabelSide == Side.Bottom)
            {
                PrefixLabel(Space(0), label);
            }

            LayoutUtility.EndLayoutGroup();
        }
    }

    /// <summary>
    /// Reserve position and do PrefixLabel from there
    /// </summary>
    public static Rect PrefixLabel(GUIContent content, GUIStyle style, GUIContent label, int id = 0)
    {
        if (id == 0)
            id = GUIUtility.GetControlID(FocusType.Passive) + 1; // Educated guess

        var r = Reserve(content ?? GUIContent.none, style = style ?? GUIStyle.none);

        // Layout.GetRect accounts style margin, so we need to take it out.
        r = style.margin.Add(r);
        r = PrefixLabel(r, label, id);
        r = style.margin.Remove(r);
        return r;
    }

    public static Rect PrefixLabel(Rect totalPosition, GUIContent label, int id = 0)
    {
        if (label != null && prefixLabelWidth > 0)
        {
            if (ev.type == EventType.Repaint || ev.type == EventType.MouseUp)
            {
                Rect r = CalculatePrefixedRect(totalPosition, label);

                if (ev.type == EventType.Repaint)
                {
                    Utility.DrawStyle(r, label, Styles.Prefix, id, false);
                }
                else if (r.Contains(ev.mousePosition) && id != 0)
                {
                    GUIUtility.keyboardControl = id;
                }
            }
        }

        if (indentPolicy == IndentPolicy.Widgets || indentPolicy == IndentPolicy.Full)
        {
            bool shouldEat = LayoutUtility.isVertical != prefixLabelSide >= Side.Top;

            float offset = indentLevel * 16;
            float width = label != null || indentPolicy == IndentPolicy.Full ? prefixLabelWidth : offset;

            if (!shouldEat && width > 0)
                Space(width);

            switch (prefixLabelSide)
            {
                case Side.Left:
                    if (shouldEat) totalPosition.xMin += width;
                    else totalPosition.x += width;
                    break;
                case Side.Right:
                    if (shouldEat) totalPosition.xMax -= width;
                    break;
                case Side.Top:
                    if (shouldEat) totalPosition.yMin += width;
                    else totalPosition.y += width;
                    totalPosition.xMin += offset;
                    break;
                case Side.Bottom:
                    if (shouldEat) totalPosition.yMax -= width;
                    totalPosition.xMin += offset;
                    break;
            }
        }

        return totalPosition;
    }

    private static Rect CalculatePrefixedRect(Rect totalPosition, GUIContent label)
    {
        bool shouldEat = LayoutUtility.isVertical != prefixLabelSide >= Side.Top;
        var r = totalPosition;
        var indent = indentPolicy == IndentPolicy.None ? 0 : indentLevel * 16;
        switch (prefixLabelSide)
        {
            case Side.Left:
                r.x += indent;
                if (!shouldEat)
                {
                    r.height = LayoutUtility.GetContainerRect(true).height;
                }
                r.width = prefixLabelWidth - indent;
                break;
            case Side.Right:
                if (!shouldEat)
                {
                    r.height = LayoutUtility.GetContainerRect(true).height;
                    r.width = prefixLabelWidth;
                }
                r.x += r.width - prefixLabelWidth;
                r.width = prefixLabelWidth - indent;
                break;
            case Side.Top:
                if (!shouldEat)
                {
                    r.width = LayoutUtility.GetContainerRect().width;
                    r.height = prefixLabelWidth;
                }
                r.xMin += indent;
                break;
            case Side.Bottom:
                r.y += r.height;
                if (!shouldEat)
                {
                    r.width = LayoutUtility.GetContainerRect().width;
                    r.height = prefixLabelWidth;
                }
                r.xMin += indent;
                break;
        }
        return r;
    }

    public static Rect PrefixFoldout(Rect totalPosition, GUIContent label, ref bool expanded, int id = -1)
    {
        var r2 = CalculatePrefixedRect(totalPosition, label);
        var r = PrefixLabel(totalPosition, GUIContent.none, id);
        expanded = GUI.Toggle(r2, expanded, label, Styles.Foldout);

        return r;
    }

    public static Rect PrefixSlider(Rect totalPosition, GUIContent label, int id, out float delta)
    {
        if (id == 0) id = GUIUtility.GetControlID(FocusType.Passive);

        delta = 0;

        var r2 = CalculatePrefixedRect(totalPosition, label);
        var r = PrefixLabel(totalPosition, label, id + 1);
        if (ev.type == EventType.MouseDown && r2.Contains(ev.mousePosition))
            GUIUtility.hotControl = id;
        else if (ev.type == EventType.MouseDrag && GUIUtility.hotControl == id)
            delta = ev.delta.x;
        else if (ev.type == EventType.MouseUp && GUIUtility.hotControl == id)
            GUIUtility.hotControl = 0;

        return r;
    }

    public static int indentLevel = 0;

    public static IndentPolicy indentPolicy = IndentPolicy.Widgets;

    public static LayoutOption[] layoutOptions = new LayoutOption[] { };

    public static Rect Indent(Rect r, bool ignorePolicy = false)
    {
        if (indentPolicy == IndentPolicy.Widgets || ignorePolicy)
            r.xMin += indentLevel * 16;
        else if (indentPolicy == IndentPolicy.Full)
            r.xMin += prefixLabelWidth;

        return r;
    }

    public static Rect Reserve(GUIContent content = null, GUIStyle style = null)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        return LayoutUtility.GetRect(content ?? GUIContent.none, style ?? GUI.skin.label, layoutOptions);
    }

    public static Rect Reserve(float aspect)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        return LayoutUtility.GetAspectRect(aspect);
    }

    public static Rect Reserve(Vector2 size)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        return LayoutUtility.GetRect(size.x, size.y, layoutOptions);
    }

    public static Rect Reserve(Rect minMax)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        return LayoutUtility.GetRect(minMax.xMin, minMax.xMax, minMax.yMin, minMax.yMax, layoutOptions);
    }

    /// <summary>
    /// Reserve fixed space
    /// </summary>
    public static Rect Space(float pixels)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        if (LayoutUtility.isVertical)
            return LayoutUtility.GetRect(0, pixels, LayoutUtility.spaceStyle, Layout.Height(pixels));
        else
            return LayoutUtility.GetRect(pixels, 0, LayoutUtility.spaceStyle, Layout.Width(pixels));
    }

    /// <summary>
    /// Reserve flexible/rest of empty space
    /// </summary>
    public static Rect FlexibleSpace()
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        if (LayoutUtility.isVertical)
            return LayoutUtility.GetRect(0, 0, GUIStyle.none, Layout.ExpandHeight(true));
        else
            return LayoutUtility.GetRect(0, 0, GUIStyle.none, Layout.ExpandWidth(true));
    }

    #endregion

    #region C

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

    #endregion

    #region Layouts

    public static Rect BeginHorizontal(GUIContent label = null)
    {
        return BeginHorizontal(label, Utility.emptyLayoutOption);
    }

    public static Rect BeginHorizontal(GUIContent label, params LayoutOption[] options)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        PrefixLabelStart(label);
        if (label != null)
            BeginIndent(0, IndentPolicy.Widgets);
        {
            LayoutGroup g = LayoutUtility.BeginLayoutGroup(GUIStyle.none, options, typeof(LayoutGroup));
            g.isVertical = false;
            return g.rect;
        }
    }

    public static void EndHorizontal()
    {
        if (Utility.currentCustomEvent != null) return;

        LayoutUtility.EndLayoutGroup();
        if (prefixLabels.Peek() != null)
            EndIndent();
        PrefixLabelEnd();
    }

    public static Rect BeginVertical(GUIContent label = null)
    {
        return BeginVertical(label, Utility.emptyLayoutOption);
    }

    public static Rect BeginVertical(GUIContent label = null, params LayoutOption[] options)
    {
        if (Utility.currentCustomEvent != null) return LayoutUtility.kDummyRect;

        PrefixLabelStart(label);
        if (label != null)
            BeginIndent(0, IndentPolicy.Widgets);
        {
            LayoutGroup g = LayoutUtility.BeginLayoutGroup(GUIStyle.none, options, typeof(LayoutGroup));
            g.isVertical = true;
            return g.rect;
        }
    }

    public static void EndVertical()
    {
        if (Utility.currentCustomEvent != null) return;

        LayoutUtility.EndLayoutGroup();
        if (prefixLabels.Peek() != null)
            EndIndent();
        PrefixLabelEnd();
    }

    public static void BeginScrollView(ref Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
    {
        scroll = BeginScrollView(scroll, alwaysShowHorizontal, alwaysShowVertical, Utility.emptyLayoutOption);
    }

    public static void BeginScrollView(ref Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false, params LayoutOption[] options)
    {
        scroll = BeginScrollView(scroll, alwaysShowHorizontal, alwaysShowVertical, options);
    }

    public static Vector2 BeginScrollView(Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
    {
        return BeginScrollView(scroll, alwaysShowHorizontal, alwaysShowVertical, Utility.emptyLayoutOption);
    }

    public static Vector2 BeginScrollView(Vector2 scroll, bool alwaysShowHorizontal, bool alwaysShowVertical, params LayoutOption[] options)
    {
        if (Utility.currentCustomEvent != null) return scroll;

        ScrollGroup g = (ScrollGroup)LayoutUtility.BeginLayoutGroup(GUIStyle.none, options, typeof(ScrollGroup));
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
        if (Utility.currentCustomEvent != null) return;

        LayoutUtility.EndLayoutGroup();
        GUI.EndScrollView();
    }

    public static void BeginArea(Rect screenRect)
    {
        if (Utility.currentCustomEvent != null) return;

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

    public static void BeginArea(Rect screenRect, GUIStyle style)
    {
        if (style != null)
            GUI.Box(screenRect, GUIContent.none, style);
        BeginArea(screenRect);
    }

    public static void EndArea()
    {
        if (Utility.currentCustomEvent != null) return;

        LayoutUtility.current.layoutGroups.Pop();
        LayoutUtility.current.topLevel = (LayoutGroup)LayoutUtility.current.layoutGroups.Peek();
        GUI.EndGroup();
    }

    #endregion

    #region GUI Begin and End

    internal static Dictionary<int, Queue<Event>> _pendingEvents = new Dictionary<int, Queue<Event>>();

    internal static Event _currentCustomEvent = null;

    internal static Dictionary<int, Event> _currentCustomEvents = new Dictionary<int, Event>();

    static bool _isEditorWindow = false;

    static int _currentGUIID = 0;

    static float _currentScale = 1f;

    static GUISkin _oldSkin = null;

    public static void BeginGUI(UnityEngine.Object obj)
    {
        BeginGUI(obj, Utility.screenRect);
    }

    public static void BeginGUI(UnityEngine.Object obj, Rect position, GUISkin skin = null, float scale = 1)
    {
        BeginGUI(obj.GetInstanceID(), position, skin, scale,
#if UNITY_EDITOR
            obj is UnityEditor.EditorWindow || obj is UnityEditor.Editor
#else
            false
#endif
            );
    }

    public static void BeginGUI(int id, Rect position, GUISkin skin = null, float scale = 1, bool isEditorWindow = false)
    {
        if (id == 0) throw new ArgumentException("GUIID should never be zero");

        if (_currentCustomEvents.ContainsKey(id))
        {
            if (ev.type != EventType.Layout)
            {
                // If you're looking why the GUI flashing during custom event
                // (such as after selecting menu popups or closing dialogs)
                // This is the why. The runtime GUI has NO WAY to actually
                // sending custom event. We can only 'eats' it. Including repaint event.
                // We'll looking for better option if found.
                GUIUtility.ExitGUI();
            }
            else
            {
                _currentCustomEvents.Remove(id);
            }
        }

        if (ev.type != EventType.Repaint)
        {
            Queue<Event> pends;
            if (_pendingEvents.TryGetValue(id, out pends) && pends.Count > 0)
            {
                // Lets hijack this event
                Event.current = _currentCustomEvent = _currentCustomEvents[id] = pends.Dequeue();
            }
        }

        if (skin)
        {
            _oldSkin = GUI.skin;
            GUI.skin = skin;
        }

        _isEditorWindow = isEditorWindow;

        LayoutUtility.Begin(_currentGUIID = id);

        if ((_currentScale = scale) != 1f)
        {
            // Activate scaling
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            var pos = new Rect(position.position / scale, position.size / scale);
            BeginArea(pos);
        }
        else
            BeginArea(position);
    }

    public static void EndGUI()
    {
        EndArea();

        if (ev.type != EventType.Used && PopupBase.shownPopup.ContainsKey(Utility.currentGUIID))
        {
            PopupBase.shownPopup[Utility.currentGUIID].OnGUI();
        }

        if (Utility.delayCall != null)
        {
            Utility.delayCall();
            Utility.delayCall = null;
        }

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

        if (_currentCustomEvent != null)
            Event.current = _currentCustomEvent = null; // Reset to master event

        if (_oldSkin)
        {
            GUI.skin = _oldSkin;
            _oldSkin = null;
        }

        GUI.matrix = Matrix4x4.identity;
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

    #endregion

    #region Behaviour Blocks

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
            if (ev.type == EventType.Layout)
            {
                // Standard duration for animated foldout is 0.4 seconds
                fade += (val ? 1 : -1) * Time.unscaledDeltaTime * 2.5f;
                fade = Mathf.Clamp(fade, 0, 1);
                _fadeTimes[id] = fade;
            }
            if (val)
            {
                var f = 1 - fade;
                BeginFadeGroup(1 - f * f * f * f);
            }
            else
                BeginFadeGroup(fade * fade * fade * fade);

            return fade > 0;
        }
    }

    public static bool BeginFadeGroup(float value)
    {
        if (Utility.currentCustomEvent != null) return value > 0;

        // Fade groups interfere with layout even when the fade group is collapsed because
        // the margins of the elements before and after are added together instead of overlapping.
        // This creates unwanted extra space between controls if there's an inactive fade group in between.
        // We avoid this by simply not having the fade group if it's collapsed.
        if (value == 0)
        {
            return false;
        }
        // Active fade groups interfere with styles that have overflow, because the overflow part gets clipped.
        // We avoid this by simply not having the fade group if it's fully expanded.
        if (value == 1)
        {
            return true;
        }

        LayoutFadeGroup g = (LayoutFadeGroup)LayoutUtility.BeginLayoutGroup(GUIStyle.none, null, typeof(LayoutFadeGroup));
        g.isVertical = true;
        g.resetCoords = true;
        g.fadeValue = value;
        g.wasGUIEnabled = GUI.enabled;
        g.guiColor = GUI.color;
        if (value != 0.0f && value != 1.0f && Event.current.type == EventType.MouseDown)
        {
            Event.current.Use();
        }

        // We don't want the fade group gui clip to be used for calculating the label width of controls in this fade group, so we lock the context width.
        //EditorGUIUtility.LockContextWidth();
        GUI.BeginGroup(g.rect);

        return value != 0;
    }

    public static void EndFadeGroup()
    {
        if (Utility.currentCustomEvent != null) return;

        // If we're inside a fade group, end it here.
        // See BeginFadeGroup for details on why it's not always present.
        LayoutFadeGroup g = LayoutUtility.topLevel as LayoutFadeGroup;
        if (g != null)
        {
            GUI.EndGroup();
            //EditorGUIUtility.UnlockContextWidth();
            GUI.enabled = g.wasGUIEnabled;
            GUI.color = g.guiColor;
            LayoutUtility.EndLayoutGroup();
        }
    }

    static Stack<bool> enabledStacks = new Stack<bool>();

    public static void BeginDisabledGroup(bool disabled, bool overriding = false)
    {
        enabledStacks.Push(GUI.enabled);
        if (overriding)
            GUI.enabled = !disabled;
        else
            GUI.enabled &= !disabled;
    }

    public static void EndDisabledGroup()
    {
        GUI.enabled = enabledStacks.Pop();
    }

    static Stack<ValueTuple<int, IndentPolicy>> guiIndentPolicies = new Stack<ValueTuple<int, IndentPolicy>>();

    public static void BeginIndent(IndentPolicy policy)
    {
        BeginIndent(-1, policy);
    }

    public static void BeginIndent(int indent = -1, IndentPolicy policy = IndentPolicy.Inherit)
    {
        guiIndentPolicies.Push(new ValueTuple<int, IndentPolicy>(indentLevel, indentPolicy));

        if (indent >= 0)
            indentLevel = indent;
        else
            indentLevel++;

        if (policy != IndentPolicy.Inherit)
            indentPolicy = policy;
    }

    public static void EndIndent()
    {
        var val = guiIndentPolicies.Pop();
        indentLevel = val.item1;
        indentPolicy = val.item2;
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
        if (side == Side.Bottom || side == Side.Top)
            prefixLabelWidth = GUI.skin.label.CalcHeight(C(" "), 60);
    }

    public static void EndLabelOption()
    {
        var o = labelStacks.Pop();
        prefixLabelWidth = o.item1;
        prefixLabelSide = o.item2;
    }

    #endregion

}
