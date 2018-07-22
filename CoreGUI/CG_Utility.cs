using System;
using System.Collections.Generic;
using System.Linq;
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

        public static DisposableScope Indent(IndentPolicy policy)
        {
            return Indent(-1, policy);
        }

        public static DisposableScope Indent(int indent = -1, IndentPolicy policy = IndentPolicy.Inherit)
        {
            BeginIndent(indent, policy);
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

        // Dictionary<"int of control ID", ValueTuple<"last returned value", "non-deferred value">>
        static Dictionary<int, ValueTuple<object, object>> _deferredValues = new Dictionary<int, ValueTuple<object, object>>();

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
        /// Reserve a position from Unity's GUILayout
        /// </summary>
        public static Rect BindFromGUILayout(bool expanded = true)
        {
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, expanded ? _xPand : _xNoPand);
            return GetSafeRect(r);
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

}
