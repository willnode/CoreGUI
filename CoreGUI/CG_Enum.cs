using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    public static T EnumGridField<T>(GUIContent label, T value, int row = 1, bool flags = false)
    {
        var t = EnumOperator.GetData(typeof(T));
        var ds = t.contents;
        var dv = t.values;

        var col = ds.Length / row;
        Rect r = new Rect();

        for (int i = 0; i < ds.Length; i++)
        {
            int x = i % col, y = i / col;
            if (x == 0)
            {
                r = PrefixLabel(Reserve(), y == 0 ? label : GUIContent.none);
                r.width /= col;
            }
            else
                r.x += r.width;

            if (GUI.Toggle(r, dv[i].Equals(value), ds[i], GUI.skin.button))
                value = (T)dv[i];
        }

        return value;
    }

    public static class EnumOperator
    {
        public class EnumData
        {
            public readonly object[] values;
            public readonly GUIContent[] contents;
            public readonly string[] names;
            public readonly Type type;
            public readonly bool flags;

            public EnumData(Type type)
            {
                this.type = type;
                values = Enum.GetValues(type).Cast<object>().ToArray();
                names = Enum.GetNames(type);
                contents = names.Select(x => new GUIContent(x)).ToArray();
                flags = type.GetCustomAttributes(typeof(FlagsAttribute), true).Any();
            }
            
            public EnumData(object[] values, GUIContent[] contents, bool flags)
            {
                this.type = null;
                this.values = values;
                this.contents = contents;
                this.flags = flags;
                this.names = contents.Select(x => x.text).ToArray();
            }
        }

        static Dictionary<Type, EnumData> enumCaches = new Dictionary<Type, EnumData>();

        public static EnumData GetData(Type enumType)
        {
            EnumData data;
            if (enumCaches.TryGetValue(enumType, out data))
                return data;

            data = new EnumData(enumType);
            enumCaches.Add(enumType, data);
            return data;
        }

        public static void ClearData() { enumCaches.Clear(); }
    }
}
