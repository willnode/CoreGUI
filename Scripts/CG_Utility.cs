using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static partial class CoreGUI
{
    /// <summary>
    /// Event.current shortcut
    /// </summary>
    public static Event ev { get { return Event.current; } }

    private struct ValueTuple<T1, T2>
    {
        public T1 item1;
        public T2 item2;

        public ValueTuple(T1 t1, T2 t2)
        {
            item1 = t1;
            item2 = t2;
        }
    }

    public static class Scoped
    {
        public static DisposableScope Horizontal(GUIContent label, params LayoutOption[] options)
        {
            BeginHorizontal(label, options);
            return new DisposableScope(EndHorizontal);
        }

        public static DisposableScope Horizontal(GUIContent label = null)
        {
            BeginHorizontal(label);
            return new DisposableScope(EndHorizontal);
        }

        public static DisposableScope Vertical(GUIContent label, params LayoutOption[] options)
        {
            BeginVertical(label, options);
            return new DisposableScope(EndVertical);
        }

        public static DisposableScope Vertical(GUIContent label = null)
        {
            BeginVertical(label);
            return new DisposableScope(EndVertical);
        }
        
        public static DisposableScope Indent(int indent = -1)
        {
            BeginIndent(indent);
            return new DisposableScope(EndIndent);
        }

        public static DisposableScope ScrollView(ref Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
        {
            BeginScrollView(ref scroll, alwaysShowHorizontal, alwaysShowVertical);
            return new DisposableScope(EndScrollView);
        }

        public static DisposableScope ScrollView(ref Vector2 scroll, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false, params LayoutOption[] options)
        {
            BeginScrollView(ref scroll, alwaysShowHorizontal, alwaysShowVertical, options);
            return new DisposableScope(EndScrollView);
        }

        public static DisposableScope LabelOption(Side side)
        {
            return LabelOption(-1, side);
        }

        public static DisposableScope LabelOption(int width = -1, Side side = Side.Inherit)
        {
            BeginLabelOption(width, side);
            return new DisposableScope(EndLabelOption);
        }

        public static DisposableScope LayoutOption(params LayoutOption[] options)
        {
            BeginLayoutOption(options);
            return new DisposableScope(EndLayoutOption);
        }

        public static DisposableScope DisabledGroup(bool disabled, bool overriding = false)
        {
            BeginDisabledGroup(disabled, overriding);
            return new DisposableScope(EndDisabledGroup);
        }

        public static void FadeGroup(bool expanded, Action whenExpanded)
        {
            if (BeginFadeGroup(expanded))
                whenExpanded();
            EndFadeGroup();
        }

        public static DisposableScope ChangeCheck(Action whenChanged)
        {
            BeginChangeCheck();
            return new DisposableScope(() =>
            {
                if (EndChangeCheck())
                    whenChanged();
            });
        }
    }

    public struct DisposableScope : IDisposable
    {
        public readonly Action dispose;

        public DisposableScope(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            dispose();
        }
    }

    public static class Utility
    {

        /// <summary>
        /// Screen size Rect shortcut
        /// </summary>
        public static Rect screenRect { get { return new Rect(0, 0, Screen.width, Screen.height); } }


        /// <summary>
        /// Screen size Rect / currentScale shortcut
        /// </summary>
        public static Rect scaledScreenRect { get { return new Rect(0, 0, Screen.width / _currentScale, Screen.height / _currentScale); } }

        /// <summary>
        /// Delegate to be called once before EndGUI()
        /// </summary>
        public static Action delayCall;

        static Dictionary<int, Rect> _safeRects = new Dictionary<int, Rect>();

        /// <summary>
        /// Provide extra layer to obtain last known rect in case it's in Layout mode.
        /// </summary>
        public static Rect GetSafeRect(Rect r)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);
            Rect r2;
            _safeRects.TryGetValue(id, out r2);

            if (ev.type == EventType.Used || ev.type == EventType.Layout)
            {
                return r2;
            }
            else
            {
                if (r != r2)
                {
                    _safeRects[id] = r;
#if UNITY_EDITOR
                    // Because EditorWindow don't get repainted every frame...
                    if (isEditorWindow)
                        InternalRepaintEditorWindow();
#endif
                }
                return r;
            }
        }


        public static float ScaleFromScreen(Vector2 referenceResolution, ScreenMatchMode matchMode, float matchRatio, Rect screenSize)
        {
            // Taken from CanvasScaler.cs of Unity UI
            switch (matchMode)
            {
                case ScreenMatchMode.MatchRatio:

                    // We take the log of the relative width and height before taking the average.
                    // Then we transform it back in the original space.
                    // the reason to transform in and out of logarithmic space is to have better behavior.
                    // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                    // In normal space the average would be (0.5 + 2) / 2 = 1.25
                    // In logarithmic space the average is (-1 + 1) / 2 = 0
                    float logWidth = Mathf.Log(screenSize.x / referenceResolution.x, 2);
                    float logHeight = Mathf.Log(screenSize.y / referenceResolution.y, 2);
                    float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, matchRatio);
                    return Mathf.Pow(2, logWeightedAverage);

                case ScreenMatchMode.Expand:
                    return Mathf.Min(screenSize.x / referenceResolution.x, screenSize.y / referenceResolution.y);
                case ScreenMatchMode.Shrink:
                    return Mathf.Max(screenSize.x / referenceResolution.x, screenSize.y / referenceResolution.y);
                default:
                    return 1;
            }
        }

        public static float ScaleFromDPI(float referenceDPI)
        {
            var curDPI = Screen.dpi;
            if (curDPI > 0)
                return ScaleFromDPI(referenceDPI, curDPI);
            else
                return 1; // Maybe there's better idea?
        }

        public static float ScaleFromDPI(float referenceDPI, float currentDPI)
        {
            return currentDPI / referenceDPI;
        }

        // Dictionary<"int of control ID", ValueTuple<"last returned value", "non-deferred value">>
        static Dictionary<int, ValueTuple<object, object>> _deferredValues = new Dictionary<int, ValueTuple<object, object>>();

        /// <summary>
        /// Delay value returned from widget
        /// </summary>
        public static T GetDeferredValue<T>(T value, Func<T, T> widget)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);

            ValueTuple<object, object> tuple;

            bool flag = _deferredValues.TryGetValue(id, out tuple);

            if (!flag || !tuple.item1.Equals(value))
            {
                // cache is invalid. Plz update
                tuple.item1 = value;
                tuple.item2 = value;
            }

            var shouldSet = ev.type == EventType.MouseUp || (ev.type == EventType.KeyUp &&
                (ev.keyCode == KeyCode.Tab || ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.Escape));

            var val2 = widget((T)tuple.item2);
            {
                if (shouldSet)
                    tuple.item1 = val2;
                tuple.item2 = val2;
                _deferredValues[id] = tuple;
            }
            return (T)tuple.item1;
        }

        /// <summary>
        /// Current GUI Identifier (GUIID)
        /// </summary>
        public static int currentGUIID { get { return _currentGUIID; } }

        /// <summary>
        /// Is current GUI running inside EditorWindow?
        /// </summary>
        public static bool isEditorWindow { get { return _isEditorWindow; } }

        /// <summary>
        /// Current custom event that being executed
        /// </summary>
        public static Event currentCustomEvent { get { return _currentCustomEvent; } }

        /// <summary>
        /// Send custom event to targeted GUIID
        /// </summary>
        public static void SendEvent(Event customEvent, int GUIID = 0)
        {
            if (GUIID == 0)
                GUIID = currentGUIID;

#if UNITY_EDITOR
            if (isEditorWindow)
                // The editor has builtin custom event sender. No flashing necessary.
                UnityEditor.EditorWindow.focusedWindow.SendEvent(customEvent);
            else
#endif
            {
                Queue<Event> ev;
                if (!_pendingEvents.TryGetValue(GUIID, out ev))
                    _pendingEvents[GUIID] = ev = new Queue<Event>();

                ev.Enqueue(customEvent);
            }
        }

        /// <summary>
        /// Send custom ExecuteCommand event.
        /// (the executeCommandKey is sent to Event.keyCode for IL2CPP bug workaround)
        /// (executeCommandKey can accept any short value, don't have to what exist in enumeration)
        /// </summary>
        public static void SendEvent(string executeCommandName, KeyCode executeCommandKey, int GUIID = 0)
        {
            Event e;
#if UNITY_EDITOR
            if (isEditorWindow)
            {
                e = UnityEditor.EditorGUIUtility.CommandEvent(executeCommandName);
                e.keyCode = executeCommandKey;
            }
            else
#endif
            {
                e = new Event(ev);
                e.type = EventType.ExecuteCommand;
                e.commandName = executeCommandName;
                e.keyCode = executeCommandKey;
            }
            SendEvent(e, GUIID);
        }


        /// <summary>
        /// High DPI scaling factor similar to EditorGUILayout.pixelsPerPoint
        /// </summary>
        public static float pixelsPerPoint { get { return _pixelsPerPoint(); } }

        static Func<float> _pixelsPerPoint;

        /// <summary>
        /// Call to internal EditorWindow repaint hook
        /// </summary>
        public static readonly Action InternalRepaintEditorWindow;

        static Func<GUIStyle, GUIContent, Vector2, Vector2> calcSizeWithConstraints;

        static Utility()
        {
            var m = typeof(GUIStyle).GetMethod("CalcSizeWithConstraints", BindingFlags.NonPublic | BindingFlags.Instance);
            calcSizeWithConstraints = (Func<GUIStyle, GUIContent, Vector2, Vector2>)Delegate.CreateDelegate(typeof(Func<GUIStyle, GUIContent, Vector2, Vector2>), m);
            var f = typeof(GUIUtility).GetMethod("get_pixelsPerPoint", BindingFlags.NonPublic | BindingFlags.Static);
            _pixelsPerPoint = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), f);
            var f2 = typeof(GUI).GetMethod("InternalRepaintEditorWindow", BindingFlags.Static | BindingFlags.NonPublic);
            InternalRepaintEditorWindow = (Action)Delegate.CreateDelegate(typeof(Action), f2);
        }

        /// <summary>
        /// Calculate GUIStyle size with constraints (exposed via reflection)
        /// </summary>
        public static Vector2 CalcSizeWithConstraints(GUIStyle style, GUIContent content, Vector2 constraints)
        {
            return calcSizeWithConstraints(style, content, constraints);
        }

        /// <summary>
        /// Immutable, non-allocating empty layout option
        /// </summary>
        public readonly static LayoutOption[] emptyLayoutOption = new LayoutOption[] { };

        static GUILayoutOption[] _xPand = new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true) };
        static GUILayoutOption[] _xNoPand = new GUILayoutOption[] { };

        /// <summary>
        /// Reserve and get a position from Unity's GUILayout
        /// </summary>
        public static Rect BindFromGUILayout(bool expanded = true)
        {
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, expanded ? _xPand : _xNoPand);
            return GetSafeRect(r);
        }

        /// <summary>
        /// Get screen position from RectTransform of Overlay-based UI Canvas
        /// </summary>
        public static Rect BindFromUIOverlay(RectTransform transform, float padding = 0)
        {
            var min = transform.TransformPoint(transform.rect.min);
            var max = transform.TransformPoint(transform.rect.max);
            min.y = Screen.height - min.y;
            max.y = Screen.height - max.y;
            return Rect.MinMaxRect(min.x + padding, max.y + padding, max.x - padding, min.y - padding);
        }

        /// <summary>
        /// Opinionated GUIStyle drawing function
        /// </summary>
        public static void DrawStyle(Rect r, GUIContent content, GUIStyle style, int id, bool on = false)
        {
            // Primary reason why not simply giveaway ControlID:
            // The button is not appear to be clicked (active) even pressed via keyboard
            // (requires mouse to hover them). So we build our own.
            style.Draw(r, content ?? GUIContent.none, r.Contains(ev.mousePosition) || GUIUtility.hotControl == id,
                GUIUtility.hotControl == id, on, GUIUtility.keyboardControl == id);
        }

        public static GUIContent[] ToGUIContents(params string[] texts) { return ToGUIContents(texts, null, null); }
        public static GUIContent[] ToGUIContents(params Texture2D[] textures) { return ToGUIContents(null, textures, null); }
        public static GUIContent[] ToGUIContents(string[] texts, Texture2D[] textures) { return ToGUIContents(texts, textures, null); }
        public static GUIContent[] ToGUIContents(string[] texts, string[] tooltips) { return ToGUIContents(texts, null, tooltips); }
        public static GUIContent[] ToGUIContents(Texture2D[] textures, string[] tooltips) { return ToGUIContents(null, textures, tooltips); }

        /// <summary>
        /// Create new GUIContents from array
        /// </summary>
        public static GUIContent[] ToGUIContents(string[] texts, Texture[] textures, string[] tooltips)
        {
            var length = Mathf.Min(texts == null ? int.MaxValue : texts.Length,
                Mathf.Min(textures == null ? int.MaxValue : textures.Length,
                tooltips == null ? int.MaxValue : tooltips.Length));

            if (length == int.MaxValue)
                throw new ArgumentException("At least one of argument must be non-null");

            var result = new GUIContent[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = new GUIContent(
                    texts == null ? null : texts[i],
                    textures == null ? null : textures[i],
                    tooltips == null ? null : tooltips[i]
                    );
            }

            return result;
        }
    }
    
    public enum Side
    {
        Left, Right, Top, Bottom, Inherit = -1
    }

    public enum ScreenMatchMode
    {
        Expand = 0,
        Shrink = 1,
        MatchRatio = 2,
    }

}
