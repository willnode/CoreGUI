using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static partial class CoreGUI
{
    public class Styles
    {
        private GUISkin skin;

        public Styles(GUISkin skin)
        {
            this.skin = skin;
            Invalidate();
        }

        private GUIStyle m_Box, m_Label, m_TextField, m_TextArea, m_Button, m_Toggle, m_Window, m_ScrollView;

        private GUIStyle m_HorizontalSlider, m_HorizontalSliderThumb, m_VerticalSlider, m_VerticalSliderThumb, m_MinMaxHorizontalSliderThumb;
        
        private GUIStyle m_HorizontalScrollbar, m_HorizontalScrollbarThumb, m_HorizontalScrollbarLeftButton, m_HorizontalScrollbarRightButton;
        
        private GUIStyle m_VerticalScrollbar, m_VerticalScrollbarThumb, m_VerticalScrollbarUpButton, m_VerticalScrollbarDownButton;

        private GUIStyle m_HelpBox, m_Foldout, m_RadioButton, m_Tooltip, m_NumberField, m_ColorField, m_DropDownList, m_Popup, m_ToggleMixed;
        
        private GUIStyle m_Prefix, m_MiniLabel, m_LargeLabel, m_BoldLabel, m_MiniBoldLabel, m_WordWrappedLabel, m_LinkLabel;

        private GUIStyle m_MiniButton, m_MiniButtonLeft, m_MiniButtonMid, m_MiniButtonRight, m_NotificationText, m_NotificationBackground;

        private GUIStyle m_ProgressBarBack, m_ProgressBarBar, m_ProgressBarText, m_SearchField, m_SearchFieldCancelButton;

        public GUIStyle GetStyle(string name)
        {
            return skin.FindStyle(name);
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

            m_HorizontalSlider = skin.horizontalScrollbar;
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
            m_Popup = GetStyle("MiniPopup") ?? m_DropDownList;
            m_NumberField = GetStyle("textField") ?? m_TextField;
            m_ToggleMixed = GetStyle("ToggleMixed") ?? m_Toggle;
            m_ColorField = GetStyle("ColorField") ?? m_Box;
            m_Foldout = GetStyle("Foldout") ?? m_Toggle;
            m_Prefix = GetStyle("Prefix") ?? m_Label;
            m_LinkLabel = GetStyle("LinkLabel") ?? new GUIStyle(m_Label);
            m_LinkLabel.normal.textColor = new Color(0.25f, 0.5f, 0.9f, 1f);
            m_LinkLabel.stretchWidth = false;
            m_TextArea.wordWrap = true;
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

        public static GUIStyle MinMaxHorizontalSliderThumb { get { return CurrentSkin.m_MinMaxHorizontalSliderThumb; } }

        public static GUIStyle HorizontalScrollbar { get { return CurrentSkin.m_HorizontalScrollbar; } }

        public static GUIStyle HorizontalScrollbarThumb { get { return CurrentSkin.m_HorizontalScrollbarThumb; } }

        public static GUIStyle HorizontalScrollbarLeftButton { get { return CurrentSkin.m_HorizontalScrollbarLeftButton; } }

        public static GUIStyle HorizontalScrollbarRightButton { get { return CurrentSkin.m_HorizontalScrollbarRightButton; } }

        public static GUIStyle VerticalScrollbar { get { return CurrentSkin.m_VerticalScrollbar; } }

        public static GUIStyle VerticalScrollbarThumb { get { return CurrentSkin.m_VerticalScrollbarThumb; } }

        public static GUIStyle VerticalScrollbarUpButton { get { return CurrentSkin.m_VerticalScrollbarUpButton; } }

        public static GUIStyle VerticalScrollbarDownButton { get { return CurrentSkin.m_VerticalScrollbarDownButton; } }

        public static GUIStyle HelpBox { get { return CurrentSkin.m_HelpBox; } }

        public static GUIStyle Foldout { get { return CurrentSkin.m_Foldout; } }

        public static GUIStyle RadioButton { get { return CurrentSkin.m_RadioButton; } }

        public static GUIStyle Tooltip { get { return CurrentSkin.m_Tooltip; } }

        public static GUIStyle NumberField { get { return CurrentSkin.m_NumberField; } }

        public static GUIStyle ColorField { get { return CurrentSkin.m_ColorField; } }

        public static GUIStyle DropDownList { get { return CurrentSkin.m_DropDownList; } }

        public static GUIStyle Popup { get { return CurrentSkin.m_Popup; } }

        public static GUIStyle ToggleMixed { get { return CurrentSkin.m_ToggleMixed; } }

        public static GUIStyle Prefix { get { return CurrentSkin.m_Prefix; } }

        public static GUIStyle MiniLabel { get { return CurrentSkin.m_MiniLabel; } }

        public static GUIStyle LargeLabel { get { return CurrentSkin.m_LargeLabel; } }

        public static GUIStyle BoldLabel { get { return CurrentSkin.m_BoldLabel; } }

        public static GUIStyle MiniBoldLabel { get { return CurrentSkin.m_MiniBoldLabel; } }

        public static GUIStyle WordWrappedLabel { get { return CurrentSkin.m_WordWrappedLabel; } }

        public static GUIStyle LinkLabel { get { return CurrentSkin.m_LinkLabel; } }

        public static GUIStyle MiniButton { get { return CurrentSkin.m_MiniButton; } }

        public static GUIStyle MiniButtonLeft { get { return CurrentSkin.m_MiniButtonLeft; } }

        public static GUIStyle MiniButtonMid { get { return CurrentSkin.m_MiniButtonMid; } }

        public static GUIStyle MiniButtonRight { get { return CurrentSkin.m_MiniButtonRight; } }

        public static GUIStyle NotificationText { get { return CurrentSkin.m_NotificationText; } }

        public static GUIStyle NotificationBackground { get { return CurrentSkin.m_NotificationBackground; } }

        public static GUIStyle ProgressBarBack { get { return CurrentSkin.m_ProgressBarBack; } }

        public static GUIStyle ProgressBarBar { get { return CurrentSkin.m_ProgressBarBar; } }

        public static GUIStyle ProgressBarText { get { return CurrentSkin.m_ProgressBarText; } }

        public static GUIStyle SearchField { get { return CurrentSkin.m_SearchField; } }

        public static GUIStyle SearchFieldCancelButton { get { return CurrentSkin.m_SearchFieldCancelButton; } }
    }
}
