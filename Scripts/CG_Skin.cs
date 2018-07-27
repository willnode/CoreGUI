using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class CoreGUI
{
    public class Styles
    {
        public readonly GUISkin skin;

        public Styles(GUISkin skin)
        {
            this.skin = skin;
            Invalidate();
        }

        // Should these variables be public readonly? :/

#pragma warning disable 0414 // unused warning

        private GUIStyle m_Box, m_Label, m_TextField, m_TextArea, m_Button, m_Toggle, m_Window, m_ScrollView;

        private GUIStyle m_HorizontalSlider, m_HorizontalSliderThumb, m_VerticalSlider, m_VerticalSliderThumb, m_MinMaxHorizontalSliderThumb;

        private GUIStyle m_HorizontalScrollbar, m_HorizontalScrollbarThumb, m_HorizontalScrollbarLeftButton, m_HorizontalScrollbarRightButton;

        private GUIStyle m_VerticalScrollbar, m_VerticalScrollbarThumb, m_VerticalScrollbarUpButton, m_VerticalScrollbarDownButton;

        private GUIStyle m_HelpBox, m_Foldout, m_RadioButton, m_Tooltip, m_NumberField, m_ColorField, m_DropDownList, m_Popup, m_ToggleMixed;

        private GUIStyle m_Prefix, m_MiniLabel, m_LargeLabel, m_BoldLabel, m_MiniBoldLabel, m_WordWrappedLabel, m_LinkLabel;

        private GUIStyle m_MiniButton, m_MiniButtonLeft, m_MiniButtonMid, m_MiniButtonRight, m_NotificationText, m_NotificationBackground;

        private GUIStyle m_ProgressBarBack, m_ProgressBarBar, m_ProgressBarText, m_SearchField, m_SearchFieldCancelButton, m_MenuWindow, m_MenuItem;

#pragma warning restore 0414

        public GUIStyle GetStyle(string name)
        {
            return skin.FindStyle(name);
        }

        private Dictionary<string, GUIStyleVariants> m_StyleVariants = new Dictionary<string, GUIStyleVariants>();

        public GUIStyleVariants GetStyleVariants(GUIStyle normalStyle)
        {
            GUIStyleVariants sv;
            if (m_StyleVariants.TryGetValue(normalStyle.name, out sv))
                return sv;
            else
            {
                // Create
                m_StyleVariants[normalStyle.name] = sv = new GUIStyleVariants(skin, normalStyle);
            }
            return sv;
        }

        GUIStyle DeriveStyle(GUIStyle sample, Action<GUIStyle> steps)
        {
            var style = new GUIStyle(sample);
            steps(style);
            return style;
        }

        public void Invalidate()
        {
            m_Box = skin.box;
            m_Label = skin.label;
            m_TextField = skin.textField;
            m_TextArea = skin.textArea;
            m_Button = skin.button;
            m_Toggle = skin.toggle;
            m_Window = skin.window;
            m_ScrollView = skin.scrollView;

            m_HorizontalSlider = skin.horizontalSlider;
            m_HorizontalSliderThumb = skin.horizontalSliderThumb;
            m_VerticalSlider = skin.verticalSlider;
            m_VerticalSliderThumb = skin.verticalSliderThumb;

            m_HorizontalScrollbar = skin.horizontalScrollbar;
            m_HorizontalScrollbarThumb = skin.horizontalScrollbarThumb;
            m_HorizontalScrollbarLeftButton = skin.horizontalScrollbarLeftButton;
            m_HorizontalScrollbarRightButton = skin.horizontalScrollbarRightButton;

            m_VerticalScrollbar = skin.verticalScrollbar;
            m_VerticalScrollbarThumb = skin.verticalScrollbarThumb;
            m_VerticalScrollbarUpButton = skin.verticalScrollbarUpButton;
            m_VerticalScrollbarDownButton = skin.verticalScrollbarDownButton;

            // As far I'm aware these GetStyle is not case-sensitive.

            m_MiniLabel = GetStyle("miniLabel") ?? m_Label;
            m_LargeLabel = GetStyle("LargeLabel") ?? m_Label;
            m_BoldLabel = GetStyle("BoldLabel") ?? m_Label;
            m_MiniBoldLabel = GetStyle("MiniBoldLabel") ?? m_Label;
            m_WordWrappedLabel = GetStyle("WordWrappedLabel") ?? m_Label;

            m_RadioButton = GetStyle("Radio") ?? m_Toggle;
            m_MiniButton = GetStyle("miniButton") ?? m_Button;
            m_MiniButtonLeft = GetStyle("miniButtonLeft") ?? m_MiniButton;
            m_MiniButtonMid = GetStyle("miniButtonMid") ?? m_MiniButton;
            m_MiniButtonRight = GetStyle("miniButtonRight") ?? m_MiniButton;

            m_SearchField = GetStyle("SearchTextField");
            m_SearchFieldCancelButton = GetStyle("SearchCancelButton");
            m_HelpBox = GetStyle("HelpBox") ?? m_Box;
            m_MinMaxHorizontalSliderThumb = GetStyle("MinMaxHorizontalSliderThumb") ?? m_HorizontalSliderThumb;
            m_DropDownList = GetStyle("DropDownButton") ?? m_Button;
            m_ProgressBarBack = GetStyle("ProgressBarBack") ?? m_Box;
            m_ProgressBarBar = GetStyle("ProgressBarBar") ?? m_Box;
            m_ProgressBarText = GetStyle("ProgressBarText") ?? m_Label;
            m_Tooltip = GetStyle("Tooltip") ?? m_Box;
            m_NotificationText = GetStyle("NotificationText") ?? m_Label;
            m_NotificationBackground = GetStyle("NotificationBackground") ?? m_Box;
            m_Popup = GetStyle("MiniPopup") ?? GetStyle("Popup") ?? m_DropDownList;
            m_NumberField = GetStyle("textField") ?? m_TextField;
            m_ToggleMixed = GetStyle("ToggleMixed") ?? m_Toggle;
            m_ColorField = GetStyle("ColorField") ?? m_Box;
            m_Foldout = GetStyle("Foldout") ?? m_Toggle;
            m_Prefix = GetStyle("Prefix") ?? DeriveStyle(m_Label, (s) =>
            {
                s.padding = new RectOffset(4, 0, 3, 3);
                s.alignment = TextAnchor.MiddleLeft;
            });
            m_LinkLabel = GetStyle("LinkLabel") ?? DeriveStyle(m_Label, (s) =>
            {
                s.normal.textColor = new Color(0.25f, 0.5f, 0.9f, 1f);
                s.stretchWidth = false;
            });
            m_MenuWindow = GetStyle("MenuWindow") ?? m_Window;
            m_MenuItem = GetStyle("MenuItem") ?? m_MiniButton;
        }

        private static Dictionary<int, Styles> s_Skins = new Dictionary<int, Styles>();

        private static int s_FastCurSkinID;

        private static Styles s_FastCurSkin;

        public static Styles CurrentSkin
        {
            get
            {
                var id = GUI.skin.GetInstanceID();
                if (id == s_FastCurSkinID)
                    return s_FastCurSkin;

                Styles skin;
                if (!s_Skins.TryGetValue(id, out skin))
                    skin = new Styles(GUI.skin);
                s_FastCurSkinID = id;
                return s_FastCurSkin = skin;
            }
        }

        public static void InvalidateSkins()
        {
            s_FastCurSkinID = 0;
            s_FastCurSkin = null;
            s_Skins.Clear();
        }

        // I commented out some unused GUIStyle because waiting for the widget to be implemented.

        public static GUIStyle Box { get { return CurrentSkin.m_Box; } }

        public static GUIStyle Label { get { return CurrentSkin.m_Label; } }

        public static GUIStyle TextField { get { return CurrentSkin.m_TextField; } }

        public static GUIStyle TextArea { get { return CurrentSkin.m_TextArea; } }

        public static GUIStyle Button { get { return CurrentSkin.m_Button; } }

        public static GUIStyle Toggle { get { return CurrentSkin.m_Toggle; } }

        public static GUIStyle Window { get { return CurrentSkin.m_Window; } }

        public static GUIStyle ScrollView { get { return CurrentSkin.m_ScrollView; } }

        public static GUIStyle HorizontalSlider { get { return CurrentSkin.m_HorizontalSlider; } }

        public static GUIStyle HorizontalSliderThumb { get { return CurrentSkin.m_HorizontalSliderThumb; } }

        public static GUIStyle VerticalSlider { get { return CurrentSkin.m_VerticalSlider; } }

        public static GUIStyle VerticalSliderThumb { get { return CurrentSkin.m_VerticalSliderThumb; } }

        //public static GUIStyle MinMaxHorizontalSliderThumb { get { return CurrentSkin.m_MinMaxHorizontalSliderThumb; } }

        public static GUIStyle HorizontalScrollbar { get { return CurrentSkin.m_HorizontalScrollbar; } }

        //public static GUIStyle HorizontalScrollbarThumb { get { return CurrentSkin.m_HorizontalScrollbarThumb; } }

        //public static GUIStyle HorizontalScrollbarLeftButton { get { return CurrentSkin.m_HorizontalScrollbarLeftButton; } }

        //public static GUIStyle HorizontalScrollbarRightButton { get { return CurrentSkin.m_HorizontalScrollbarRightButton; } }

        public static GUIStyle VerticalScrollbar { get { return CurrentSkin.m_VerticalScrollbar; } }

        //public static GUIStyle VerticalScrollbarThumb { get { return CurrentSkin.m_VerticalScrollbarThumb; } }

        //public static GUIStyle VerticalScrollbarUpButton { get { return CurrentSkin.m_VerticalScrollbarUpButton; } }

        //public static GUIStyle VerticalScrollbarDownButton { get { return CurrentSkin.m_VerticalScrollbarDownButton; } }

        //public static GUIStyle HelpBox { get { return CurrentSkin.m_HelpBox; } }

        public static GUIStyle Foldout { get { return CurrentSkin.m_Foldout; } }

        public static GUIStyle RadioButton { get { return CurrentSkin.m_RadioButton; } }

        public static GUIStyle Tooltip { get { return CurrentSkin.m_Tooltip; } }

        public static GUIStyle NumberField { get { return CurrentSkin.m_NumberField; } }

        //public static GUIStyle ColorField { get { return CurrentSkin.m_ColorField; } }

        //public static GUIStyle DropDownList { get { return CurrentSkin.m_DropDownList; } }

        public static GUIStyle Popup { get { return CurrentSkin.m_Popup; } }

        //public static GUIStyle ToggleMixed { get { return CurrentSkin.m_ToggleMixed; } }

        public static GUIStyle Prefix { get { return CurrentSkin.m_Prefix; } }

        //public static GUIStyle MiniLabel { get { return CurrentSkin.m_MiniLabel; } }

        //public static GUIStyle LinkLabel { get { return CurrentSkin.m_LinkLabel; } }

        //public static GUIStyle LargeLabel { get { return CurrentSkin.m_LargeLabel; } }

        //public static GUIStyle BoldLabel { get { return CurrentSkin.m_BoldLabel; } }

        //public static GUIStyle MiniBoldLabel { get { return CurrentSkin.m_MiniBoldLabel; } }

        //public static GUIStyle WordWrappedLabel { get { return CurrentSkin.m_WordWrappedLabel; } }

        public static GUIStyle MiniButton { get { return CurrentSkin.m_MiniButton; } }

        public static GUIStyleVariants MiniButtons { get { return CurrentSkin.GetStyleVariants(CurrentSkin.m_MiniButton); } }

        //public static GUIStyle MiniButtonLeft { get { return CurrentSkin.m_MiniButtonLeft; } }

        //public static GUIStyle MiniButtonMid { get { return CurrentSkin.m_MiniButtonMid; } }

        //public static GUIStyle MiniButtonRight { get { return CurrentSkin.m_MiniButtonRight; } }

        //public static GUIStyle NotificationText { get { return CurrentSkin.m_NotificationText; } }

        //public static GUIStyle NotificationBackground { get { return CurrentSkin.m_NotificationBackground; } }

        //public static GUIStyle ProgressBarBack { get { return CurrentSkin.m_ProgressBarBack; } }

        //public static GUIStyle ProgressBarBar { get { return CurrentSkin.m_ProgressBarBar; } }

        //public static GUIStyle ProgressBarText { get { return CurrentSkin.m_ProgressBarText; } }

        //public static GUIStyle SearchField { get { return CurrentSkin.m_SearchField; } }

        //public static GUIStyle SearchFieldCancelButton { get { return CurrentSkin.m_SearchFieldCancelButton; } }

        public static GUIStyle MenuWindow { get { return CurrentSkin.m_MenuWindow; } }

        public static GUIStyle MenuItem { get { return CurrentSkin.m_MenuItem; } }
    }

    public class GUIStyleVariants
    {
        public readonly string baseName;
        public readonly GUIStyle normal;
        public readonly GUIStyle left;
        public readonly GUIStyle mid;
        public readonly GUIStyle thumb;
        public readonly GUIStyle right;
        public readonly GUIStyle up;
        public readonly GUIStyle down;

        // Those properties are guaranteed to be not null

        public GUIStyle Normal { get { return normal; } }
        public GUIStyle Mid { get { return mid ?? normal; } }
        public GUIStyle Left { get { return left ?? mid ?? normal; } }
        public GUIStyle Right { get { return right ?? mid ?? normal; } }
        public GUIStyle Up { get { return up ?? mid ?? normal; } }
        public GUIStyle Down { get { return down ?? mid ?? normal; } }

        public GUIStyleVariants(GUISkin skin, string name)
        {
            baseName = name;
            // Must not null
            normal = skin.GetStyle(name) ?? GUIStyle.none;
            // Names follow convention that should work for miniButtons and Scrollbar
            mid = skin.FindStyle(name + "Mid");
            thumb = skin.FindStyle(name + "Thumb");
            left = skin.FindStyle(name + "Left") ?? skin.FindStyle(name + "LeftButton");
            right = skin.FindStyle(name + "Right") ?? skin.FindStyle(name + "RightButton");
            up = skin.FindStyle(name + "Up") ?? skin.FindStyle(name + "UpButton");
            down = skin.FindStyle(name + "Down") ?? skin.FindStyle(name + "DownButton");
        }

        public GUIStyleVariants(GUISkin skin, GUIStyle normalStyle) : this(skin, normalStyle.name) { }

    }

}
