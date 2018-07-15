using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    public static Color ColorField(GUIContent label, Color color, bool alpha = true)
    {
        BeginHorizontal(label);
        var r = Reserve();

        if (ev.type == EventType.Repaint)
        {
            var oldC = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b);
            if (alpha)
               r.height -= 4f;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            if (alpha)
            {
                r.y += r.height;
                r.height = 4f;
                GUI.color = Color.black;
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                r.width *= color.a;
                GUI.color = Color.white;
                GUI.DrawTexture(r, Texture2D.whiteTexture);
            }
            GUI.color = oldC;
        }

        EndHorizontal();
        return color;
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
    
    public static Quaternion QuaternionField(GUIContent label, Quaternion quaternion, bool euler = true)
    {
        return new Quaternion();
    }

    public static Bounds BoundsField(GUIContent label, Bounds bounds)
    {
        BeginVertical(label);
        BeginLabelOption(32, Side.Left);
        {
            bounds.min = VectorField(C("Min: "), bounds.min);
            bounds.max = VectorField(C("Max: "), bounds.max);
        }
        EndLabelOption();
        EndVertical();
        return bounds;
    }

    public static Vector2 VectorField(GUIContent label, Vector2 vector)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);      
        {
            vector.x = FloatField(C("X"), vector.x);
            vector.y = FloatField(C("Y"), vector.y);
        }
        EndLabelOption();
        EndHorizontal();
        return vector;
    }

    public static Vector3 VectorField(GUIContent label, Vector3 vector)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        {
            vector.x = FloatField(C("X"), vector.x);
            vector.y = FloatField(C("Y"), vector.y);
            vector.z = FloatField(C("Z"), vector.z);
        }
        EndLabelOption();
        EndHorizontal();
        return vector;
    }

    public static Vector4 VectorField(GUIContent label, Vector4 vector)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        {
            vector.x = FloatField(C("X"), vector.x);
            vector.y = FloatField(C("Y"), vector.y);
            vector.z = FloatField(C("Z"), vector.z);
            vector.w = FloatField(C("W"), vector.w);
        }
        EndLabelOption();
        EndHorizontal();
        return vector;
    }

#if !(UNITY_2017 || UNITY_5 || UNITY_4)

    public static BoundsInt BoundsField(GUIContent label, BoundsInt bounds)
    {
        BeginVertical(label);
        BeginLabelOption(32, Side.Left);
        {
            bounds.min = VectorField(C("min"), bounds.min);
            bounds.max = VectorField(C("max"), bounds.max);
        }
        EndLabelOption();
        EndVertical();
        return bounds;
    }

    public static Vector2Int VectorField(GUIContent label, Vector2Int vector)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        {
            vector.x = IntField(C("X"), vector.x);
            vector.y = IntField(C("Y"), vector.y);
        }
        EndLabelOption();
        EndHorizontal();
        return vector;
    }

    public static Vector3Int VectorField(GUIContent label, Vector3Int vector)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        {
            vector.x = IntField(C("X"), vector.x);
            vector.y = IntField(C("Y"), vector.y);
            vector.z = IntField(C("Z"), vector.z);
        }
        EndLabelOption();
        EndHorizontal();
        return vector;
    }
#endif
}

