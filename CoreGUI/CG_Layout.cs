using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngineInternal;

// Classes forked from Unity Internal Layout System
public static partial class CoreGUI
{
    public class Layout
    {

        // Option passed to a control to give it an absolute width.
        static public LayoutOption Width(float width) { return new LayoutOption(LayoutOption.Type.fixedWidth, width); }
        // Option passed to a control to specify a minimum width.\\
        static public LayoutOption MinWidth(float minWidth) { return new LayoutOption(LayoutOption.Type.minWidth, minWidth); }
        // Option passed to a control to specify a maximum width.
        static public LayoutOption MaxWidth(float maxWidth) { return new LayoutOption(LayoutOption.Type.maxWidth, maxWidth); }
        // Option passed to a control to give it an absolute height.
        static public LayoutOption Height(float height) { return new LayoutOption(LayoutOption.Type.fixedHeight, height); }

        // Option passed to a control to specify a minimum height.
        static public LayoutOption MinHeight(float minHeight) { return new LayoutOption(LayoutOption.Type.minHeight, minHeight); }

        // Option passed to a control to specify a maximum height.
        static public LayoutOption MaxHeight(float maxHeight) { return new LayoutOption(LayoutOption.Type.maxHeight, maxHeight); }

        // Option passed to a control to allow or disallow horizontal expansion.
        static public LayoutOption ExpandWidth(bool expand) { return new LayoutOption(LayoutOption.Type.stretchWidth, expand ? 1 : 0); }
        // Option passed to a control to allow or disallow vertical expansion.
        static public LayoutOption ExpandHeight(bool expand) { return new LayoutOption(LayoutOption.Type.stretchHeight, expand ? 1 : 0); }

    }
    // Utility functions for implementing and extending the GUILayout class.
    public class LayoutUtility
    {
        public sealed class LayoutCache
        {
            public LayoutGroup topLevel = new LayoutGroup();
            public Stack<LayoutGroup> layoutGroups = new Stack<LayoutGroup>();
            public LayoutGroup windows = new LayoutGroup();

            public LayoutCache()
            {
                layoutGroups.Push(topLevel);
            }

            public LayoutCache(LayoutCache other)
            {
                topLevel = other.topLevel;
                layoutGroups = other.layoutGroups;
                windows = other.windows;
            }
        }

        // TODO: Clean these up after a while
        static readonly Dictionary<int, LayoutCache> s_StoredLayouts = new Dictionary<int, LayoutCache>();
        static readonly Dictionary<int, LayoutCache> s_StoredWindows = new Dictionary<int, LayoutCache>();

        public static LayoutCache current = new LayoutCache();

        public static readonly Rect kDummyRect = new Rect(0, 0, 1, 1);

        public static void CleanupRoots()
        {
            // See GUI.CleanupRoots
            s_SpaceStyle = null;
            s_StoredLayouts.Clear();
            s_StoredWindows.Clear();
            current = new LayoutCache();
        }

        public static LayoutCache SelectIDList(int instanceID, bool isWindow)
        {
            Dictionary<int, LayoutCache> store = isWindow ? s_StoredWindows : s_StoredLayouts;
            LayoutCache cache;
            if (store.TryGetValue(instanceID, out cache) == false)
            {
                //          Debug.Log ("Creating ID " +instanceID + " " + Event.current.type);
                cache = new LayoutCache();
                store[instanceID] = cache;
            }
            else
            {
                //          Debug.Log ("reusing ID " +instanceID + " " + Event.current.type);
            }
            current.topLevel = cache.topLevel;
            current.layoutGroups = cache.layoutGroups;
            current.windows = cache.windows;
            return cache;
        }

        // Set up the public GUILayouting
        // Called by the main GUI class automatically (from GUI.Begin)
        public static void Begin(int instanceID)
        {
            LayoutCache cache = SelectIDList(instanceID, false);
            // Make a vertical group to encompass the whole thing
            if (Event.current.type == EventType.Layout)
            {
                if (cache.topLevel != null)
                    cache.topLevel.Flush();
                if (cache.windows != null)
                    cache.windows.Flush();

                current.topLevel = cache.topLevel = MemPool<LayoutGroup>.Get();
                current.layoutGroups.Clear();
                current.layoutGroups.Push(current.topLevel);
                current.windows = cache.windows = MemPool<LayoutGroup>.Get();
            }
            else
            {
                current.topLevel = cache.topLevel;
                current.layoutGroups = cache.layoutGroups;
                current.windows = cache.windows;
            }
        }

        public static void BeginContainer(LayoutCache cache)
        {
            // Make a vertical group to encompass the whole thing
            if (Event.current.type == EventType.Layout)
            {
                // Make sure to update all the cached values of the LayoutCache when doing a Layout
                cache.topLevel = new LayoutGroup();
                cache.layoutGroups.Clear();
                cache.layoutGroups.Push(cache.topLevel);
                cache.windows = new LayoutGroup();
            }
            // Make sure to use the actual cache.
            current.topLevel = cache.topLevel;
            current.layoutGroups = cache.layoutGroups;
            current.windows = cache.windows;
        }

        public static void BeginWindow(int windowID, GUIStyle style, LayoutOption[] options)
        {
            LayoutCache cache = SelectIDList(windowID, true);
            // Make a vertical group to encompass the whole thing
            if (Event.current.type == EventType.Layout)
            {
                current.topLevel = cache.topLevel = new LayoutGroup();
                current.topLevel.style = style;
                current.topLevel.windowID = windowID;
                if (options != null)
                    current.topLevel.ApplyOptions(options);
                current.layoutGroups.Clear();
                current.layoutGroups.Push(current.topLevel);
                current.windows = cache.windows = new LayoutGroup();
            }
            else
            {
                current.topLevel = cache.topLevel;
                current.layoutGroups = cache.layoutGroups;
                current.windows = cache.windows;
            }
        }

        public static void Layout()
        {
            if (current.topLevel.windowID == -1)
            {
                // Normal GUILayout.whatever -outside beginArea calls.
                // Here we go over all entries and calculate their sizes
                current.topLevel.CalcWidth();
                current.topLevel.SetHorizontal(0, Mathf.Min(Screen.width / Utility.pixelsPerPoint, current.topLevel.maxWidth));
                current.topLevel.CalcHeight();
                current.topLevel.SetVertical(0, Mathf.Min(Screen.height / Utility.pixelsPerPoint, current.topLevel.maxHeight));

                LayoutFreeGroup(current.windows);
            }
            else
            {
                LayoutSingleGroup(current.topLevel);
                LayoutFreeGroup(current.windows);
            }
        }

        // Global layout function. Called from EditorWindows (differs from game view in that they use the full window size and try to stretch GUI
        public static void LayoutFromEditorWindow()
        {
            if (current.topLevel != null)
            {
                current.topLevel.CalcWidth();
                current.topLevel.SetHorizontal(0, Screen.width / Utility.pixelsPerPoint);
                current.topLevel.CalcHeight();
                current.topLevel.SetVertical(0, Screen.height / Utility.pixelsPerPoint);

                // UNCOMMENT ME TO DEBUG THE EditorWindow ROOT LAYOUT RESULTS
                //          Debug.Log (current.topLevel);
                // Layout all beginarea parts...
                LayoutFreeGroup(current.windows);
            }
            else
            {
                Debug.LogError("GUILayout state invalid. Verify that all layout begin/end calls match.");
            }
        }

        public static void LayoutFromContainer(float w, float h)
        {
            if (current.topLevel != null)
            {
                current.topLevel.CalcWidth();
                current.topLevel.SetHorizontal(0, w);
                current.topLevel.CalcHeight();
                current.topLevel.SetVertical(0, h);

                // UNCOMMENT ME TO DEBUG THE EditorWindow ROOT LAYOUT RESULTS
                //          Debug.Log (current.topLevel);
                // Layout all beginarea parts...
                LayoutFreeGroup(current.windows);
            }
            else
            {
                Debug.LogError("GUILayout state invalid. Verify that all layout begin/end calls match.");
            }
        }

        // Global layout function. Calculates all sizes of all windows etc & assigns.
        // After this call everything has a properly calculated size
        // Called by Unity automatically.
        // Is public so we can access it from editor inspectors, but not supported by public stuff
        public static float LayoutFromInspector(float width)
        {
            if (current.topLevel != null && current.topLevel.windowID == -1)
            {
                // Here we go over all entries and calculate their sizes
                current.topLevel.CalcWidth();
                current.topLevel.SetHorizontal(0, width);
                current.topLevel.CalcHeight();
                current.topLevel.SetVertical(0, Mathf.Min(Screen.height / Utility.pixelsPerPoint, current.topLevel.maxHeight));
                // UNCOMMENT ME TO DEBUG THE INSPECTOR
                //          Debug.Log (current.topLevel);
                float height = current.topLevel.minHeight;
                // Layout all beginarea parts...
                // TODO: NOT SURE HOW THIS WORKS IN AN INSPECTOR
                LayoutFreeGroup(current.windows);
                return height;
            }
            if (current.topLevel != null)
                LayoutSingleGroup(current.topLevel);
            return 0;
        }

        public static void LayoutFreeGroup(LayoutGroup toplevel)
        {
            foreach (LayoutGroup i in toplevel.entries)
            {
                LayoutSingleGroup(i);
            }
            toplevel.ResetCursor();
        }

        static void LayoutSingleGroup(LayoutGroup i)
        {
            //if (!i.isWindow)
            {
                // CalcWidth knocks out minWidth with the calculated sizes from its children. Normally, this is fine, but since we're in a fixed-size area,
                // we want to maintain that
                float origMinWidth = i.minWidth;
                float origMaxWidth = i.maxWidth;

                // Figure out the group's min & maxWidth.
                i.CalcWidth();

                // Make it as wide as possible, but the Rect supplied takes precedence...
                i.SetHorizontal(i.rect.x, Mathf.Clamp(i.maxWidth, origMinWidth, origMaxWidth));

                // Do the same preservation for CalcHeight...
                float origMinHeight = i.minHeight;
                float origMaxHeight = i.maxHeight;

                i.CalcHeight();
                // Make it as high as possible, but the Rect supplied takes precedence...
                i.SetVertical(i.rect.y, Mathf.Clamp(i.maxHeight, origMinHeight, origMaxHeight));
            }
            //else
            //{
            //    // Figure out the group's min & maxWidth.
            //    i.CalcWidth();

            //    Rect winRect = Internal_GetWindowRect(i.windowID);

            //    // Make it as wide as possible, but the Rect supplied takes precedence...
            //    i.SetHorizontal(winRect.x, Mathf.Clamp(winRect.width, i.minWidth, i.maxWidth));

            //    i.CalcHeight();

            //    // Make it as high as possible, but the Rect supplied takes precedence...
            //    i.SetVertical(winRect.y, Mathf.Clamp(winRect.height, i.minHeight, i.maxHeight));

            //    // If GUILayout did any resizing, make sure the window reflects this.

            //    Internal_MoveWindow(i.windowID, i.rect);
            //}
        }

        // Generic helper - use this when creating a layoutgroup. It will make sure everything is wired up correctly.
        public static LayoutGroup BeginLayoutGroup<T>(GUIStyle style, LayoutOption[] options) where T : LayoutGroup, new()
        {
            LayoutGroup g;
            switch (Event.current.type)
            {
                case EventType.Used:
                case EventType.Layout:
                    g = MemPool<T>.Get();
                    g.style = style;
                    if (options != null)
                        g.ApplyOptions(options);
                    current.topLevel.Add(g);
                    break;
                default:
                    g = current.topLevel.GetNext() as LayoutGroup;
                    if (g == null)
                        throw new ArgumentException("GUILayout: Mismatched LayoutGroup." + Event.current.type);
                    g.ResetCursor();
                    //GUIDebugger.LogLayoutGroupEntry(g.rect, g.margin, g.style, g.isVertical);
                    break;
            }
            current.layoutGroups.Push(g);
            current.topLevel = g;
            DebugDraw(g.rect);
            return g;
        }

        static void DebugDraw(Rect r)
        {
            //if (ev.type == EventType.Repaint)
            //    Styles.Box.Draw(r, false, false, false, false);
        }

        // The matching end for BeginLayoutGroup
        public static void EndLayoutGroup()
        {
            if (current.layoutGroups.Count == 0
                || Event.current == null
                )
            {
                Debug.LogError("EndLayoutGroup: BeginLayoutGroup must be called first.");

                return;
            }
            //if (Event.current.type != EventType.Layout && Event.current.type != EventType.Used)
            //GUIDebugger.LogLayoutEndGroup();

            current.layoutGroups.Pop();
            if (0 < current.layoutGroups.Count)
                current.topLevel = current.layoutGroups.Peek();
            else
                current.topLevel = new LayoutGroup();
        }

        // Generic helper - use this when creating a layout group. It will make sure everything is wired up correctly.
        public static LayoutGroup BeginLayoutArea<T>(GUIStyle style) where T : LayoutGroup, new()
        {
            LayoutGroup g;
            switch (Event.current.type)
            {
                case EventType.Used:
                case EventType.Layout:
                    g = MemPool<T>.Get();
                    g.style = style;
                    current.windows.Add(g);
                    break;
                default:
                    g = current.windows.GetNext() as LayoutGroup;
                    if (g == null)
                        throw new ArgumentException("GUILayout: Mismatched LayoutGroup." + Event.current.type);
                    g.ResetCursor();
                    //GUIDebugger.LogLayoutGroupEntry(g.rect, g.margin, g.style, g.isVertical);
                    break;
            }
            current.layoutGroups.Push(g);
            current.topLevel = g;
            return g;
        }

        // Trampoline for Editor stuff
        public static LayoutGroup DoBeginLayoutArea<T>(GUIStyle style) where T : LayoutGroup, new()
        {
            return BeginLayoutArea<T>(style);
        }

        public static LayoutGroup topLevel { get { return current.topLevel; } }

        public static Rect GetRect(GUIContent content, GUIStyle style) { return DoGetRect(content, style, null); }
        // Reserve layout space for a rectangle for displaying some contents with a specific style.
        public static Rect GetRect(GUIContent content, GUIStyle style, params LayoutOption[] options) { return DoGetRect(content, style, options); }

        static Rect DoGetRect(GUIContent content, GUIStyle style, LayoutOption[] options)
        {
            //GUIUtility.CheckOnGUI();

            switch (Event.current.type)
            {
                case EventType.Layout:
                    if (style.isHeightDependantOnWidth)
                    {
                        current.topLevel.Add(WordWrapSizer.Get(style, content, options));
                    }
                    else
                    {
                        Vector2 sizeConstraints = new Vector2(0, 0);
                        if (options != null)
                        {
                            foreach (var option in options)
                            {
                                switch (option.type)
                                {
                                    case LayoutOption.Type.maxHeight:
                                        sizeConstraints.y = option.value;
                                        break;
                                    case LayoutOption.Type.maxWidth:
                                        sizeConstraints.x = option.value;
                                        break;
                                }
                            }
                        }

                        Vector2 size = Utility.CalcSizeWithConstraints(style, content, sizeConstraints);
                        current.topLevel.Add(LayoutEntry.Get(size.x, size.x, size.y, size.y, style, options));
                    }
                    return kDummyRect;

                case EventType.Used:
                    return kDummyRect;
                default:
                    var entry = current.topLevel.GetNext();
                    DebugDraw(entry.rect);
                    //GUIDebugger.LogLayoutEntry(entry.rect, entry.margin, entry.style);
                    return entry.rect;
            }
        }

        public static Rect GetRect(float width, float height) { return DoGetRect(width, width, height, height, GUIStyle.none, null); }
        public static Rect GetRect(float width, float height, GUIStyle style) { return DoGetRect(width, width, height, height, style, null); }
        public static Rect GetRect(float width, float height, params LayoutOption[] options) { return DoGetRect(width, width, height, height, GUIStyle.none, options); }
        // Reserve layout space for a rectangle with a fixed content area.
        public static Rect GetRect(float width, float height, GUIStyle style, params LayoutOption[] options)
        { return DoGetRect(width, width, height, height, style, options); }

        public static Rect GetRect(float minWidth, float maxWidth, float minHeight, float maxHeight)
        { return DoGetRect(minWidth, maxWidth, minHeight, maxHeight, GUIStyle.none, null); }

        public static Rect GetRect(float minWidth, float maxWidth, float minHeight, float maxHeight, GUIStyle style)
        { return DoGetRect(minWidth, maxWidth, minHeight, maxHeight, style, null); }

        public static Rect GetRect(float minWidth, float maxWidth, float minHeight, float maxHeight, params LayoutOption[] options)
        { return DoGetRect(minWidth, maxWidth, minHeight, maxHeight, GUIStyle.none, options); }
        // Reserve layout space for a flexible rect.
        public static Rect GetRect(float minWidth, float maxWidth, float minHeight, float maxHeight, GUIStyle style, params LayoutOption[] options)
        { return DoGetRect(minWidth, maxWidth, minHeight, maxHeight, style, options); }
        static Rect DoGetRect(float minWidth, float maxWidth, float minHeight, float maxHeight, GUIStyle style, LayoutOption[] options)
        {
            switch (Event.current.type)
            {
                case EventType.Layout:
                    current.topLevel.Add(LayoutEntry.Get(minWidth, maxWidth, minHeight, maxHeight, style, options));
                    return kDummyRect;
                case EventType.Used:
                    return kDummyRect;
                default:
                    var entry = current.topLevel.GetNext();
                    DebugDraw(entry.rect);
                    return entry.rect;
            }
        }

        public static bool isVertical { get { return current.topLevel.isVertical; } }

        // Get the rectangle of current layout group.
        public static Rect GetContainerRect(bool unpacked = false)
        {
            switch (Event.current.type)
            {
                case EventType.Layout:
                case EventType.Used:
                    return kDummyRect;
                default:
                    var top = current.topLevel;
                    var r = top.rect;
                    if (top.resetCoords)
                        r.position = Vector2.zero;
                    if (unpacked)
                        r = top.margin.Add(r);
                    return r;
            }
        }

        // Get the rectangle last used by GUILayout for a control.
        public static Rect GetLastRect()
        {
            switch (Event.current.type)
            {
                case EventType.Layout:
                case EventType.Used:
                    return kDummyRect;
                default:
                    return current.topLevel.GetLast();
            }
        }

        public static Rect GetAspectRect(float aspect) { return DoGetAspectRect(aspect, null); }
        public static Rect GetAspectRect(float aspect, GUIStyle style) { return DoGetAspectRect(aspect, null); }
        public static Rect GetAspectRect(float aspect, params LayoutOption[] options) { return DoGetAspectRect(aspect, options); }
        // Reserve layout space for a rectangle with a specific aspect ratio.
        public static Rect GetAspectRect(float aspect, GUIStyle style, params LayoutOption[] options) { return DoGetAspectRect(aspect, options); }
        private static Rect DoGetAspectRect(float aspect, LayoutOption[] options)
        {
            switch (Event.current.type)
            {
                case EventType.Layout:
                    current.topLevel.Add(AspectSizer.Get(aspect, options));
                    return kDummyRect;
                case EventType.Used:
                    return kDummyRect;
                default:
                    var entry = current.topLevel.GetNext();
                    DebugDraw(entry.rect);
                    return entry.rect;
            }
        }

        // Style used by space elements so we can do special handling of spaces.
        public static GUIStyle spaceStyle
        {
            get
            {
                if (s_SpaceStyle == null)
                {
                    s_SpaceStyle = new GUIStyle();
                    s_SpaceStyle.stretchWidth = false;
                    s_SpaceStyle.stretchHeight = false;
                }
                return s_SpaceStyle;
            }
        }
        static GUIStyle s_SpaceStyle;
    }

    public class LayoutGroup : LayoutEntry
    {
        public List<LayoutEntry> entries = new List<LayoutEntry>();
        public bool isVertical = true;                  // Is this group vertical
        public bool resetCoords = false;                // Reset coordinate for GetRect. Used for groups that are part of a window
        public float spacing = 0;                       // Spacing between the elements contained within
        public bool sameSize = true;                    // Are all subelements the same size
        public bool isWindow = false;                   // Is this a window at all?
        public int windowID = -1;                           // Optional window ID for toplevel windows. Used by Layout to tell GUI.Window of size changes...
        int m_Cursor = 0;
        protected float m_StretchableCountX = 100;
        protected float m_StretchableCountY = 100;
        protected bool m_UserSpecifiedWidth = false;
        protected bool m_UserSpecifiedHeight = false;
        // Should all elements be the same size?
        // TODO: implement
        //  bool equalSize = false;

        // The summed sizes of the children. This is used to determine whether or not the children should be stretched
        protected float m_ChildMinWidth = 100;
        protected float m_ChildMaxWidth = 100;
        protected float m_ChildMinHeight = 100;
        protected float m_ChildMaxHeight = 100;

        // How are subelements justified along the minor direction?
        // TODO: implement
        //  enum Align { start, middle, end, justify }
        //  Align align;

        readonly RectOffset m_Margin = new RectOffset();
        public override RectOffset margin { get { return m_Margin; } }

        public LayoutGroup() { }

        public static LayoutGroup Get()
        {
            return Get(GUIStyle.none, null);
        }

        public static LayoutGroup Get(GUIStyle _style, LayoutOption[] options)
        {
            var lyt = MemPool<LayoutGroup>.Get();
            lyt.style = _style;
            if (options != null)
                lyt.ApplyOptions(options);
            lyt.m_Margin.left = _style.margin.left;
            lyt.m_Margin.right = _style.margin.right;
            lyt.m_Margin.top = _style.margin.top;
            lyt.m_Margin.bottom = _style.margin.bottom;
            return lyt;
        }

        public override void Flush()
        {
            FlushBase();
            MemPool<LayoutGroup>.Release(this);
        }

        public override void FlushBase()
        {
            base.FlushBase();
            foreach (var entry in entries)
            {
                entry.Flush();
            }
            entries.Clear();

            isVertical = true;
            resetCoords = false;
            spacing = 0;
            sameSize = true;
            isWindow = false;
            windowID = -1;
            m_Cursor = 0;
            m_StretchableCountX = 100;
            m_StretchableCountY = 100;
            m_UserSpecifiedWidth = false;
            m_UserSpecifiedHeight = false;

            m_ChildMinWidth = 100;
            m_ChildMaxWidth = 100;
            m_ChildMinHeight = 100;
            m_ChildMaxHeight = 100;
        }

        public override void ApplyOptions(LayoutOption[] options)
        {
            if (options == null)
                return;
            base.ApplyOptions(options);
            foreach (LayoutOption i in options)
            {
                switch (i.type)
                {
                    case LayoutOption.Type.fixedWidth:
                    case LayoutOption.Type.minWidth:
                    case LayoutOption.Type.maxWidth:
                        m_UserSpecifiedHeight = true;
                        break;
                    case LayoutOption.Type.fixedHeight:
                    case LayoutOption.Type.minHeight:
                    case LayoutOption.Type.maxHeight:
                        m_UserSpecifiedWidth = true;
                        break;
                    // TODO:
                    //              case LayoutOption.Type.alignStart:       align = Align.start; break;
                    //              case LayoutOption.Type.alignMiddle:      align = Align.middle; break;
                    //              case LayoutOption.Type.alignEnd:     align = Align.end; break;
                    //              case LayoutOption.Type.alignJustify:     align = Align.justify; break;
                    //              case LayoutOption.Type.equalSize:        equalSize = true; break;
                    case LayoutOption.Type.spacing: spacing = (int)i.value; break;
                }
            }
        }

        protected override void ApplyStyleSettings(GUIStyle style)
        {
            base.ApplyStyleSettings(style);
            RectOffset mar = style.margin;
            m_Margin.left = mar.left;
            m_Margin.right = mar.right;
            m_Margin.top = mar.top;
            m_Margin.bottom = mar.bottom;
        }

        public void ResetCursor() { m_Cursor = 0; }

        public Rect PeekNext()
        {
            if (m_Cursor < entries.Count)
            {
                LayoutEntry e = entries[m_Cursor];
                return e.rect;
            }

            throw new ArgumentException("Getting control " + m_Cursor + "'s position in a group with only " + entries.Count + " controls when doing " + Event.current.rawType + "\nAborting");
        }

        public LayoutEntry GetNext()
        {
            if (m_Cursor < entries.Count)
            {
                LayoutEntry e = entries[m_Cursor];
                m_Cursor++;
                return e;
            }

            throw new ArgumentException("Getting control " + m_Cursor + "'s position in a group with only " + entries.Count + " controls when doing " + Event.current.rawType + "\nAborting");
        }

        //* undocumented
        public Rect GetLast()
        {
            if (m_Cursor == 0)
            {
                Debug.LogError("You cannot call GetLast immediately after beginning a group.");
                return kDummyRect;
            }

            if (m_Cursor <= entries.Count)
            {
                LayoutEntry e = entries[m_Cursor - 1];
                return e.rect;
            }

            Debug.LogError("Getting control " + m_Cursor + "'s position in a group with only " + entries.Count + " controls when doing " + Event.current.type);
            return kDummyRect;
        }

        public void Add(LayoutEntry e)
        {
            entries.Add(e);
        }

        public override void CalcWidth()
        {
            if (entries.Count == 0)
            {
                maxWidth = minWidth = style.padding.horizontal;
                return;
            }

            int leftMarginMin = 0;
            int rightMarginMin = 0;

            m_ChildMinWidth = 0;
            m_ChildMaxWidth = 0;
            m_StretchableCountX = 0;
            bool first = true;
            if (isVertical)
            {
                foreach (LayoutEntry i in entries)
                {
                    i.CalcWidth();
                    RectOffset margins = i.margin;
                    if (i.style != LayoutUtility.spaceStyle)
                    {
                        if (!first)
                        {
                            leftMarginMin = Mathf.Min(margins.left, leftMarginMin);
                            rightMarginMin = Mathf.Min(margins.right, rightMarginMin);
                        }
                        else
                        {
                            leftMarginMin = margins.left;
                            rightMarginMin = margins.right;
                            first = false;
                        }
                        m_ChildMinWidth = Mathf.Max(i.minWidth + margins.horizontal, m_ChildMinWidth);
                        m_ChildMaxWidth = Mathf.Max(i.maxWidth + margins.horizontal, m_ChildMaxWidth);
                    }
                    m_StretchableCountX += i.stretchWidth;
                }
                // Before, we added the margins to the width, now we want to suptract them again.
                m_ChildMinWidth -= leftMarginMin + rightMarginMin;
                m_ChildMaxWidth -= leftMarginMin + rightMarginMin;
            }
            else
            {
                int lastMargin = 0;
                foreach (LayoutEntry i in entries)
                {
                    i.CalcWidth();
                    RectOffset m = i.margin;
                    int margin;

                    // Specialcase spaceStyle - instead of handling margins normally, we just want to insert the size...
                    // This ensure that Space(1) adds ONE space, and doesn't prevent margin collapses
                    if (i.style != LayoutUtility.spaceStyle)
                    {
                        if (!first)
                            margin = lastMargin > m.left ? lastMargin : m.left;
                        else
                        {
                            // the first element's margins are handles _leftMarginMin and should not be added to the children's sizes
                            margin = 0;
                            first = false;
                        }
                        m_ChildMinWidth += i.minWidth + spacing + margin;
                        m_ChildMaxWidth += i.maxWidth + spacing + margin;
                        lastMargin = m.right;
                        m_StretchableCountX += i.stretchWidth;
                    }
                    else
                    {
                        m_ChildMinWidth += i.minWidth;
                        m_ChildMaxWidth += i.maxWidth;
                        m_StretchableCountX += i.stretchWidth;
                    }
                }
                m_ChildMinWidth -= spacing;
                m_ChildMaxWidth -= spacing;
                if (entries.Count != 0)
                {
                    leftMarginMin = entries[0].margin.left;
                    rightMarginMin = lastMargin;
                }
                else
                {
                    leftMarginMin = rightMarginMin = 0;
                }
            }
            // Catch the cases where we have ONLY space elements in a group

            // calculated padding values.
            float leftPadding = 0;
            float rightPadding = 0;

            // If we have a style, the margins are handled i.r.t. padding.
            if (style != GUIStyle.none || m_UserSpecifiedWidth)
            {
                // Add the padding of this group to the total min & max widths
                leftPadding = Mathf.Max(style.padding.left, leftMarginMin);
                rightPadding = Mathf.Max(style.padding.right, rightMarginMin);
            }
            else
            {
                // If we don't have a GUIStyle, we pop the min of margins outward from children on to us.
                m_Margin.left = leftMarginMin;
                m_Margin.right = rightMarginMin;
                leftPadding = rightPadding = 0;
            }

            // If we have a specified minwidth, take that into account...
            minWidth = Mathf.Max(minWidth, m_ChildMinWidth + leftPadding + rightPadding);

            if (maxWidth == 0)          // if we don't have a max width, take the one that was calculated
            {
                stretchWidth += m_StretchableCountX + (style.stretchWidth ? 1 : 0);
                maxWidth = m_ChildMaxWidth + leftPadding + rightPadding;
            }
            else
            {
                // Since we have a maximum width, this element can't stretch width.
                stretchWidth = 0;
            }
            // Finally, if our minimum width is greater than our maximum width, minWidth wins
            maxWidth = Mathf.Max(maxWidth, minWidth);

            // If the style sets us to be a fixed width that wins completely
            if (style.fixedWidth != 0)
            {
                maxWidth = minWidth = style.fixedWidth;
                stretchWidth = 0;
            }
        }

        public override void SetHorizontal(float x, float width)
        {
            base.SetHorizontal(x, width);

            if (resetCoords)
                x = 0;

            RectOffset padding = style.padding;

            if (isVertical)
            {
                // If we have a GUIStyle here, spacing from our edges to children are max (our padding, their margins)
                if (style != GUIStyle.none)
                {
                    foreach (LayoutEntry i in entries)
                    {
                        // NOTE: we can't use .horizontal here (As that could make things like right button margin getting eaten by large left padding - so we need to split up in left and right
                        float leftMar = Mathf.Max(i.margin.left, padding.left);
                        float thisX = x + leftMar;
                        float thisWidth = width - Mathf.Max(i.margin.right, padding.right) - leftMar;
                        if (i.stretchWidth != 0)
                            i.SetHorizontal(thisX, thisWidth);
                        else
                            i.SetHorizontal(thisX, Mathf.Clamp(thisWidth, i.minWidth, i.maxWidth));
                    }
                }
                else
                {
                    // If not, PART of the subelements' margins have already been propagated upwards to this group, so we need to subtract that  from what we apply
                    float thisX = x - margin.left;
                    float thisWidth = width + margin.horizontal;
                    foreach (LayoutEntry i in entries)
                    {
                        if (i.stretchWidth != 0)
                        {
                            i.SetHorizontal(thisX + i.margin.left, thisWidth - i.margin.horizontal);
                        }
                        else
                            i.SetHorizontal(thisX + i.margin.left, Mathf.Clamp(thisWidth - i.margin.horizontal, i.minWidth, i.maxWidth));
                    }
                }
            }
            else
            {  // we're horizontally laid out:
               // apply margins/padding here
               // If we have a style, adjust the sizing to take care of padding (if we don't the horizontal margins have been propagated fully up the hierarchy)...
                if (style != GUIStyle.none)
                {
                    float leftMar = padding.left, rightMar = padding.right;
                    if (entries.Count != 0)
                    {
                        leftMar = Mathf.Max(leftMar, entries[0].margin.left);
                        rightMar = Mathf.Max(rightMar, entries[entries.Count - 1].margin.right);
                    }
                    x += leftMar;
                    width -= rightMar + leftMar;
                }

                // Find out how much leftover width we should distribute.
                float widthToDistribute = width - spacing * (entries.Count - 1);
                // Where to place us in height between min and max
                float minMaxScale = 0;
                // How much height to add to stretchable elements
                if (m_ChildMinWidth != m_ChildMaxWidth)
                    minMaxScale = Mathf.Clamp((widthToDistribute - m_ChildMinWidth) / (m_ChildMaxWidth - m_ChildMinWidth), 0, 1);

                // Handle stretching
                float perItemStretch = 0;
                if (widthToDistribute > m_ChildMaxWidth) // If we have too much space, we need to distribute it.
                {
                    if (m_StretchableCountX > 0)
                    {
                        perItemStretch = (widthToDistribute - m_ChildMaxWidth) / m_StretchableCountX;
                    }
                }

                // Set the positions
                int lastMargin = 0;
                bool firstMargin = true;
                //          Debug.Log ("" + x + ", " + width + "   perItemStretch:" + perItemStretch);
                //          Debug.Log ("MinMaxScale"+ minMaxScale);
                foreach (LayoutEntry i in entries)
                {
                    float thisWidth = Mathf.Lerp(i.minWidth, i.maxWidth, minMaxScale);
                    //              Debug.Log (i.minWidth);
                    thisWidth += perItemStretch * i.stretchWidth;

                    if (i.style != LayoutUtility.spaceStyle) // Skip margins on spaces.
                    {
                        int leftMargin = i.margin.left;
                        if (firstMargin)
                        {
                            leftMargin = 0;
                            firstMargin = false;
                        }
                        int margin = lastMargin > leftMargin ? lastMargin : leftMargin;
                        x += margin;
                        lastMargin = i.margin.right;
                    }

                    i.SetHorizontal(Mathf.Round(x), Mathf.Round(thisWidth));
                    x += thisWidth + spacing;
                }
            }
        }

        public override void CalcHeight()
        {
            if (entries.Count == 0)
            {
                maxHeight = minHeight = style.padding.vertical;
                return;
            }

            int topMarginMin = 0;
            int bottomMarginMin = 0;

            m_ChildMinHeight = 0;
            m_ChildMaxHeight = 0;
            m_StretchableCountY = 0;

            if (isVertical)
            {
                int lastMargin = 0;
                bool first = true;
                foreach (LayoutEntry i in entries)
                {
                    i.CalcHeight();
                    RectOffset m = i.margin;
                    int margin;

                    // Specialcase spaces - it's a space, so instead of handling margins normally, we just want to insert the size...
                    // This ensure that Space(1) adds ONE space, and doesn't prevent margin collapses
                    if (i.style != LayoutUtility.spaceStyle)
                    {
                        if (!first)
                            margin = Mathf.Max(lastMargin, m.top);
                        else
                        {
                            margin = 0;
                            first = false;
                        }

                        m_ChildMinHeight += i.minHeight + spacing + margin;
                        m_ChildMaxHeight += i.maxHeight + spacing + margin;
                        lastMargin = m.bottom;
                        m_StretchableCountY += i.stretchHeight;
                    }
                    else
                    {
                        m_ChildMinHeight += i.minHeight;
                        m_ChildMaxHeight += i.maxHeight;
                        m_StretchableCountY += i.stretchHeight;
                    }
                }

                m_ChildMinHeight -= spacing;
                m_ChildMaxHeight -= spacing;
                if (entries.Count != 0)
                {
                    topMarginMin = entries[0].margin.top;
                    bottomMarginMin = lastMargin;
                }
                else
                {
                    bottomMarginMin = topMarginMin = 0;
                }
            }
            else
            {
                bool first = true;
                foreach (LayoutEntry i in entries)
                {
                    i.CalcHeight();
                    RectOffset margins = i.margin;
                    if (i.style != LayoutUtility.spaceStyle)
                    {
                        if (!first)
                        {
                            topMarginMin = Mathf.Min(margins.top, topMarginMin);
                            bottomMarginMin = Mathf.Min(margins.bottom, bottomMarginMin);
                        }
                        else
                        {
                            topMarginMin = margins.top;
                            bottomMarginMin = margins.bottom;
                            first = false;
                        }
                        m_ChildMinHeight = Mathf.Max(i.minHeight, m_ChildMinHeight);
                        m_ChildMaxHeight = Mathf.Max(i.maxHeight, m_ChildMaxHeight);
                    }
                    m_StretchableCountY += i.stretchHeight;
                }
            }
            float firstPadding = 0;
            float lastPadding = 0;

            // If we have a style, the margins are handled i.r.t. padding.
            if (style != GUIStyle.none || m_UserSpecifiedHeight)
            {
                // Add the padding of this group to the total min & max widths
                firstPadding = Mathf.Max(style.padding.top, topMarginMin);
                lastPadding = Mathf.Max(style.padding.bottom, bottomMarginMin);
            }
            else
            {
                // If we don't have a GUIStyle, we bubble the margins outward from children on to us.
                m_Margin.top = topMarginMin;
                m_Margin.bottom = bottomMarginMin;
                firstPadding = lastPadding = 0;
            }
            //Debug.Log ("Margins: " + _topMarginMin + ", " + _bottomMarginMin + "          childHeights:" + childMinHeight + ", " + childMaxHeight);
            // If we have a specified minheight, take that into account...
            minHeight = Mathf.Max(minHeight, m_ChildMinHeight + firstPadding + lastPadding);

            if (maxHeight == 0)         // if we don't have a max height, take the one that was calculated
            {
                stretchHeight += m_StretchableCountY + (style.stretchHeight ? 1 : 0);
                maxHeight = m_ChildMaxHeight + firstPadding + lastPadding;
            }
            else
            {
                // Since we have a maximum height, this element can't stretch height.
                stretchHeight = 0;
            }
            // Finally, if out minimum height is greater than our maximum height, minHeight wins
            maxHeight = Mathf.Max(maxHeight, minHeight);

            // If the style sets us to be a fixed height
            if (style.fixedHeight != 0)
            {
                maxHeight = minHeight = style.fixedHeight;
                stretchHeight = 0;
            }
        }

        public override void SetVertical(float y, float height)
        {
            base.SetVertical(y, height);

            if (entries.Count == 0)
                return;

            RectOffset padding = style.padding;

            if (resetCoords)
                y = 0;

            if (isVertical)
            {
                // If we have a skin, adjust the sizing to take care of padding (if we don't have a skin the vertical margins have been propagated fully up the hierarchy)...
                if (style != GUIStyle.none)
                {
                    float topMar = padding.top, bottomMar = padding.bottom;
                    if (entries.Count != 0)
                    {
                        topMar = Mathf.Max(topMar, entries[0].margin.top);
                        bottomMar = Mathf.Max(bottomMar, entries[entries.Count - 1].margin.bottom);
                    }
                    y += topMar;
                    height -= bottomMar + topMar;
                }

                // Find out how much leftover height we should distribute.
                float heightToDistribute = height - spacing * (entries.Count - 1);
                // Where to place us in height between min and max
                float minMaxScale = 0;
                // How much height to add to stretchable elements
                if (m_ChildMinHeight != m_ChildMaxHeight)
                    minMaxScale = Mathf.Clamp((heightToDistribute - m_ChildMinHeight) / (m_ChildMaxHeight - m_ChildMinHeight), 0, 1);

                // Handle stretching
                float perItemStretch = 0;
                if (heightToDistribute > m_ChildMaxHeight)          // If we have too much space - stretch any stretchable children
                {
                    if (m_StretchableCountY > 0)
                        perItemStretch = (heightToDistribute - m_ChildMaxHeight) / m_StretchableCountY;
                }

                // Set the positions
                int lastMargin = 0;
                bool firstMargin = true;
                foreach (LayoutEntry i in entries)
                {
                    float thisHeight = Mathf.Lerp(i.minHeight, i.maxHeight, minMaxScale);
                    thisHeight += perItemStretch * i.stretchHeight;

                    if (i.style != LayoutUtility.spaceStyle)
                    {   // Skip margins on spaces.
                        int topMargin = i.margin.top;
                        if (firstMargin)
                        {
                            topMargin = 0;
                            firstMargin = false;
                        }
                        int margin = lastMargin > topMargin ? lastMargin : topMargin;
                        y += margin;
                        lastMargin = i.margin.bottom;
                    }
                    i.SetVertical(Mathf.Round(y), Mathf.Round(thisHeight));
                    y += thisHeight + spacing;
                }
            }
            else
            {
                // If we have a GUIStyle here, we need to respect the subelements' margins
                if (style != GUIStyle.none)
                {
                    foreach (LayoutEntry i in entries)
                    {
                        float topMar = Mathf.Max(i.margin.top, padding.top);
                        float thisY = y + topMar;
                        float thisHeight = height - Mathf.Max(i.margin.bottom, padding.bottom) - topMar;

                        if (i.stretchHeight != 0)
                            i.SetVertical(thisY, thisHeight);
                        else
                            i.SetVertical(thisY, Mathf.Clamp(thisHeight, i.minHeight, i.maxHeight));
                    }
                }
                else
                {
                    // If not, the subelements' margins have already been propagated upwards to this group, so we can safely ignore them
                    float thisY = y - margin.top;
                    float thisHeight = height + margin.vertical;
                    foreach (LayoutEntry i in entries)
                    {
                        if (i.stretchHeight != 0)
                            i.SetVertical(thisY + i.margin.top, thisHeight - i.margin.vertical);
                        else
                            i.SetVertical(thisY + i.margin.top, Mathf.Clamp(thisHeight - i.margin.vertical, i.minHeight, i.maxHeight));
                    }
                }
            }
        }

        public override string ToString()
        {
            string str = "", space = "";
            for (int i = 0; i < indent; i++)
                space += " ";
            str += /* space + */ base.ToString() + " Margins: " + m_ChildMinHeight + " {\n";
            indent += 4;
            foreach (LayoutEntry i in entries)
            {
                str += i.ToString() + "\n";
            }
            str += space + "}";
            indent -= 4;
            return str;
        }
    }

    public class LayoutEntry : IFlushable
    {
        // The min and max sizes. Used during calculations...
        public float minWidth, maxWidth, minHeight, maxHeight;

        // The rectangle that this element ends up having
        public Rect rect = new Rect(0, 0, 0, 0);

        // Can this element stretch?
        public float stretchWidth, stretchHeight;

        // The style to use.
        GUIStyle m_Style = GUIStyle.none;

        public GUIStyle style
        {
            get { return m_Style; }
            set { m_Style = value; ApplyStyleSettings(value); }
        }

        public static Rect kDummyRect = new Rect(0, 0, 1, 1);

        // The margins of this element.
        public virtual RectOffset margin { get { return style.margin; } }

        bool IFlushable.IsFlushed { get; set; }

        public static LayoutEntry Get(float _minWidth, float _maxWidth, float _minHeight, float _maxHeight, GUIStyle _style)
        {
            var lyt = MemPool<LayoutEntry>.Get();
            lyt.minWidth = _minWidth;
            lyt.maxWidth = _maxWidth;
            lyt.minHeight = _minHeight;
            lyt.maxHeight = _maxHeight;
            if (_style == null)
                _style = GUIStyle.none;
            lyt.style = _style;
            return lyt;
        }

        public static LayoutEntry Get(float _minWidth, float _maxWidth, float _minHeight, float _maxHeight, GUIStyle _style, LayoutOption[] options)
        {
            var lyt = MemPool<LayoutEntry>.Get();
            lyt.minWidth = _minWidth;
            lyt.maxWidth = _maxWidth;
            lyt.minHeight = _minHeight;
            lyt.maxHeight = _maxHeight;
            if (_style == null)
                _style = GUIStyle.none;
            lyt.style = _style;
            lyt.ApplyOptions(options);
            return lyt;
        }

        public virtual void CalcWidth() { }
        public virtual void CalcHeight() { }
        public virtual void SetHorizontal(float x, float width) { rect.x = x; rect.width = width; }
        public virtual void SetVertical(float y, float height) { rect.y = y; rect.height = height; }

        protected virtual void ApplyStyleSettings(GUIStyle style)
        {
            stretchWidth = (style.fixedWidth == 0 && style.stretchWidth) ? 1 : 0;
            stretchHeight = (style.fixedHeight == 0 && style.stretchHeight) ? 1 : 0;
            m_Style = style;
        }

        public virtual void ApplyOptions(LayoutOption[] options)
        {
            if (options == null)
                return;

            // Changed implementation from original UT's way handling this:
            // If user only specifies MinWidth, then MaxWidth still assumes to be Infinite 
            // (instead of the same with MinWidth).
            // This avoid having ugly workaround like Layout.MaxWidth(999999)

            foreach (LayoutOption i in options)
            {
                switch (i.type)
                {
                    case LayoutOption.Type.fixedWidth: minWidth = maxWidth = i.value; stretchWidth = 0; break;
                    case LayoutOption.Type.fixedHeight: minHeight = maxHeight = i.value; stretchHeight = 0; break;
                    case LayoutOption.Type.minWidth: minWidth = i.value; if (maxWidth == 0) maxWidth = float.PositiveInfinity; if (maxWidth < minWidth) maxWidth = minWidth; break;
                    case LayoutOption.Type.maxWidth: maxWidth = i.value; if (minWidth > maxWidth) minWidth = maxWidth; stretchWidth = 0; break;
                    case LayoutOption.Type.minHeight: minHeight = i.value; if (maxHeight < minHeight) maxHeight = minHeight; break;
                    case LayoutOption.Type.maxHeight: maxHeight = i.value; if (maxHeight == 0) maxHeight = float.PositiveInfinity; if (minHeight > maxHeight) minHeight = maxHeight; stretchHeight = 0; break;
                    case LayoutOption.Type.stretchWidth: stretchWidth = i.value; break;
                    case LayoutOption.Type.stretchHeight: stretchHeight = i.value; break;
                }
            }

            if (maxWidth != 0 && maxWidth < minWidth)
                maxWidth = minWidth;
            if (maxHeight != 0 && maxHeight < minHeight)
                maxHeight = minHeight;
        }

        protected static int indent = 0;
        public override string ToString()
        {
            string space = "";
            for (int i = 0; i < indent; i++)
                space += " ";
            return space + String.Format("{1}-{0} (x:{2}-{3}, y:{4}-{5})", style != null ? style.name : "NULL", GetType(), rect.x, rect.xMax, rect.y, rect.yMax) +
                "   -   W: " + minWidth + "-" + maxWidth + (stretchWidth != 0 ? "+" : "") + ", H: " + minHeight + "-" + maxHeight + (stretchHeight != 0 ? "+" : "");
        }

        public virtual void Flush()
        {
            FlushBase();
            MemPool<LayoutEntry>.Release(this);
        }

        public virtual void FlushBase()
        {
            minWidth = maxWidth = minHeight = maxHeight = stretchHeight = stretchWidth = 0;

            m_Style = GUIStyle.none;

            rect = new Rect();
        }
    }

    // Layouter that makes elements which sizes will always conform to a specific aspect ratio.
    public sealed class AspectSizer : LayoutEntry
    {
        float aspect;

        public static AspectSizer Get(float aspect, LayoutOption[] options)
        {
            var lyt = MemPool<AspectSizer>.Get();
            lyt.aspect = aspect;
            lyt.style = GUIStyle.none;
            lyt.ApplyOptions(options);
            return lyt;
        }

        public override void CalcHeight()
        {
            minHeight = maxHeight = rect.width / aspect;
        }

        public override void Flush()
        {
            FlushBase();
            MemPool<AspectSizer>.Release(this);
        }
    }

    // Will layout a button grid so it can fit within the given rect.
    // *undocumented*
    public sealed class GridSizer : LayoutEntry
    {
        // Helper: Create the layout group and scale it to fit
        public static Rect GetRect(GUIContent[] contents, int xCount, GUIStyle style, LayoutOption[] options)
        {
            Rect r = new Rect(0, 0, 0, 0);
            switch (Event.current.type)
            {
                case EventType.Layout:
                    LayoutUtility.current.topLevel.Add(GridSizer.Get(contents, xCount, style, options));
                    break;
                case EventType.Used:
                    return kDummyRect;
                default:
                    r = LayoutUtility.current.topLevel.GetNext().rect;
                    break;
            }
            return r;
        }

        int m_Count;
        int m_XCount;
        float m_MinButtonWidth = -1;
        float m_MaxButtonWidth = -1;
        float m_MinButtonHeight = -1;
        float m_MaxButtonHeight = -1;

        private static GridSizer Get(GUIContent[] contents, int xCount, GUIStyle buttonStyle, LayoutOption[] options)
        {
            var lyt = MemPool<GridSizer>.Get();
            lyt.Init(contents, xCount, buttonStyle, options);
            return lyt;
        }


        private void Init(GUIContent[] contents, int xCount, GUIStyle buttonStyle, LayoutOption[] options)
        {

            m_Count = contents.Length;
            m_XCount = xCount;

            // Most settings comes from the button style (can we stretch, etc). Hence, I apply the style here
            ApplyStyleSettings(buttonStyle);

            // We can have custom options coming from userland. We apply this last so it overrides
            ApplyOptions(options);

            if (xCount == 0 || contents.Length == 0)
                return;

            // public horizontal spacing
            float totalHorizSpacing = Mathf.Max(buttonStyle.margin.left, buttonStyle.margin.right) * (m_XCount - 1);
            //          Debug.Log (String.Format ("margins: {0}, {1}   totalHoriz: {2}", buttonStyle.margin.left, buttonStyle.margin.right, totalHorizSpacing));
            // public horizontal margins
            float totalVerticalSpacing = Mathf.Max(buttonStyle.margin.top, buttonStyle.margin.bottom) * (rows - 1);


            // Handle fixedSize buttons
            if (buttonStyle.fixedWidth != 0)
                m_MinButtonWidth = m_MaxButtonWidth = buttonStyle.fixedWidth;
            //          Debug.Log ("buttonStyle.fixedHeight " + buttonStyle.fixedHeight);
            if (buttonStyle.fixedHeight != 0)
                m_MinButtonHeight = m_MaxButtonHeight = buttonStyle.fixedHeight;

            // Apply GUILayout.Width/Height/whatever properties.
            if (m_MinButtonWidth == -1)
            {
                if (minWidth != 0)
                    m_MinButtonWidth = (minWidth - totalHorizSpacing) / m_XCount;
                if (maxWidth != 0)
                    m_MaxButtonWidth = (maxWidth - totalHorizSpacing) / m_XCount;
            }

            if (m_MinButtonHeight == -1)
            {
                if (minHeight != 0)
                    m_MinButtonHeight = (minHeight - totalVerticalSpacing) / rows;
                if (maxHeight != 0)
                    m_MaxButtonHeight = (maxHeight - totalVerticalSpacing) / rows;
            }
            //          Debug.Log (String.Format ("minButtonWidth {0}, maxButtonWidth {1}, minButtonHeight {2}, maxButtonHeight{3}", minButtonWidth, maxButtonWidth, minButtonHeight, maxButtonHeight));

            // if anything is left unknown, we need to iterate over all elements and figure out the sizes.
            if (m_MinButtonHeight == -1 || m_MaxButtonHeight == -1 || m_MinButtonWidth == -1 || m_MaxButtonWidth == -1)
            {
                // figure out the max size. Since the buttons are in a grid, the max size determines stuff.
                float calcHeight = 0, calcWidth = 0;
                foreach (GUIContent i in contents)
                {
                    Vector2 size = buttonStyle.CalcSize(i);
                    calcWidth = Mathf.Max(calcWidth, size.x);
                    calcHeight = Mathf.Max(calcHeight, size.y);
                }

                // If the user didn't supply minWidth, we need to calculate that
                if (m_MinButtonWidth == -1)
                {
                    // if the user has supplied a maxButtonWidth, the buttons can never get larger.
                    if (m_MaxButtonWidth != -1)
                        m_MinButtonWidth = Mathf.Min(calcWidth, m_MaxButtonWidth);
                    else
                        m_MinButtonWidth = calcWidth;
                }

                // If the user didn't supply maxWidth, we need to calculate that
                if (m_MaxButtonWidth == -1)
                {
                    // if the user has supplied a minButtonWidth, the buttons can never get smaller.
                    if (m_MinButtonWidth != -1)
                        m_MaxButtonWidth = Mathf.Max(calcWidth, m_MinButtonWidth);
                    else
                        m_MaxButtonWidth = calcWidth;
                }

                // If the user didn't supply minWidth, we need to calculate that
                if (m_MinButtonHeight == -1)
                {
                    // if the user has supplied a maxButtonWidth, the buttons can never get larger.
                    if (m_MaxButtonHeight != -1)
                        m_MinButtonHeight = Mathf.Min(calcHeight, m_MaxButtonHeight);
                    else
                        m_MinButtonHeight = calcHeight;
                }

                // If the user didn't supply maxWidth, we need to calculate that
                if (m_MaxButtonHeight == -1)
                {
                    // if the user has supplied a minButtonWidth, the buttons can never get smaller.
                    if (m_MinButtonHeight != -1)
                        maxHeight = Mathf.Max(maxHeight, m_MinButtonHeight);
                    m_MaxButtonHeight = maxHeight;
                }
            }
            // We now know the button sizes. Calculate min & max values from that
            minWidth = m_MinButtonWidth * m_XCount + totalHorizSpacing;
            maxWidth = m_MaxButtonWidth * m_XCount + totalHorizSpacing;
            minHeight = m_MinButtonHeight * rows + totalVerticalSpacing;
            maxHeight = m_MaxButtonHeight * rows + totalVerticalSpacing;
            //          Debug.Log (String.Format ("minWidth {0}, maxWidth {1}, minHeight {2}, maxHeight{3}", minWidth, maxWidth, minHeight, maxHeight));
        }

        int rows
        {
            get
            {
                int rows = m_Count / m_XCount;
                if (m_Count % m_XCount != 0)
                    rows++;
                return rows;
            }
        }
    }

    // Class that can handle word-wrap sizing. this is specialcased as setting width can make the text wordwrap, which would then increase height...
    public sealed class WordWrapSizer : LayoutEntry
    {
        readonly GUIContent m_Content = new GUIContent();
        // We need to differentiate between min & maxHeight we calculate for ourselves and one that is forced by the user
        // (When inside a scrollview, we can be told to layout twice, so we need to know the difference)
        float m_ForcedMinHeight;
        float m_ForcedMaxHeight;

        public static WordWrapSizer Get(GUIStyle style, GUIContent content, LayoutOption[] options)
        {
            var lyt = MemPool<WordWrapSizer>.Get();
            lyt.style = style ?? GUIStyle.none;
            lyt.m_Content.text = content.text;
            lyt.m_Content.image = content.image;
            lyt.m_Content.tooltip = content.tooltip;
            lyt.ApplyOptions(options);
            lyt.m_ForcedMinHeight = lyt.minHeight;
            lyt.m_ForcedMaxHeight = lyt.maxHeight;
            return lyt;
        }

        public override void Flush()
        {
            FlushBase();
            MemPool<WordWrapSizer>.Release(this);
        }

        public override void FlushBase()
        {
            base.FlushBase();
            m_ForcedMaxHeight = 0;
            m_ForcedMinHeight = 0;
            m_Content.text = null;
            m_Content.image = null;
            m_Content.tooltip = null;
        }

        public override void CalcWidth()
        {
            if (minWidth == 0 || maxWidth == 0)
            {
                float _minWidth, _maxWidth;
                style.CalcMinMaxWidth(m_Content, out _minWidth, out _maxWidth);
                if (minWidth == 0)
                    minWidth = _minWidth;
                if (maxWidth == 0)
                    maxWidth = _maxWidth;
            }
        }

        public override void CalcHeight()
        {
            // When inside a scrollview, this can get called twice (as vertical scrollbar reduces width, which causes a reflow).
            // Hence, we need to use the separately cached values for min & maxHeight coming from the user...
            if (m_ForcedMinHeight == 0 || m_ForcedMaxHeight == 0)
            {
                float height = style.CalcHeight(m_Content, rect.width);
                if (m_ForcedMinHeight == 0)
                    minHeight = height;
                else
                    minHeight = m_ForcedMinHeight;

                if (m_ForcedMaxHeight == 0)
                    maxHeight = height;
                else
                    maxHeight = m_ForcedMaxHeight;
            }
        }
    }

    // Class publicly used to pass layout options into [[GUILayout]] functions. You don't use these directly, but construct them with the layouting functions in the [[GUILayout]] class.
    public struct LayoutOption
    {
        public enum Type
        {
            fixedWidth, fixedHeight, minWidth, maxWidth, minHeight, maxHeight, stretchWidth, stretchHeight,
            // These are just for the spacing variables
            alignStart, alignMiddle, alignEnd, alignJustify, equalSize, spacing
        }
        // *undocumented*
        public Type type;
        // *undocumented*
        public float value;
        // *undocumented*
        public LayoutOption(Type type, float value)
        {
            this.type = type;
            this.value = value;
        }
    }


    // Layout controller for content inside scroll views
    public sealed class ScrollGroup : LayoutGroup
    {
        public float calcMinWidth, calcMaxWidth, calcMinHeight, calcMaxHeight;
        public float clientWidth, clientHeight;
        public bool allowHorizontalScroll = true;
        public bool allowVerticalScroll = true;
        public bool needsHorizontalScrollbar, needsVerticalScrollbar;
        public GUIStyle horizontalScrollbar, verticalScrollbar;

        public static new ScrollGroup Get()
        {
            return MemPool<ScrollGroup>.Get();
        }

        public override void Flush()
        {
            FlushBase();
            MemPool<ScrollGroup>.Release(this);
        }

        public override void CalcWidth()
        {
            // Save the size values & reset so we calc the sizes of children without any contraints
            float _minWidth = minWidth;
            float _maxWidth = maxWidth;
            if (allowHorizontalScroll)
            {
                minWidth = 0;
                maxWidth = 0;
            }

            base.CalcWidth();
            calcMinWidth = minWidth;
            calcMaxWidth = maxWidth;

            // restore the stored constraints for our parent's sizing
            if (allowHorizontalScroll)
            {
                // Set an explicit small minWidth so it will correctly scroll when place inside horizontal groups
                if (minWidth > 32)
                    minWidth = 32;

                if (_minWidth != 0)
                    minWidth = _minWidth;
                if (_maxWidth != 0)
                {
                    maxWidth = _maxWidth;
                    stretchWidth = 0;
                }
            }
        }

        public override void SetHorizontal(float x, float width)
        {
            float _cWidth = needsVerticalScrollbar ? width - verticalScrollbar.fixedWidth - verticalScrollbar.margin.left : width;
            //if (allowVerticalScroll == false)
            //  Debug.Log ("width " + width);
            // If we get a vertical scrollbar, the width changes, so we need to do a recalculation with the new width.
            if (allowHorizontalScroll && _cWidth < calcMinWidth)
            {
                // We're too small horizontally, so we need a horizontal scrollbar.
                needsHorizontalScrollbar = true;

                // set the min and max width we calculated for the children so SetHorizontal works correctly
                minWidth = calcMinWidth;
                maxWidth = calcMaxWidth;
                base.SetHorizontal(x, calcMinWidth);

                // SetHorizontal also sets our width, but we know better
                rect.width = width;

                clientWidth = calcMinWidth;
            }
            else
            {
                // Got enough space.
                needsHorizontalScrollbar = false;

                // set the min and max width we calculated for the children so SetHorizontal works correctly
                if (allowHorizontalScroll)
                {
                    minWidth = calcMinWidth;
                    maxWidth = calcMaxWidth;
                }
                base.SetHorizontal(x, _cWidth);
                rect.width = width;

                // Store the client width
                clientWidth = _cWidth;
            }
        }

        public override void CalcHeight()
        {
            // Save the values & reset so we calc the sizes of children without any contraints
            float _minHeight = minHeight;
            float _maxHeight = maxHeight;
            if (allowVerticalScroll)
            {
                minHeight = 0;
                maxHeight = 0;
            }

            base.CalcHeight();

            calcMinHeight = minHeight;
            calcMaxHeight = maxHeight;

            // if we KNOW we need a horizontal scrollbar, claim space for it now
            // otherwise we get a vertical scrollbar and leftover space beneath the scrollview.
            if (needsHorizontalScrollbar)
            {
                float scrollerSize = horizontalScrollbar.fixedHeight + horizontalScrollbar.margin.top;
                minHeight += scrollerSize;
                maxHeight += scrollerSize;
            }

            // restore the stored constraints from user SetHeight calls.
            if (allowVerticalScroll)
            {
                if (minHeight > 32)
                    minHeight = 32;

                if (_minHeight != 0)
                    minHeight = _minHeight;
                if (_maxHeight != 0)
                {
                    maxHeight = _maxHeight;
                    stretchHeight = 0;
                }
            }
        }

        public override void SetVertical(float y, float height)
        {
            // if we have a horizontal scrollbar, we have less space than we thought
            float availableHeight = height;
            if (needsHorizontalScrollbar)
                availableHeight -= horizontalScrollbar.fixedHeight + horizontalScrollbar.margin.top;

            // Now we know how much height we have, and hence how much vertical space to distribute.
            // If we get a vertical scrollbar, the width changes, so we need to do a recalculation with the new width.
            if (allowVerticalScroll && availableHeight < calcMinHeight)
            {
                // We're too small vertically, so we need a vertical scrollbar.
                // This means that we have less horizontal space, which can change the vertical size.
                if (!needsHorizontalScrollbar && !needsVerticalScrollbar)
                {
                    // Subtract scrollbar width from the size...
                    clientWidth = rect.width - verticalScrollbar.fixedWidth - verticalScrollbar.margin.left;

                    // ...But make sure we never get too small.
                    if (clientWidth < calcMinWidth)
                        clientWidth = calcMinWidth;

                    // Set the new (smaller) size.
                    float outsideWidth = rect.width;        // store a backup of our own width
                    SetHorizontal(rect.x, clientWidth);

                    // This can have caused a reflow, so we need to recalclate from here on down
                    // (we already know we need a vertical scrollbar, so this size change cannot bubble upwards.
                    CalcHeight();

                    rect.width = outsideWidth;
                }


                // set the min and max height we calculated for the children so SetVertical works correctly
                float origMinHeight = minHeight, origMaxHeight = maxHeight;
                minHeight = calcMinHeight;
                maxHeight = calcMaxHeight;
                base.SetVertical(y, calcMinHeight);
                minHeight = origMinHeight;
                maxHeight = origMaxHeight;

                rect.height = height;
                clientHeight = calcMinHeight;
            }
            else
            {
                // set the min and max height we calculated for the children so SetVertical works correctly
                if (allowVerticalScroll)
                {
                    minHeight = calcMinHeight;
                    maxHeight = calcMaxHeight;
                }
                base.SetVertical(y, availableHeight);
                rect.height = height;
                clientHeight = availableHeight;
            }
        }
    }


    public class LayoutFadeGroup : LayoutGroup
    {
        public float fadeValue;
        public bool wasGUIEnabled;
        public Color guiColor;

        public override void CalcHeight()
        {
            base.CalcHeight();
            minHeight *= fadeValue;
            maxHeight *= fadeValue;
        }

        public static new LayoutFadeGroup Get()
        {
            var lyt = MemPool<LayoutFadeGroup>.Get();
            return lyt;
        }

        public override void Flush()
        {
            FlushBase();
            MemPool<LayoutFadeGroup>.Release(this);
        }
    }
}

