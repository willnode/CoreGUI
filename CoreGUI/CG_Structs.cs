#if !(UNITY_2017 || UNITY_5 || UNITY_4)
#define INT_STRUCTS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    public static Color ColorField(GUIContent label, Color color, bool alpha = true)
    {
        using (Scoped.Horizontal(label))
        {
            var r = Reserve();
            var id = GUIUtility.GetControlID(FocusType.Keyboard);
#if UNITY_EDITOR
            if (Utility.isEditorWindow)
#if UNITY_2017 || UNITY_5
                return UnityEditor.EditorGUI.ColorField(r, GUIContent.none, color, true, alpha, false, null);
#else
                return UnityEditor.EditorGUI.ColorField(r, GUIContent.none, color, true, alpha, false);
#endif
#endif
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
            else if (ev.type == EventType.MouseUp && r.Contains(ev.mousePosition))
            {
                ColorDialogPopup.Show(id, color);
            }
            else if (ev.type == EventType.ExecuteCommand)
            {
                var c = PopupCallback.GetValue(id);
                if (c != null)
                    return (Color)c;
            }
            return color;
        }
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

    public static Quaternion QuaternionField(GUIContent label, Quaternion quaternion, bool asEuler = true)
    {
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        {
            if (asEuler)
            {
                BeginChangeCheck();
                var eul = quaternion.eulerAngles;
                eul.x = FloatField(C("X"), eul.x);
                eul.y = FloatField(C("Y"), eul.y);
                eul.z = FloatField(C("Z"), eul.z);
                if (EndChangeCheck())
                    quaternion = Quaternion.Euler(eul);
            }
            else
            {
                quaternion.x = FloatField(C("X"), quaternion.x);
                quaternion.y = FloatField(C("Y"), quaternion.y);
                quaternion.z = FloatField(C("Z"), quaternion.z);
                quaternion.w = FloatField(C("W"), quaternion.w);
            }
        }
        EndLabelOption();
        EndHorizontal();
        return quaternion;
    }

    public static Bounds BoundsField(GUIContent label, Bounds bounds)
    {
        BeginVertical(label);
        BeginLabelOption(38, Side.Left);
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

    public static RectOffset RectOffsetField(GUIContent label, RectOffset rect)
    {
        bool flag = rect == null;
        if (flag) rect = new RectOffset();

        BeginChangeCheck();
        BeginHorizontal(label);
        BeginLabelOption(16, Side.Left);
        BeginVertical();
        {
            rect.left = IntField(C("L"), rect.left);
            rect.right = IntField(C("R"), rect.right);
        }
        EndVertical();
        BeginVertical();
        {
            rect.top = IntField(C("T"), rect.top);
            rect.bottom = IntField(C("B"), rect.bottom);
        }
        EndVertical();
        EndLabelOption();
        EndHorizontal();

        if (EndChangeCheck())
            return rect;
        else if (flag)
            return null;
        else
            return rect;
    }

#if INT_STRUCTS

    public static BoundsInt BoundsField(GUIContent label, BoundsInt bounds)
    {
        BeginVertical(label);
        BeginLabelOption(38, Side.Left);
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

