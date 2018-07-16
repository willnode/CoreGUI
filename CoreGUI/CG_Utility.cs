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
        /// <summary>
        /// Get Rect from Current BeginArea/BeginHorizontal/BeginVertical
        /// </summary>
        public static Rect GetTopLayoutRect()
        {
            return (Rect)topLevelRect.GetValue(topLevel());
        }

        public static bool GetTopLayoutIsVertical()
        {
            return (bool)topLevelIsVertical.GetValue(topLevel());
        }

        static Func<object> topLevel;

        static FieldInfo topLevelRect;

        static FieldInfo topLevelIsVertical;

        static Utility()
        {
            var m = typeof(GUILayoutUtility).GetMethod("get_topLevel", BindingFlags.NonPublic | BindingFlags.Static);
            topLevel = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), m);
            topLevelRect = m.ReturnType.GetField("rect");
            topLevelIsVertical = m.ReturnType.GetField("isVertical");
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
