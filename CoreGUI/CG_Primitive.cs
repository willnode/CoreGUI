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
        var r = PrefixLabel(Indent(Reserve()), label);
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
        var r = PrefixLabel(Indent(Reserve()), label);
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
        var r = PrefixLabel(Indent(Reserve()), label);
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
        var r = PrefixLabel(Indent(Reserve()), label);
        return GUI.TextField(r, value);
    }

    public static string StringArea(GUIContent label, string value)
    {
        var r = PrefixLabel(Indent(Reserve()), label);
        return GUI.TextArea(r, value);
    }
}
