using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Draws the small badge at the right of a Hierarchy row: red for a setup problem, a neutral color for a
    /// Savable Bridge that is set up but hasn't saved or loaded anything yet, blue once it has saved data, green
    /// right after a restore, and a grey dot on a parent whose children have one of those badges. The classic
    /// Hierarchy window draws rows with IMGUI and calls the row hook below. The UI Toolkit Hierarchy window
    /// (Unity 6.7 and 7) never calls that hook, so <see cref="HierarchyBadgeRows"/> adds the badge to its rows instead.
    /// </summary>
    [InitializeOnLoad]
    internal static class HierarchySavedBadge
    {
        const float DotSize = 6f;
        const float RightMargin = 4f;

        internal static readonly Color SetupWrongColor = new Color(0.74f, 0.24f, 0.24f, 1f);
        internal static readonly Color SetUpColor = new Color(0.45f, 0.45f, 0.48f, 1f);
        internal static readonly Color SavedColor = new Color(0.24f, 0.46f, 0.64f, 1f);
        internal static readonly Color LoadedColor = new Color(0.30f, 0.55f, 0.33f, 1f);
        internal static readonly Color DotColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        // The widest label ("Setup wrong") decides every badge's width, so they all line up in the same column.
        const string WidestLabel = "Setup wrong";

        static GUIStyle badgeStyle;
        static float cachedBadgeWidth = -1f;

        static HierarchySavedBadge()
        {
            // Unity 7's Hierarchy hands out an EntityId. Unity 6.7 hands out an int, which is only turned
            // into an object here, never created from an object.
#if UNITY_7000_0_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += DrawRowByEntityId;
#else
            EditorApplication.hierarchyWindowItemOnGUI += DrawRowByInstanceId;
#endif
#if UNITY_6000_7_OR_NEWER
            HierarchyBadgeRows.Register();
#endif
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            ObjectChangeEvents.changesPublished += OnObjectChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SaveActivity.Changed += Forget;
        }

        static void OnHierarchyChanged()
        {
            Forget();
        }

        static void OnObjectChanged(ref ObjectChangeEventStream stream)
        {
            Forget();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            Forget();
        }

        /// <summary>Clears every cached badge, so the next repaint works each one out again.</summary>
        internal static void Forget()
        {
            HierarchyBadgeCache.Clear();
#if UNITY_6000_7_OR_NEWER
            HierarchyBadgeRows.RefreshAll();
#endif
        }

#if UNITY_7000_0_OR_NEWER
        static void DrawRowByEntityId(EntityId entityId, Rect rowRect)
        {
            DrawBadge(EditorUtility.EntityIdToObject(entityId) as GameObject, rowRect);
        }
#else
        static void DrawRowByInstanceId(int instanceId, Rect rowRect)
        {
            DrawBadge(EditorUtility.InstanceIDToObject(instanceId) as GameObject, rowRect);
        }
#endif

        static void DrawBadge(GameObject gameObject, Rect rowRect)
        {
            // Scene rows hand over no GameObject.
            bool isDrawing = Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove;
            if (gameObject == null || !isDrawing)
            {
                return;
            }

            HierarchyBadgeInfo badge = HierarchyBadgeCache.Get(gameObject);
            if (badge.Kind == HierarchyBadgeKind.None)
            {
                return;
            }

            if (badge.Kind == HierarchyBadgeKind.ChildrenSave)
            {
                DrawDot(rowRect, badge.Tooltip);
                return;
            }
            DrawLabel(rowRect, badge);
        }

        static void DrawLabel(Rect rowRect, HierarchyBadgeInfo badge)
        {
            float width = BadgeWidth();
            Rect badgeRect = new Rect(rowRect.xMax - width - RightMargin, rowRect.y + 2f, width, rowRect.height - 4f);
            EditorGUI.DrawRect(badgeRect, ColorFor(badge.Kind));
            GUI.Label(badgeRect, new GUIContent(LabelFor(badge.Kind), badge.Tooltip), BadgeStyle());
        }

        static void DrawDot(Rect rowRect, string tooltip)
        {
            float top = rowRect.y + (rowRect.height - DotSize) / 2f;
            Rect dotRect = new Rect(rowRect.xMax - DotSize - RightMargin - 2f, top, DotSize, DotSize);
            EditorGUI.DrawRect(dotRect, DotColor);
            GUI.Label(new Rect(dotRect.x - 3f, rowRect.y, DotSize + 6f, rowRect.height), new GUIContent(string.Empty, tooltip));
        }

        /// <summary>The label a badge kind shows. "Children save" is the dot's tooltip prefix, not a label, so it isn't here.</summary>
        internal static string LabelFor(HierarchyBadgeKind kind)
        {
            switch (kind)
            {
                case HierarchyBadgeKind.SetupWrong:
                    return "Setup wrong";
                case HierarchyBadgeKind.SetUp:
                    return "Set up";
                case HierarchyBadgeKind.Saved:
                    return "Saved";
                case HierarchyBadgeKind.Loaded:
                    return "Loaded";
                default:
                    return string.Empty;
            }
        }

        internal static Color ColorFor(HierarchyBadgeKind kind)
        {
            switch (kind)
            {
                case HierarchyBadgeKind.SetupWrong:
                    return SetupWrongColor;
                case HierarchyBadgeKind.Loaded:
                    return LoadedColor;
                case HierarchyBadgeKind.Saved:
                    return SavedColor;
                default:
                    return SetUpColor;
            }
        }

        // Measured once from the widest label, so every badge shares one width and the column doesn't jitter
        // as a row moves between kinds.
        internal static float BadgeWidth()
        {
            if (cachedBadgeWidth < 0f)
            {
                cachedBadgeWidth = BadgeStyle().CalcSize(new GUIContent(WidestLabel)).x + 8f;
            }
            return cachedBadgeWidth;
        }

        static GUIStyle BadgeStyle()
        {
            if (badgeStyle == null)
            {
                badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9
                };
                badgeStyle.normal.textColor = Color.white;
            }
            return badgeStyle;
        }
    }
}
