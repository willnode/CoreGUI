#if !(UNITY_2017 || UNITY_5 || UNITY_4)
#define INT_STRUCTS
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class CoreGUI
{

    static Dictionary<Type, Func<GUIContent, object, object>> _propertyFieldCache;

    static CoreGUI()
    {
        InitializePropertyFieldCache();
    }

    static void InitializePropertyFieldCache()
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        _propertyFieldCache = new Dictionary<Type, Func<GUIContent, object, object>>();
        // Common
        AddPropHandler<bool>((l, v) => Checkbox(l, (bool)v));
        AddPropHandler<string>((l, v) => TextField(l, (string)v));
        // Numbers
        AddPropHandler<int>((l, v) => NumberField(l, (int)v, int.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<uint>((l, v) => NumberField(l, (uint)v, uint.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<long>((l, v) => NumberField(l, (long)v, long.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<ulong>((l, v) => NumberField(l, (ulong)v, ulong.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<short>((l, v) => NumberField(l, (short)v, short.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<ushort>((l, v) => NumberField(l, (ushort)v, ushort.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<byte>((l, v) => NumberField(l, (byte)v, byte.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<sbyte>((l, v) => NumberField(l, (sbyte)v, sbyte.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<float>((l, v) => NumberField(l, (float)v, float.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<double>((l, v) => NumberField(l, (double)v, double.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<decimal>((l, v) => NumberField(l, (decimal)v, decimal.TryParse, (i) => i.ToString(culture)));
        AddPropHandler<DateTime>((l, v) => NumberField(l, (DateTime)v, DateTime.TryParse, (i) => i.ToString(culture)));
        // Vectors
        AddPropHandler<Vector2>((l, v) => VectorField(l, (Vector2)v));
        AddPropHandler<Vector3>((l, v) => VectorField(l, (Vector3)v));
        AddPropHandler<Vector4>((l, v) => VectorField(l, (Vector4)v));
        AddPropHandler<Rect>((l, v) => RectField(l, (Rect)v));
        AddPropHandler<Bounds>((l, v) => BoundsField(l, (Bounds)v));
        AddPropHandler<Quaternion>((l, v) => QuaternionField(l, (Quaternion)v));
        AddPropHandler<Color>((l, v) => ColorField(l, (Color)v));
        AddPropHandler<Color32>((l, v) => (Color32)ColorField(l, (Color32)v));
        AddPropHandler<RectOffset>((l, v) => RectOffsetField(l, (RectOffset)v));
#if INT_STRUCTS
        AddPropHandler<BoundsInt>((l, v) => BoundsField(l, (BoundsInt)v));
        AddPropHandler<Vector2Int>((l, v) => VectorField(l, (Vector2Int)v));
        AddPropHandler<Vector3Int>((l, v) => VectorField(l, (Vector3Int)v));
#endif
        // Unsupported handlers
        AddPropHandler<IntPtr>((l, v) => v);
        AddPropHandler<UIntPtr>((l, v) => v);
        AddPropHandler<Type>((l, v) => v);
    }

    static void AddPropHandler<T>(Func<GUIContent, object, object> f)
    {
        _propertyFieldCache.Add(typeof(T), f);
    }

    public static object PropertyField(GUIContent label, object value, Type type)
    {
        Func<GUIContent, object, object> parser;
        if (type == null) throw new ArgumentNullException("type");
        else if (type.IsEnum) return EnumPopup(label, value, type);
        else if (_propertyFieldCache.TryGetValue(type, out parser)) return parser(label, value);
#if UNITY_EDITOR
        else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            if (Utility.isEditorWindow)
                return UnityEditor.EditorGUI.ObjectField(PrefixLabel(null, UnityEditor.EditorStyles.objectField, label), (UnityEngine.Object)value, type, true);
            else
                return value; // TODO
        }
#endif
        else if (value == null) return value; // ?
        else
        {
            // Enumerate children
            if (BeginFadeGroup(Foldout(label)))
            {
                BeginIndent();
                PropertyFields(value, type);
                EndIndent();
            }
            EndFadeGroup();
            return value;
        }
    }

    public static void PropertyFields(object value, Type type)
    {
        var enumer = ObjectEnumerator.GetObjectEnumerator(type);
        PropertyFields(value, enumer);
    }

    public static void PropertyFields(object value, ObjectEnumerator enumer)
    {
        foreach (var item in enumer.GetEnumerator(value))
        {
            BeginChangeCheck();
            var x = PropertyField(C(item.name), item.value, item.fieldType);
            if (EndChangeCheck())
            {
                var xx = item;
                xx.value = x;
            }
        }
    }
}
