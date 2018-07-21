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
        BeginLayoutOption(Layout.Width(80));
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
        BeginLayoutOption(Layout.Width(80));
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
        return NumberField(label, value, float.TryParse, (v, d) => v == 0 ? d : v + d * 0.1f);
    }

    public static int IntField(GUIContent label, int value)
    {
        return NumberField(label, value, int.TryParse, (v, d) => v + (int)d);
    }

    public delegate bool TryParseFunc<T2>(string s, out T2 result);
    

    static string lastNumberStr;
    static int lastNumberID;
    static int lastNumberAnyID;

    static public T NumberField<T>(GUIContent label, T value, TryParseFunc<T> parser, Func<T, float, T> deltaProcessor = null)
    {
        BeginChangeCheck();

        var id = GUIUtility.GetControlID(FocusType.Passive);

        Rect r = Reserve(null, GUI.skin.textField);

        if (deltaProcessor == null)
            r = PrefixLabel(r, label, id + 1);
        else
        {
            float delta;
            r = PrefixSlider(r, label, id, out delta);
            if (delta != 0)
                value = deltaProcessor(value, delta);
        }

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
        var id = GUIUtility.GetControlID(FocusType.Keyboard);

        if (GUIUtility.keyboardControl == id && ev.type == EventType.KeyDown)
        {
            if (ev.keyCode == KeyCode.LeftArrow || ev.keyCode == KeyCode.RightArrow)
            {
                var delta = Mathf.Pow(10, Mathf.Round(Mathf.Log10(Mathf.Abs(max - min))) - 2);
                delta *= (ev.keyCode == KeyCode.LeftArrow ? -1f : 1f) * (ev.shift ? 10f : 1f);
                value = Mathf.Clamp(value + delta, min, max);
                GUI.changed = true;
                Event.current.Use();
            }
        }

        return GUI.Slider(r, value, 0, min, max, Styles.HorizontalSlider, Styles.HorizontalSliderThumb, true, id);
    }

    static public int HorizontalSlider(GUIContent label, int value, int min, int max)
    {
        return Mathf.RoundToInt(HorizontalSlider(label, (float)value, min, max));
    }

    public static string TextField(GUIContent label, string value)
    {
        var pos = Reserve();
        var id = GUIUtility.GetControlID(FocusType.Passive, pos);
        var r = PrefixLabel(pos, label, id + 1);
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

    static Dictionary<int, Rect> s_textAreaLastRect = new Dictionary<int, Rect>();

    public static string TextArea(GUIContent label, string value, int minLines, int maxLines, bool scrollBar = true)
    {
        using (Scoped.Horizontal(label))
        {
            var style = GUI.skin.textField;
            if (minLines == 0 && maxLines == int.MaxValue)
            {
                // No need fancy scrollbar or special settings
                return GUI.TextArea(Reserve(C(value), style), value);
            }
            else
            {
                var id = GUIUtility.GetControlID(FocusType.Passive);
                var lastRect = s_textAreaLastRect.ContainsKey(id) ? s_textAreaLastRect[id] : new Rect(0, 0, Screen.width, Screen.height);

                var sz = style.lineHeight;
                var h = style.CalcHeight(C(value), lastRect.width);
                var h2 = Mathf.Clamp(h, minLines * sz, maxLines == int.MaxValue ? maxLines : sz * maxLines);
                var r = Reserve(new Vector2(0, h2));

                if (ev.type != EventType.Layout && ev.type != EventType.Used)
                    s_textAreaLastRect[id] = r;

                if (h != h2 && scrollBar)
                {
                    r.width -= 14;

                    var text = GUI.TextArea(r, value);

                    TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

                    if (editor.position != r)
                        return text; //Maybe we catch the wrong one?

                    var s = style.CalcHeight(C(value), r.width);
                    
                    r.xMin = r.xMax;
                    r.width = 14;

                    editor.scrollOffset.y = GUI.VerticalScrollbar(r, editor.scrollOffset.y, r.height, 0, s);

                    return text;
                }

                return GUI.TextArea(r, value);
            }
        }

    }
}
