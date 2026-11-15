using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Draws the small "Saved" badge at the right of a Hierarchy row, red when a saved script there has a mistake,
    /// and a grey dot on a parent whose children save. The classic Hierarchy window draws rows with IMGUI and calls
    /// the row hook below. The UI Toolkit Hierarchy window (Unity 6.7 and 7) never calls that hook, so
    /// <see cref="HierarchyBadgeRows"/> adds the badge to its rows instead.
    /// </summary>
    [InitializeOnLoad]
    internal static class HierarchySavedBadge
    {
        const float BadgeWidth = 40f;
        const float DotSize = 6f;
        const float RightMargin = 4f;

        internal static readonly Color SavedColor = new Color(0.24f, 0.46f, 0.64f, 1f);
        internal static readonly Color MistakeColor = new Color(0.74f, 0.24f, 0.24f, 1f);
        internal static readonly Color DotColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        static GUIStyle badgeStyle;

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
        }

        static void OnHierarchyChanged()
        {
            Forget();
        }

        static void OnObjectChanged(ref ObjectChangeEventStream stream)
        {
            Forget();
        }

        // IMGUI rows redraw on their own. UI Toolkit rows keep their badge until they are bound again,
        // so the ones on screen are redrawn here.
        static void Forget()
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
            Rect badgeRect = new Rect(rowRect.xMax - BadgeWidth - RightMargin, rowRect.y + 2f, BadgeWidth, rowRect.height - 4f);
            Color color = badge.Kind == HierarchyBadgeKind.Mistake ? MistakeColor : SavedColor;
            EditorGUI.DrawRect(badgeRect, color);
            GUI.Label(badgeRect, new GUIContent("Saved", badge.Tooltip), BadgeStyle());
        }

        static void DrawDot(Rect rowRect, string tooltip)
        {
            float top = rowRect.y + (rowRect.height - DotSize) / 2f;
            Rect dotRect = new Rect(rowRect.xMax - DotSize - RightMargin - 2f, top, DotSize, DotSize);
            EditorGUI.DrawRect(dotRect, DotColor);
            GUI.Label(new Rect(dotRect.x - 3f, rowRect.y, DotSize + 6f, rowRect.height), new GUIContent(string.Empty, tooltip));
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
