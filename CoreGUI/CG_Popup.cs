﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{
    public abstract class PopupBase
    {
        public static Dictionary<int, PopupBase> shownPopup = new Dictionary<int, PopupBase>();

        public virtual GUIContent PopupContent { get { return GUIContent.none; } }

        public virtual GUIStyle PopupStyle { get { return Styles.Window; } }

        public virtual bool IsPersistent { get { return false; } }

        public Action<object> action;

        public int callerGUIID;

        public Rect position;

        public abstract Vector2 GetSize();

        float callerScale = 1;

        GUISkin callerSkin = null;

        public PopupBase(Action<object> action)
        {
            this.action = action;
            callerGUIID = Utility.currentGUIID;
            callerScale = _currentScale;
            callerSkin = GUI.skin;
        }

        public void OnGUI()
        {
            var id = GUIUtility.GetControlID(FocusType.Keyboard);

            position = GUI.ModalWindow(id, position, (idd) =>
            {
                var local = position;
                local.position = Vector2.zero;
                local.size *= callerScale;

                BeginGUI(idd, local, callerSkin, callerScale);

                OnWindowGUI(idd);

                // Quit when mouse clicks outside window.
                // Ugly workaround. But works.
                if (!IsPersistent && ((ev.type == EventType.Ignore && !local.Contains(ev.mousePosition))
                || (ev.type == EventType.KeyUp && ev.keyCode == KeyCode.Escape)))
                {
                    Close(); // Force close
                    ev.Use();
                }

                EndGUI();

            }, PopupContent, PopupStyle);
        }

        public abstract void OnWindowGUI(int id);

        public void Close()
        {
            shownPopup.Remove(callerGUIID);
        }

        public void Apply(object item, bool autoClose = true)
        {
            action(item);
            if (autoClose)
                Close();
        }

        public static void Show(PopupBase popup)
        {
            if (shownPopup.ContainsKey(popup.callerGUIID))
            {
                var k = shownPopup[popup.callerGUIID];
                // Just in case
                k.callerGUIID = popup.callerGUIID;
                k.Close();
            }

            shownPopup.Add(popup.callerGUIID, popup);
        }

        protected void SetSafePosition(Rect pos)
        {
            position = pos;
            position.y += position.height; // If position == widget rect, show under it.
            position.size = Vector2.Max(position.size, GetSize());
            position.position = Vector2.Max(Vector2.zero, Vector2.Min(position.position, new Vector2(Screen.width, Screen.height) - position.size));
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
            Show(PopupCallback.MakeCallback(id), items);
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
            if (Utility.isEditorWindow)
            {
                // Fallback because this built-in popup is literally worse in editor mode
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

    public static class PopupCallback
    {
        public static Dictionary<int, object> _pendingObjects = new Dictionary<int, object>();

        const string kCommandName = "PopupCallback";

        public static Action<object> MakeCallback(int id)
        {
            if (id == 0)
            {
                // This means it's just discarded
                return (o) => { };
            }

            var iid = Utility.currentGUIID;
            return delegate (object o)
            {
                _pendingObjects[id] = o;
                Event e;
#if UNITY_EDITOR
                if (Utility.isEditorWindow)
                {
                    e = UnityEditor.EditorGUIUtility.CommandEvent(kCommandName);
                }
                else
#endif
                {
                    e = new Event(ev);
                    e.type = EventType.ExecuteCommand;
                    e.commandName = kCommandName;
                }
                Utility.SendEvent(e, iid);
            };
        }

        public static object GetValue(int id)
        {
            if (id == 0)
                Debug.LogWarning("You're trying to request an object with null ID");

            // Because Layout must be sync with other events...
            if (ev.type == EventType.ExecuteCommand && ev.commandName == kCommandName)
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

    public struct MenuPopupItem
    {
        public GUIContent content;
        public object value;
        public bool disabled;
    }

    public class ColorDialogPopup : PopupBase
    {
        public override GUIContent PopupContent { get { return C("Color Dialog"); } }

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
                    GUI.DragWindow(r);
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
            Show(PopupCallback.MakeCallback(id), color, alpha);
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

    public class MessagePopup : PopupBase
    {
        public override Vector2 GetSize()
        {
            return new Vector2(500, 200);
        }

        public override bool IsPersistent { get { return true; } }

        public override void OnWindowGUI(int id)
        {
            Label(message);

            GUI.DragWindow(LayoutUtility.GetLastRect());

            FlexibleSpace();
            FlexibleSpace();
            FlexibleSpace();

            using (Scoped.Horizontal())
            {
                FlexibleSpace();
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (Button(buttons[i]))
                    {
                        Apply(i);
                    }
                }
            }

            FlexibleSpace();

        }

        public override GUIContent PopupContent { get { return title; } }

        public GUIContent title;

        public GUIContent message;

        public GUIContent[] buttons;

        public MessagePopup(Action<object> action, GUIContent title, GUIContent message, GUIContent[] buttons) : base(action)
        {
            this.title = title;
            this.message = message;
            this.buttons = buttons;
            var r = new Rect(Vector2.zero, GetSize());
            r.center = Utility.screenRect.center;
            this.SetSafePosition(r);
        }

        public enum ButtonScheme { OK, YesNo, YesNoCancel, ContinueRetryAbort }

        static Dictionary<ButtonScheme, GUIContent[]> schemes = new Dictionary<ButtonScheme, GUIContent[]>()
        {
            { ButtonScheme.OK, Utility.ToGUIContents("OK") },
            { ButtonScheme.YesNo, Utility.ToGUIContents("Yes", "No") },
            { ButtonScheme.YesNoCancel, Utility.ToGUIContents("Yes", "No", "Cancel") },
            { ButtonScheme.ContinueRetryAbort, Utility.ToGUIContents("Continue", "Retry", "Abort") },
        };


        public static void Show(string title, string message)
        {
            Show(0, title, message);
        }

        public static void Show(int id, string title, string message, ButtonScheme scheme = ButtonScheme.OK)
        {
            Show(PopupCallback.MakeCallback(id), new GUIContent(title), new GUIContent(message), schemes[scheme]);
        }

        public static void Show(int id, string title, string message, params string[] buttons)
        {
            Show(PopupCallback.MakeCallback(id), new GUIContent(title), new GUIContent(message), Utility.ToGUIContents(buttons));            
        }
        
        public static void Show(int id, GUIContent title, GUIContent message, GUIContent[] buttons)
        {
            Show(PopupCallback.MakeCallback(id), title, message, buttons);
        }

        public static void Show(Action<object> action, GUIContent title, GUIContent message, GUIContent[] buttons)
        {
            Show(new MessagePopup(action, title, message, buttons));
        }
    }
}

