using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{
    public abstract class PopupBase
    {
        public static Dictionary<int, PopupBase> shownPopup = new Dictionary<int, PopupBase>();

        public virtual bool IsDraggable { get { return false; } }

        public virtual GUIContent TitleContent { get { return GUIContent.none; } }

        public virtual bool IsModal { get { return false; } }

        public Action<object> action;

        public int callerGUIID;
        
        public Rect position;
        
        public abstract Vector2 GetSize();

        bool isJustShown = true;

        public PopupBase(Action<object> action)
        {
            this.action = action;
            callerGUIID = Utility.currentGUIID;
        }

        public void OnGUI()
        {
            var id = GUIUtility.GetControlID(FocusType.Keyboard);

            if (isJustShown)
            {
                GUI.FocusWindow(id);
                GUI.BringWindowToFront(id);
                isJustShown = false;
            }

            var pos = GUI.ModalWindow(id, position, (idd) =>
            {
                var local = position;
                local.position = Vector2.zero;
                BeginGUI(idd, local);

                OnWindowGUI(idd);

                EndGUI();

                // Ugly workaround. But works.
                if (ev.type == EventType.Ignore && !IsModal && !local.Contains(ev.mousePosition))
                {
                    Close(); // Force close
                    ev.Use();
                }

            }, TitleContent);

            if (IsDraggable)
                position = pos;
        }

        public abstract void OnWindowGUI(int id);

        public void Close()
        {
            shownPopup.Remove(callerGUIID);
        }

        public void Apply(object item)
        {
            action(item);
        }

        public static void Show(PopupBase popup)
        {
            popup.isJustShown = true;
            shownPopup.Add(popup.callerGUIID, popup);
        }

        protected void SetSafePosition(Rect pos)
        {
            position = pos;
            position.y += position.height; // If position == widget rect, show under it.
            position.size = Vector2.Max(position.size, GetSize());
            position.position = Vector2.Max(Vector2.zero, Vector2.Min(position.position, new Vector2(Screen.width, Screen.height) - position.size));
        }



        // ---

        public static Dictionary<int, object> _pendingObjects = new Dictionary<int, object>();

        public static Action<object> MakeCallback(int id)
        {
            var iid = Utility.currentGUIID;
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
                Utility.SendEvent(e, iid);
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

    public class MenuPopup : PopupBase
    {
        public List<MenuPopupItem> popups = new List<MenuPopupItem>();

        public override Vector2 GetSize()
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

        public override void OnWindowGUI(int id)
        {
            for (int i = 0; i < popups.Count; i++)
            {
                if (Button(popups[i].content, Styles.MiniButton))
                {
                    Apply(popups[i].value);
                    Close();
                }
            }
        }

        public static void Show(int id, List<MenuPopupItem> items)
        {
            Show(MakeCallback(id), items);
        }

        public static void Show(Action<object> action, List<MenuPopupItem> items)
        {
            Utility.delayCall += delegate
            {
                Show(new Rect((Event.current.mousePosition), Vector2.zero), action, items);
            };
        }

        public static void Show(Rect position, Action<object> action, List<MenuPopupItem> items)
        {
#if UNITY_EDITOR
            if (Utility.IsOnEditorWindow())
            {
                // Fallback because this built-in popup is literally broken in editor mode (known issue?)
                UnityEditor.EditorUtility.DisplayCustomMenu(position, items.Select(x => x.content).ToArray(), -1,
                    (userData, options, selected) => action(items[selected].value),
                    null);
                return;
            }
#endif
            Show(new MenuPopup(position, action, items));
        }

        public MenuPopup(Rect position, Action<object> action, List<MenuPopupItem> popups) : base(action)
        {
            this.popups = popups;
            SetSafePosition(position);
        }

    }

    public struct MenuPopupItem
    {
        public GUIContent content;
        public object value;
        public bool disabled;
    }

    public class ColorDialogPopup : PopupBase
    {
        public override GUIContent TitleContent { get { return C("Color Dialog"); } }

        public override bool IsDraggable { get { return true; } }

        public Color color;

        public bool alpha;

        public bool hsl;

        public override Vector2 GetSize()
        {
            return new Vector2(400, 300);
        }

        static GUIContent[] hsvOps = new GUIContent[] { new GUIContent("RGB"), new GUIContent("HSV") };

        public override void OnWindowGUI(int id)
        {
            using (Scoped.Indent())
            using (Scoped.LabelOption(16, Side.Left))
            {
                {
                    var r = Reserve(3f);
                    if (ev.type == EventType.Repaint)
                    {
                        var tint = GUI.color;
                        GUI.color = new Color(color.r, color.g, color.b);
                        GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill);
                        GUI.color = tint;
                    }
                }
                using (Scoped.Horizontal())
                {
                    hsl = MiniButtons(null, hsvOps, hsl ? 1 : 0) == 1;
                    Toggle(C("Normalized"), true, Styles.MiniButton);
                }
                
                if (!hsl)
                {
                    color.r = FloatSlider(C("R"), color.r, 0, 1);
                    color.g = FloatSlider(C("G"), color.g, 0, 1);
                    color.b = FloatSlider(C("B"), color.b, 0, 1);
                }
                else
                {
                    float h, s, v;
                    Color.RGBToHSV(color, out h, out s, out v);
                    BeginChangeCheck();
                    h = FloatSlider(C("H"), h, 0, 1);
                    s = FloatSlider(C("S"), s, 0, 1);
                    v = FloatSlider(C("V"), v, 0, 1);
                    if (EndChangeCheck())
                        color = Color.HSVToRGB(h, s, v);
                }
                if (alpha)
                    color.a = FloatSlider(C("A"), color.a, 0, 1);
                if (Button(C("OK")))
                {
                    Apply(color);
                    Close();
                }
            }
        }
        
        public ColorDialogPopup(Rect position, Action<object> action, Color color, bool alpha = true) : base(action)
        {
            this.color = color;
            this.alpha = alpha;
            if (!alpha)
                color.a = 1;
            SetSafePosition(position);
        }

        public static void Show(int id, Color color, bool alpha = true)
        {
            Show(MakeCallback(id), color, alpha);
        }

        public static void Show(Action<object> action, Color color, bool alpha = true)
        {
            Utility.delayCall += delegate
            {
                Show(new Rect((Event.current.mousePosition), Vector2.zero), action, color, alpha);
            };
        }

        public static void Show(Rect position, Action<object> action, Color color, bool alpha = true)
        {
            Show(new ColorDialogPopup(position, action, color, alpha));
        }

    }
}

