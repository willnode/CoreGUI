using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    public static Color ColorField(GUIContent label, Color color, bool hdr = false)
    {
        return Color.white;
    }

    public static Rect RectField(GUIContent label, Rect rect)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        BeginVertical();
        {
            rect.x = FloatField(C("X"), rect.x);
            rect.width = FloatField(C("W"), rect.width);
        }
        EndVertical();
        BeginVertical();
        {
            rect.y = FloatField(C("Y"), rect.y);
            rect.height = FloatField(C("H"), rect.height);
        }
        EndVertical();
        EndLabelOption();
        EndHorizontal();
        return rect;
    }
    
    public static Quaternion QuaternionField(GUIContent label, Rect rect, bool raw = false)
    {
        return new Quaternion();
    }

    public static Bounds BoundsField(GUIContent label, Bounds bounds)
    {
        return new Bounds();
    }

    public static Vector2 VectorField(GUIContent label, Vector2 vector)
    {
        return new Vector2();
    }

    public static Vector3 VectorField(GUIContent label, Vector3 vector)
    {
        return new Vector2();
    }

    public static Vector4 VectorField(GUIContent label, Vector4 vector)
    {
        return new Vector2();
    }

    public static Vector2Int VectorField(GUIContent label, Vector2Int vector)
    {
        return new Vector2Int();
    }

    public static Vector3Int VectorField(GUIContent label, Vector3Int vector)
    {
        return new Vector3Int();
    }
}

