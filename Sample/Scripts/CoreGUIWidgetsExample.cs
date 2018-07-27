using UnityEngine;
using static CoreGUI;

public class CoreGUIWidgetsExample : MonoBehaviour
{
    public Side labelSide = Side.Left;

    string normalText;
    string areaText;
    string passwordText;

    float floatValue;
    int intValue;
    bool boolValue;

    Rect rectValue;
    Bounds boundsValue;
    Color colorValue = new Color(0, 0.5f, 0.8f, 0.5f);
    Quaternion quaternionValue;

    Vector2 vector2Value;
    Vector3 vector3Value;
    Vector4 vector4Value;

    Vector2 scroll;
    bool foldout;

    public GUISkin skin;
    public float scale = 1;
    float refScale;

    private void Start()
    {
        scale = Utility.ScaleFromDPI(96);
        floatValue = Utility.pixelsPerPoint;
    }

    private void OnGUI()
    {
        BeginGUI(this, Utility.screenRect, skin, scale);

        using (Scoped.LabelOption(labelSide <= Side.Right ? 120 : -1, labelSide))
        using (Scoped.ScrollView(ref scroll))
        using (Scoped.Vertical(null, Layout.MinWidth(500)))
        using (Scoped.Indent())
        {
            Label(C("GUI Core Widgets", "Lorem Ipsum"));

            Label(C("Settings"));
            using (Scoped.Indent())
            {
                labelSide = EnumPopup(C("Label Side"), labelSide);
                scale = Utility.GetDeferredValue(scale, x => FloatSlider(C("Scale"), x, 0.5f, 3f));
            }

            Label(C("Strings"));
            using (Scoped.Indent())
            {
                normalText = TextField(C("Normal Text"), normalText);
                areaText = TextArea(C("Text Area"), areaText, 1, 5);
                passwordText = PasswordField(C("Password"), passwordText, '●');
            }

            Label(C("Numbers"));
            using (Scoped.Indent())
            {
                floatValue = FloatField(C("Float Field"), floatValue);
                intValue = IntField(C("Int Field"), intValue);
                floatValue = FloatSlider(C("Float Range"), floatValue, 0, 10);
                intValue = IntSlider(C("Int Range"), intValue, 0, 10);
            }

            Label(C("Vectors"));
            using (Scoped.Indent())
            {
                vector2Value = VectorField(C("Vector 2"), vector2Value);
                vector3Value = VectorField(C("Vector 3"), vector3Value);
                vector4Value = VectorField(C("Vector 4"), vector4Value);
                quaternionValue = QuaternionField(C("Euler"), quaternionValue);
            }

            Label(C("Structs"));
            using (Scoped.Indent())
            {
                colorValue = ColorField(C("Color"), colorValue);
                rectValue = RectField(C("Rect"), rectValue);
                boundsValue = BoundsField(C("Bounds"), boundsValue);
            }

            Scoped.FadeGroup(Foldout(C("Foldout"), ref foldout), () =>
            {
                using (Scoped.Horizontal(C("Buttons")))
                {
                    using (Scoped.Vertical())
                    {
                        if (Button(C("Lorem ipsum dolor sit amett")))
                            MessagePopup.Show("This is the Title", "This is a message box");
                        if (Button(C("Click me")))
                            MessagePopup.Show(0, "Something is right", "Sorry sir it's not broken....",
                                MessagePopup.ButtonScheme.ContinueRetryAbort);
                    }
                    using (Scoped.Vertical())
                    {
                        if (Button(C("Beep boop")))
                        {
#if UNITY_EDITOR
                            UnityEditor.Selection.activeObject = GUI.skin;
#endif
                        }
                        Button(C("Another button here"));
                    }
                }

                using (Scoped.Indent())
                {
                    Label(C("Lorem Ipsum dolor sit amet"));
                    Label(C("Lorem Ipsum dolor sit amet"));
                    Label(C("Lorem Ipsum dolor sit amet"));
                }
            });

            scroll = DragScrollView(scroll);
        }

        DrawTooltips();
        EndGUI();
    }
}

