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

        public List<PopupItem> popups;

        public bool MenuPopup()
        {
            for (int i = 0; i < popups.Count; i++)
            {
                if (popups[i] == null) continue;
                if (Button(popups[i].content))
                {
                    popups[i].click();
                    return true;
                }
            }
            return false;
        }

        public float GetHeight()
        {
            return popups.Count * 16;
        }

        public bool Show(Rect position)
        {
            position.height = GetHeight();
            return MenuPopup();
        }
    }

    public class PopupItem
    {
        public GUIContent content;
        public Action click;
        public Action<bool> hover;
        public bool enabled;
    }
}

