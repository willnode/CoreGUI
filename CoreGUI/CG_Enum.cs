using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{
    
    public static T EnumField<T>(GUIContent label, T value)
    {
        var t = EnumOperator.GetData(typeof(T));
        var p = t.GeneratePopup();
        var i = Array.IndexOf(t.values, value);
        if (Button(i < 0 ? GUIContent.none : t.contents[i], label))
            p.Show();
        var v = p.OnGUI();
        if (v != null)
            return (T)v;

        return value;
    }

    public static T EnumGridToggles<T>(GUIContent label, T value, int colums = 3, bool flags = false)
    {
        return EnumGridToggles(label, value, GUI.skin.toggle, colums, flags);
    }

    public static T EnumGridToggles<T>(GUIContent label, T value, GUIStyle style, int colums = 3, bool flags = false)
    {
        var t = EnumOperator.GetData(typeof(T));
        var ds = t.contents;
        var dv = t.values;
        
        BeginHorizontal(label);
        BeginVertical();

        for (int i = 0; i < ds.Length; i++)
        {
            if (i % colums == 0 && i > 0)
            {   
                EndVertical();
                BeginVertical();
            }

            if (Toggle(ds[i], dv[i].Equals(value)))
                value = (T)dv[i];
        }

        EndVertical();
        EndHorizontal();

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

            public EnumData(string[] names, bool flags)
            {
                this.type = null;
                this.contents = names.Select(x => new GUIContent(x)).ToArray();
                this.values = Enumerable.Range(0, names.Length).Cast<object>().ToArray();
                this.flags = flags;
                this.names = names;
            }

            public EnumData(string[] names, object[] values, bool flags)
            {
                this.type = null;
                this.values = values;
                this.contents = names.Select(x => new GUIContent(x)).ToArray();
                this.flags = flags;
                this.names = names;
            }

            public EnumData(GUIContent[] contents, object[] values, bool flags)
            {
                this.type = null;
                this.values = values;
                this.contents = contents;
                this.flags = flags;
                this.names = contents.Select(x => x.text).ToArray();
            }

            public Popup popup;

            public Popup GeneratePopup()
            {
                if (popup == null)
                {
                    popup = new Popup();
                    for (int i = 0; i < values.Length; i++)
                    {
                        popup.popups.Add(new PopupItem() { content = contents[i], value = values[i] });
                    }
                }
                return popup;
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
