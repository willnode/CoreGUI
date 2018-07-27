using System;
using UnityEditor;
using UnityEngine;
using static CoreGUI;

[CustomEditor(typeof(GUISkin))]
public class GUISkinEditor : Editor
{
    void Draw(GUIStyle style)
    {
        var flag = selected == style;
        var c = C(style.fixedWidth == 0 ? style.name : string.Empty, style.name);
        var r = Indent(Reserve(GUIContent.none, style), true);
        Space(4);
        r.height += 4;
        if (GUI.Toggle(r, flag, c, style))
            selected = style;
        else if (ev.type == EventType.MouseDown && r.Contains(ev.mousePosition))
            selected = style;

        r.x -= 12;
        r.width = 8;

        if (flag && ev.type == EventType.Repaint)
        {
            GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill);
        }
    }

    [NonSerialized]
    GUIStyle selected;

    int selectedTab;
    int selectedState;
    int selectedParams;
    static bool secretButton;

    static readonly GUIContent[] s_tabs = Utility.ToGUIContents("Builtin", "Custom");
    static readonly GUIContent[] s_styleStates = Utility.ToGUIContents("Normal", "Hover", "Active", "Focus");
    static readonly GUIContent[] s_styleStates2 = Utility.ToGUIContents("On Normal", "On Hover", "On Active", "On Focus");
    static readonly GUIContent[] s_styleParams = Utility.ToGUIContents("Border", "Margin", "Overflow", "Padding");
    static readonly GUIContent[] s_customTask = Utility.ToGUIContents("+", "-");

    Vector2 scroll;

    static Rect GetRect()
    {
        var r = Utility.BindFromGUILayout();
        r.yMax = Screen.height - 50;
        return r;
    }

    private void OnEnable()
    {
        // Ssshhhh.... type "internal" in Help -> About Unity
        secretButton = EditorPrefs.GetBool("DeveloperMode", false);
    }

    public override void OnInspectorGUI()
    {
        var s = (GUISkin)target;
        GUI.enabled = true;
        BeginGUI(this, GetRect());
        BeginChangeCheck();
        using (Scoped.Horizontal(null, Layout.ExpandHeight(true)))
        {
            using (Scoped.Vertical(null, Layout.Width(EditorGUIUtility.labelWidth)))
            {
                selectedTab = HorizontalButtons(null, s_tabs, selectedTab);
                using (Scoped.ScrollView(ref scroll, false, false, Layout.ExpandHeight(true)))
                using (Scoped.LayoutOption(Layout.ExpandWidth(true)))
                using (Scoped.Indent())
                {
                    if (selectedTab == 0)
                    {
                        Draw(s.box);
                        Draw(s.label);
                        Draw(s.textField);
                        Draw(s.textArea);
                        Draw(s.button);
                        Draw(s.toggle);
                        Draw(s.window);
                        Draw(s.scrollView);
                        Draw(s.horizontalSlider);
                        Draw(s.horizontalSliderThumb);
                        Draw(s.horizontalScrollbar);
                        Draw(s.horizontalScrollbarThumb);
                        Draw(s.horizontalScrollbarLeftButton);
                        Draw(s.horizontalScrollbarRightButton);
                        Draw(s.verticalSlider);
                        Draw(s.verticalSliderThumb);
                        Draw(s.verticalScrollbar);
                        Draw(s.verticalScrollbarThumb);
                        Draw(s.verticalScrollbarUpButton);
                        Draw(s.verticalScrollbarDownButton);
                    }
                    else
                    {
                        var selectedAct = HorizontalButtons(null, s_customTask, -1);
                        if (selectedAct != -1)
                        {
                            Undo.RecordObject(s, "Add/Remove Style");
                            var a = s.customStyles;
                            var sel = Array.IndexOf(a, selected);
                            if (sel < 0)
                                sel = Mathf.Max(0, a.Length - 1);
                            if (selectedAct == 0)
                            {
                                var styles = s.customStyles;
                                ArrayUtility.Insert(ref styles, styles.Length == 0 ? 0 : sel + 1, 
                                    new GUIStyle(selected ?? GUIStyle.none));
                                s.customStyles = styles;
                            } else
                            {
                                var styles = s.customStyles;
                                ArrayUtility.RemoveAt(ref styles, sel);
                                s.customStyles = styles;
                            }
                        }
                        foreach (var style in s.customStyles)
                        {
                            Draw(style);
                        }
                    }
                }
            }

            using (Scoped.Vertical(null, Layout.ExpandHeight(true), Layout.ExpandWidth(true)))
            {
                if (secretButton && Button(C("Select Editor Skin")))
                    Selection.activeObject = GUI.skin;

                using (Scoped.Vertical())
                using (Scoped.DisabledGroup((s.hideFlags & HideFlags.NotEditable) != 0))
                {
                    if (selected != null)
                    {
                        using (Scoped.DisabledGroup(selectedTab == 0))
                        {
                            selected.name = TextField(C("Name"), selected.name);
                        }

                        var r = FlexibleSpace();

                        using (Scoped.DisabledGroup(false, true))
                        {
                            if (ev.type == EventType.Repaint)
                            {
                                var center = r.center;
                                r.size *= 0.5f;
                                r.center = center;
                                selected.Draw(r, selectedState % 4 == 1, selectedState % 4 == 2, selectedState >= 4, selectedState % 4 == 3);
                            }
                            selectedState = HorizontalButtons(null, s_styleStates, selectedState, 0);
                            selectedState = HorizontalButtons(null, s_styleStates2, selectedState, 4);
                        }

                        using (Scoped.Indent())
                        {
                            var state = GetState(selectedState, selected);
                            state.background = (Texture2D)PropertyField(C("Background"), state.background, typeof(Texture2D));
                            state.textColor = ColorField(C("Text Color"), state.textColor);
                            // For simplicity, we just show one ScaledBackground.
                            var bg = state.scaledBackgrounds;
                            Texture2D scaledBg = bg.Length == 0 ? null : bg[0];
                            BeginChangeCheck();
                            scaledBg = (Texture2D)PropertyField(C("Scaled BG"), scaledBg, typeof(Texture2D));
                            if (EndChangeCheck())
                            {
                                state.scaledBackgrounds = scaledBg ? new Texture2D[] { scaledBg } : new Texture2D[] { };
                            }
                        }
                        Space(16);
                        using (Scoped.DisabledGroup(false, true))
                        {
                            selectedParams = HorizontalButtons(null, s_styleParams, selectedParams, 0);
                        }
                        using (Scoped.Indent())
                        {
                            switch (selectedParams)
                            {
                                case 0: selected.border = RectOffsetField(null, selected.border); break;
                                case 1: selected.margin = RectOffsetField(null, selected.margin); break;
                                case 2: selected.overflow = RectOffsetField(null, selected.overflow); break;
                                case 3: selected.padding = RectOffsetField(null, selected.padding); break;
                            }
                        }
                        Space(16);

                        selected.contentOffset = VectorField(C("Content Offset"), selected.contentOffset);

                        using (Scoped.Horizontal())
                        {

                            using (Scoped.Vertical())
                            {
                                selected.alignment = EnumPopup(C("Aligment"), selected.alignment);
                                selected.fixedWidth = FloatField(C("Fixed Width"), selected.fixedWidth);
                                selected.font = (Font)PropertyField(C("Font"), selected.font, typeof(Font));
                                selected.fontSize = IntField(C("Font Size"), selected.fontSize);
                                selected.stretchWidth = Checkbox(C("Stretch Width"), selected.stretchWidth);
                                selected.richText = Checkbox(C("Rich Text"), selected.richText);
                            }
                            using (Scoped.Vertical())
                            {
                                selected.clipping = EnumPopup(C("Clipping"), selected.clipping);
                                selected.fixedHeight = FloatField(C("Fixed Height"), selected.fixedHeight);
                                selected.fontStyle = EnumPopup(C("Font Style"), selected.fontStyle);
                                selected.imagePosition = EnumPopup(C("Image Position"), selected.imagePosition);
                                selected.stretchHeight = Checkbox(C("Stretch Height"), selected.stretchHeight);
                                selected.wordWrap = Checkbox(C("Word Wrap"), selected.wordWrap);
                            }
                        }
                    }
                }
            }
        }
        if (EndChangeCheck())
            EditorUtility.SetDirty(s);
        EndGUI();
    }

    static GUIStyleState GetState(int mode, GUIStyle style)
    {
        switch (mode)
        {
            default: case 0: return style.normal;
            case 1: return style.hover;
            case 2: return style.active;
            case 3: return style.focused;
            case 4: return style.onNormal;
            case 5: return style.onHover;
            case 6: return style.onActive;
            case 7: return style.onFocused;
        }
    }
}

