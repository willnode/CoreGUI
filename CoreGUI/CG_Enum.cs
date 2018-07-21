using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{

    public static T EnumPopup<T>(GUIContent label, T value)
    {
        return (T)EnumPopup(label, value, typeof(T));
    }

    public static object EnumPopup(GUIContent label, object value, Type enumType)
    {
        var t = EnumOperator.GetData(enumType);
        return EnumPopup(label, value, t);
    }

    public static object EnumPopup(GUIContent label, object value, EnumerationData data)
    {
        var i = Array.IndexOf(data.values, value);
        var id = GUIUtility.GetControlID(FocusType.Keyboard);
        var r = PrefixLabel(Reserve(), label);
        switch (ev.type)
        {
            case EventType.MouseDown:
                if (r.Contains(ev.mousePosition))
                    data.ShowAsPopup(id);
                break;
            case EventType.Repaint:
                Styles.Popup.Draw(r, data.contents[i], id);
                break;
            case EventType.Layout:
            default:
                var val = Popup.GetValue(id);
                if (val != null)
                    return val;
                break;
        }
        return value;
    }


    public static T EnumGridToggles<T>(GUIContent label, T value, int colums = 3)
    {
        return EnumGridToggles(label, value, GUI.skin.toggle, colums);
    }

    public static T EnumGridToggles<T>(GUIContent label, T value, GUIStyle style, int colums = 3)
    {
        return (T)SelectionGrid(label, EnumOperator.GetData(typeof(T)), value, style, colums);
    }

    public static Enum EnumGridToggles(GUIContent label, Enum value, GUIStyle style, int colums = 3)
    {
        return (Enum)SelectionGrid(label, EnumOperator.GetData(value.GetType()), value, style, colums);
    }

    public static object SelectionGrid(GUIContent label, EnumerationData data, object value, GUIStyle style, int colums = 3)
    {
        var ds = data.contents;
        var dv = data.values;

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
                value = dv[i];
        }

        EndVertical();
        EndHorizontal();

        return value;
    }

    public static int MiniButtons(GUIContent label, GUIContent[] contents, int selected, int offset = 0)
    {
        BeginHorizontal(label);

        if (contents.Length > 0 && Toggle(contents[0], selected == offset + 0, contents.Length == 1 ? Styles.MiniButton : Styles.MiniButtonLeft))
            selected = 0 + offset;

        for (int i = 1; i < contents.Length - 1; i++)
        {
            if (Toggle(contents[i], selected == offset + i, Styles.MiniButtonMid))
                selected = i + offset;
        }

        if (contents.Length > 1 && Toggle(contents[contents.Length - 1], selected == offset + contents.Length - 1, Styles.MiniButtonRight))
            selected = contents.Length - 1 + offset;

        EndHorizontal();

        return selected;
    }

    public static class EnumOperator
    {
        static Dictionary<Type, EnumerationData> enumCaches = new Dictionary<Type, EnumerationData>();

        public static EnumerationData GetData(Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentException("Assigned enumType is not Enum!");

            EnumerationData data;
            if (enumCaches.TryGetValue(enumType, out data))
                return data;

            data = new EnumerationData(enumType);
            enumCaches.Add(enumType, data);
            return data;
        }

        public static void ClearData() { enumCaches.Clear(); }
    }

    public class EnumerationData
    {
        public readonly object[] values;
        public readonly GUIContent[] contents;
        public readonly string[] names;
        public readonly Type type;
        public readonly bool flags;

        public EnumerationData(Type type)
        {
            this.type = type;
            values = Enum.GetValues(type).Cast<object>().ToArray();
            names = Enum.GetNames(type);
            contents = names.Select(x => new GUIContent(x)).ToArray();
            flags = type.GetCustomAttributes(typeof(FlagsAttribute), true).Any();
        }

        public EnumerationData(string[] names, bool flags)
        {
            this.type = null;
            this.contents = names.Select(x => new GUIContent(x)).ToArray();
            this.values = Enumerable.Range(0, names.Length).Cast<object>().ToArray();
            this.flags = flags;
            this.names = names;
        }

        public EnumerationData(string[] names, object[] values, bool flags)
        {
            this.type = null;
            this.values = values;
            this.contents = names.Select(x => new GUIContent(x)).ToArray();
            this.flags = flags;
            this.names = names;
        }

        public EnumerationData(GUIContent[] contents, object[] values, bool flags)
        {
            this.type = null;
            this.values = values;
            this.contents = contents;
            this.flags = flags;
            this.names = contents.Select(x => x.text).ToArray();
        }

        public List<PopupItem> popupCache;

        public void ShowAsPopup(int id)
        {
            if (popupCache == null)
            {
                popupCache = new List<PopupItem>();
                for (int i = 0; i < values.Length; i++)
                {
                    popupCache.Add(new PopupItem() { content = contents[i], value = values[i] });
                }
            }
            Popup.Show(popupCache, id);
        }

    }

    public class ObjectEnumerator
    {
        public static Dictionary<Type, ObjectEnumerator> _cached = new Dictionary<Type, ObjectEnumerator>();

        public readonly FieldInfo[] fields;
        public readonly string[] names;
        public readonly Type type;

        public ObjectEnumerator(Type type) : this(type, null, null)
        {

        }

        static bool isFieldOK(FieldInfo f)
        {
            if (!f.IsInitOnly) return false;
            if (Attribute.GetCustomAttribute(f, typeof(NonSerializedAttribute)) != null) return false;
            if (f.IsPublic || Attribute.GetCustomAttribute(f, typeof(SerializeField)) != null) return true;
            return false;
        }

        static string nicifyVarName(string name)
        {
            if (name.StartsWith("m_"))
                name = name.Substring(2);

            return string.Concat(name.Select(x => char.IsUpper(x) ? " " + x : new string(x, 1))).TrimStart(' ');
        }

        public ObjectEnumerator(Type type, string[] inclusionNames, string[] exclusionNames)
        {
            this.type = type;

            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            var f = type.GetFields(flags).Where(x => isFieldOK(x));

            if (inclusionNames != null)
                f = f.Where(x => inclusionNames.Contains(x.Name));
            if (exclusionNames != null)
                f = f.Where(x => !exclusionNames.Contains(x.Name));

            fields = f.ToArray();
            names = f.Select(x => nicifyVarName(x.Name)).ToArray();
        }

        public struct Enumerator : IEnumerable<EnumeratedField>
        {
            public readonly FieldInfo[] fields;
            public readonly string[] names;

            public readonly object data;

            public Enumerator(FieldInfo[] fields, string[] names, object data)
            {
                this.fields = fields;
                this.data = data;
                this.names  = names;
            }

            public IEnumerator<EnumeratedField> GetEnumerator()
            {
                int i = 0;
                foreach (var field in fields)
                {
                    yield return new EnumeratedField(field, data, names[i++]);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                int i = 0;
                foreach (var field in fields)
                {
                    yield return new EnumeratedField(field, data, names[i++]);
                }
            }
        }

        public Enumerator GetEnumerator(object data)
        {
            return new Enumerator(fields, names, data);
        }

        public static ObjectEnumerator GetObjectEnumerator(Type type)
        {
            ObjectEnumerator obj;
            if (_cached.TryGetValue(type, out obj))
                return obj;

            // Create new
            obj = new ObjectEnumerator(type);
            _cached[type] = obj;
            return obj;
        }
    }

    public struct EnumeratedField
    {
        public readonly FieldInfo field;

        public readonly object data;

        public readonly string name;

        public EnumeratedField(FieldInfo field, object data, string name)
        {
            this.field = field;
            this.data = data;
            this.name = name;
        }

        public object value
        {
            get { return field.GetValue(data); }
            set { field.SetValue(data, value); }
        }

        public Type fieldType { get { return field.FieldType; } }
    }
}
