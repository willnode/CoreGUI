using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    static public float FloatSlider(GUIContent label, float value, float min, float max)
    {
        BeginHorizontal(label);
        value = HorizontalSlider(null, value, min, max);
        BeginLayoutOption(GUILayout.Width(80));
        BeginChangeCheck();
        value = FloatField(null, value);
        if (EndChangeCheck())
            value = Mathf.Clamp(value, min, max);
        EndLayoutOption();
        EndHorizontal();
        return value;
    }

    static public int IntSlider(GUIContent label, int value, int min, int max)
    {
        BeginHorizontal(label);
        value = HorizontalSlider(null, value, min, max);
        BeginLayoutOption(GUILayout.Width(80));
        BeginChangeCheck();
        value = IntField(null, value);
        if (EndChangeCheck())
            value = Mathf.Clamp(value, min, max);
        EndLayoutOption();
        EndHorizontal();
        return value;
    }

    public static double DoubleField(GUIContent label, double value)
    {
        return NumberField(label, value, double.TryParse);
    }

    public static float FloatField(GUIContent label, float value)
    {
        return NumberField(label, value, float.TryParse);
    }

    public static int IntField(GUIContent label, int value)
    {
        return NumberField(label, value, int.TryParse);
    }
    
    public delegate bool TryParseFunc<T2>(string s, out T2 result);

    static string lastNumberStr;
    static int lastNumberID;
    static int lastNumberAnyID;

    static public T NumberField<T>(GUIContent label, T value, TryParseFunc<T> parser)
    {
        var r = PrefixLabel(Reserve(null, GUI.skin.textField), label);
        BeginChangeCheck();

        var id = GUIUtility.GetControlID(FocusType.Passive);

        if (lastNumberAnyID != GUIUtility.keyboardControl)
        {
            lastNumberAnyID = GUIUtility.keyboardControl;
            lastNumberID = -1;
        }

        var v = GUI.TextField(r, lastNumberID == id ? lastNumberStr : value.ToString());

        if (EndChangeCheck())
        {
            lastNumberID = id;

            T f;
            lastNumberStr = v;

            if (parser(v, out f))
                return f;
            else if (v.Length == 0)
                 return default(T);
        }

        return value;
    }
    
    static public float HorizontalSlider(GUIContent label, float value, float min, float max)
    {
        var r = PrefixLabel(Reserve(), label);
        r.y += (r.height - GUI.skin.horizontalScrollbar.fixedHeight) / 2;
        return GUI.HorizontalSlider(r, value, min, max);
    }
    
    static public int HorizontalSlider(GUIContent label, int value, int min, int max)
    {
        var r = PrefixLabel(Reserve(), label);
        r.y += (r.height - GUI.skin.horizontalScrollbar.fixedHeight) / 2;
        return Mathf.RoundToInt(GUI.HorizontalSlider(r, value, min, max));
    }

    public static string TextField(GUIContent label, string value)
    {
        var r = PrefixLabel(Reserve(), label);
        return GUI.TextField(r, value);
    }
    
    public static string PasswordField(GUIContent label, string value, char maskChar)
    {
        var r = PrefixLabel(Reserve(), label);
        return GUI.PasswordField(r, value ?? "", maskChar);
    }
    
    public static string TextArea(GUIContent label, string value)
    {
        return TextArea(label, value, 0, int.MaxValue);
    }
    
    public static string TextArea(GUIContent label, string value, int minLines, int maxLines, bool scrollBar = true)
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
