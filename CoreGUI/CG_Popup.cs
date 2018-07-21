using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{
    public class Popup
    {
        public static Popup shownPopup;

        public Action<object> action;

        public List<PopupItem> popups = new List<PopupItem>();

        static Vector2 GetSize(List<PopupItem> popups)
        {
            var g = Vector2.zero;
            var style = Styles.MiniButton;

            foreach (var item in popups)
            {
                var v = style.CalcSize(item.content);
                g.y += v.y + style.margin.top;
                g.x = Mathf.Max(g.x, v.x);
            }

            g.x += style.margin.horizontal;
            return g;
        }

        bool isJustShown = true;

        public Rect position;

        public void OnGUI()
        {
            var id = GUIUtility.GetControlID(FocusType.Keyboard);

            if (isJustShown)
            {
                GUIUtility.keyboardControl = id;
                isJustShown = false;
            }
            else if (GUIUtility.keyboardControl != id)
            {
                shownPopup = null;
                return;
            }

            BeginArea(position, GUI.skin.box);
            using (Scoped.Indent(IndentPolicy.None))
            {
                for (int i = 0; i < popups.Count; i++)
                {
                    if (Button(popups[i].content, Styles.MiniButton))
                    {
                        action(popups[i].value);
                        shownPopup = null;
                    }
                }
            }
            EndArea();
        }

        public static void Show(List<PopupItem> items, int id)
        {
            Show(items, MakeCallback(id));
        }

        public static void Show(List<PopupItem> items, Action<object> action)
        {
            Utility.delayCall += delegate
            {
                Show(new Rect((Event.current.mousePosition), Vector2.zero), items, action);
            };
        }

        public static void Show(Rect position, List<PopupItem> items, Action<object> action)
        {
#if UNITY_EDITOR
            if (Utility.IsOnEditorWindow())
            {
                // Fallback because this built-in popup is literally broken in editor mode (known issue)
                UnityEditor.EditorUtility.DisplayCustomMenu(position, items.Select(x => x.content).ToArray(), -1,
                    (userData, options, selected) => action(items[selected].value),
                    null);
                return;
            }
#endif
            position.y += position.height; // If position == widget rect, show under it.
            position.size = Vector2.Max(position.size, GetSize(items));
            position.position = Vector2.Min(position.position, new Vector2(Screen.width, Screen.height) - position.size);
            shownPopup = new Popup()
            {
                position = position,
                popups = items,
                action = action,
            };
        }

        // ---

        public static Dictionary<int, object> _pendingObjects = new Dictionary<int, object>();

        public static Action<object> MakeCallback(int id)
        {
            return delegate (object o)
            {
                _pendingObjects[id] = o;
                Event e;
#if UNITY_EDITOR
                if (Utility.IsOnEditorWindow())
                {
                    e = UnityEditor.EditorGUIUtility.CommandEvent("PopupSet");
                }
                else
#endif
                {
                    e = new Event(ev);
                    e.type = EventType.ExecuteCommand;
                    e.commandName = "PopupSet";
                }
                Utility.SendEvent(e);
            };
        }

        public static object GetValue(int id)
        {
            // Because Layout must be sync with other events...
            if (ev.type == EventType.ExecuteCommand && ev.commandName == "PopupSet")
            {
                object o;
                if (_pendingObjects.TryGetValue(id, out o))
                {
                    _pendingObjects.Remove(id);
                    return o;
                }
            }
            return null;
        }
    }

    public struct PopupItem
    {
        public GUIContent content;
        public object value;
        public bool disabled;
    }
}

