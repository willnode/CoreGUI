using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static partial class CoreGUI
{
    public class Popup
    {
        public readonly int id = Time.time.GetHashCode();

        public List<PopupItem> popups = new List<PopupItem>();

        public Vector2 GetSize()
        {
            var g = Vector2.zero;
            var style = GUI.skin.button;

            foreach (var item in popups)
            {
                var v = style.CalcSize(item.content);
                g.y += v.y + style.margin.top;
                g.x = Mathf.Max(g.x, v.x);
            }

            g.x += style.margin.horizontal;

            return g;
        }

        public bool shown { get; private set; }

        bool isJustShown = true;

        public Rect position;

        public object OnGUI()
        {
            if (!shown)
                return null;

            object obj = null;

            var id = GUIUtility.GetControlID(FocusType.Keyboard);

            if (isJustShown && id >= 0)
            {
                GUIUtility.keyboardControl = id;
                isJustShown = false;
            }
            else if (id >= 0 && GUIUtility.keyboardControl != id)
            {
                shown = false;
                return null;
            }
            
            BeginArea(position, GUI.skin.box);
            using (Scoped.Indent(IndentPolicy.None))
            {
                for (int i = 0; i < popups.Count; i++)
                {
                    if (popups[i] == null) continue;
                    if (Button(popups[i].content))
                    {
                        shown = false;
                        obj = popups[i].value;
                    }
                }
            }
            EndArea();
            return obj;
        }

        public void Show()
        {
            Show(new Rect((Event.current.mousePosition), Vector2.zero));
        }

        public void Show(Rect position)
        {
            shown = isJustShown = true;
            position.y += position.height;
            position.size = Vector2.Max(position.size, GetSize());
            var top = LayoutUtility.topLevel;
            if (top is ScrollGroup)
            {
                var tops = (ScrollGroup)top;
                position.position = Vector2.Min(position.position, new Vector2(tops.clientWidth, tops.clientHeight) + tops.rect.position - position.size);
            } else
               position.position = Vector2.Min(position.position, LayoutUtility.topLevel.rect.max - position.size);
            this.position = position;
        }
    }

    public class PopupItem
    {
        public GUIContent content;
        public object value;
        public bool enabled = true;
    }
}

