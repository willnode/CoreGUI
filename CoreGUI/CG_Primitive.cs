using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    static double lastDoubleFail = double.NaN;
    static string lastDoubleFailStr;

    public static double DoubleField(GUIContent label, double value)
    {
        var r = PrefixLabel(Reserve(), label);
        var v = GUI.TextField(r, lastDoubleFail == value ? lastDoubleFailStr : value.ToString());
        if (GUI.changed)
        {
            double f;
            if (double.TryParse(v, out f))
            {
                lastDoubleFailStr = v;
                lastDoubleFail = value;
                return f;
            }
            else
            {
                lastDoubleFailStr = v;
                lastDoubleFail = value;
            }
        }
        return value;
    }

    static float lastFloatFail = float.NaN;
    static string lastFloatFailStr;

    public static float FloatField(GUIContent label, float value)
    {
        var r = PrefixLabel(Reserve(), label);
        var v = GUI.TextField(r, lastFloatFail == value ? lastFloatFailStr : value.ToString());
        if (GUI.changed)
        {
            float f;
            if (float.TryParse(v, out f))
            {
                lastFloatFailStr = v;
                lastFloatFail = value;
                return f;
            }
            else
            {
                lastFloatFailStr = v;
                lastFloatFail = value;
            }
        }
        return value;
    }

    static int lastIntFail = 0;
    static string lastIntFailStr;

    public static int IntField(GUIContent label, int value)
    {
        var r = PrefixLabel(Reserve(), label);
        var v = GUI.TextField(r, lastIntFail == value ? lastIntFailStr : value.ToString());
        if (GUI.changed)
        {
            int f;
            if (int.TryParse(v, out f))
            {
                lastIntFailStr = v;
                lastIntFail = value;
                return f;
            }
            else
            {
                lastIntFailStr = v;
                lastIntFail = value;
            }
        }
        return value;
    }

    public static string StringField(GUIContent label, string value)
    {
        var r = PrefixLabel(Reserve(), label);
        return GUI.TextField(r, value);
    }

    public static string StringArea(GUIContent label, string value)
    {
        return StringArea(label, value, 0, int.MaxValue);
    }
    
    public static string StringArea(GUIContent label, string value, int minLines, int maxLines, bool scrollBar = true)
    {
        BeginLayoutOption(GUILayout.ExpandWidth(true));
        var r = PrefixLabel(Reserve(Vector2.zero), label);
        EndLayoutOption();

        var style = GUI.skin.textArea;
        var s = style.CalcHeight(C(value), r.width);
        if (minLines > 0 && maxLines < int.MaxValue)
        {
            var sz = style.fontSize <= 0 ? 16 : style.fontSize;
            var s2 = Mathf.Clamp(s, minLines * sz, maxLines * sz + style.padding.vertical);

            if (s2 < s && scrollBar)
            {
                Reserve(new Vector2(0, s2));
                r.height = s2;
                r.width -= 14;
                var text = GUI.TextArea(r, value);
                TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

                if (editor.text == null || editor.text != text)
                    return text; //Maybe we catch the wrong one?

                r.width += 14;
                r.xMin = r.xMax - 14;
                s = style.CalcHeight(C(value), r.width - 14f);

                editor.scrollOffset.y = GUI.VerticalScrollbar(r, editor.scrollOffset.y, r.height, 0, s);
                
                return text;
            }
            else
                s = s2;
        }

        Reserve(new Vector2(0, s));
        r.height = s;
        return GUI.TextArea(r, value);
    }
}
