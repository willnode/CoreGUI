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
        public readonly T1 item1;
        public readonly T2 item2;

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

        static Dictionary<int, Rect> _safeRects = new Dictionary<int, Rect>();

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
                    InternalRepaintEditorWindow();
#endif
                }
                return r;
            }
        }

        public static float pixelsPerPoint { get { return _pixelsPerPoint(); } }

        public static Func<float> _pixelsPerPoint;

        public static Action InternalRepaintEditorWindow;

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

        public static Vector2 CalcSizeWithConstraints(GUIStyle style, GUIContent content, Vector2 constraints)
        {
            return calcSizeWithConstraints(style, content, constraints);
        }

        public readonly static LayoutOption[] emptyLayoutOption = new LayoutOption[] { };

        static GUILayoutOption[] _xPand = new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true) };
        static GUILayoutOption[] _xNoPand = new GUILayoutOption[] { };

        public static Rect BindFromGUILayout(bool expanded = true)
        {
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, expanded ? _xPand : _xNoPand);
            return GetSafeRect(r);
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
