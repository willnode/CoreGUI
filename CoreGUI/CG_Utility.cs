using System;
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

    public static class Utility
    {

        public static float pixelsPerPoint { get { return _pixelsPerPoint(); } }

        public static Func<float> _pixelsPerPoint;

        public static GUIStyle spaceStyle { get; internal set; }

        static Func<GUIStyle, GUIContent, Vector2, Vector2> calcSizeWithConstraints;
        
        static Utility()
        {
            var m = typeof(GUIStyle).GetMethod("CalcSizeWithConstraints", BindingFlags.NonPublic | BindingFlags.Instance);
            calcSizeWithConstraints = (Func<GUIStyle, GUIContent, Vector2, Vector2>)Delegate.CreateDelegate(typeof(Func<GUIStyle, GUIContent, Vector2, Vector2>), m);
            var f = typeof(GUIUtility).GetMethod("get_pixelsPerPoint", BindingFlags.NonPublic | BindingFlags.Static);
            _pixelsPerPoint = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), f);
        }

        public static Vector2 CalcSizeWithConstraints(GUIStyle style, GUIContent content, Vector2 constraints)
        {
            return calcSizeWithConstraints(style, content, constraints);
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
