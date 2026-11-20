#if UNITY_6000_7_OR_NEWER
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Puts the badge on rows of the UI Toolkit Hierarchy window (Unity 6.7 and 7). That window never calls the
    /// IMGUI row hook, so it gets a badge element in each row's right-hand container instead.
    /// </summary>
    internal static class HierarchyBadgeRows
    {
        /// <summary>USS class on every badge element. The probe and tests find badges by it.</summary>
        internal const string BadgeClass = "ap-hierarchy-badge";

        const float BadgeHeight = 14f;
        const float DotSize = 6f;

        // Rows on screen after a domain reload are bound again by the window, so nothing is drawn here yet.
        internal static void Register()
        {
            HierarchyWindow.BindViewItem += OnBindViewItem;
            HierarchyWindow.UnbindViewItem += OnUnbindViewItem;
        }

        /// <summary>
        /// Redraws the badges on rows already on screen. A row bound just before a change keeps its badge until it is
        /// bound again, so this runs right after every change. It runs now, not on a delayCall: on Unity 7000.0.0a16
        /// a delayCall queued here never ran, and the rows kept their old badge.
        /// </summary>
        internal static void RefreshAll()
        {
            foreach (HierarchyWindow window in Resources.FindObjectsOfTypeAll<HierarchyWindow>())
            {
                List<HierarchyViewItem> items = window.rootVisualElement.Query<HierarchyViewItem>().ToList();
                foreach (HierarchyViewItem item in items)
                {
                    Decorate(item);
                }
            }
        }

        static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            Decorate(item);
        }

        static void OnUnbindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            RemoveBadge(item);
        }

        // Rows are reused for other GameObjects as the Hierarchy scrolls, so every bind starts from no badge.
        static void Decorate(HierarchyViewItem item)
        {
            RemoveBadge(item);

            // Scene rows and other node types hand over no GameObject.
            GameObject gameObject = item.Handler is HierarchyGameObjectHandler handler ? handler.GetGameObject(in item.Node) : null;
            if (gameObject == null)
            {
                return;
            }

            HierarchyBadgeInfo badge = HierarchyBadgeCache.Get(gameObject);
            if (badge.Kind == HierarchyBadgeKind.None)
            {
                return;
            }
            item.RightCustomContainer.Add(badge.Kind == HierarchyBadgeKind.ChildrenSave ? CreateDot(badge.Tooltip) : CreateLabel(badge));
        }

        static void RemoveBadge(HierarchyViewItem item)
        {
            item.RightCustomContainer.Q(className: BadgeClass)?.RemoveFromHierarchy();
        }

        static VisualElement CreateLabel(HierarchyBadgeInfo badge)
        {
            Label label = new Label(HierarchySavedBadge.LabelFor(badge.Kind)) { tooltip = badge.Tooltip };
            label.AddToClassList(BadgeClass);
            label.style.width = HierarchySavedBadge.BadgeWidth();
            label.style.height = BadgeHeight;
            label.style.alignSelf = Align.Center;
            label.style.marginLeft = 2f;
            label.style.marginRight = 4f;
            label.style.paddingLeft = 0f;
            label.style.paddingRight = 0f;
            label.style.paddingTop = 0f;
            label.style.paddingBottom = 0f;
            label.style.fontSize = 9f;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = Color.white;
            label.style.backgroundColor = HierarchySavedBadge.ColorFor(badge.Kind);
            SetRadius(label.style, 3f);
            return label;
        }

        // The hover area is wider than the dot so its tooltip is easy to reach.
        static VisualElement CreateDot(string tooltip)
        {
            VisualElement hoverArea = new VisualElement { tooltip = tooltip };
            hoverArea.AddToClassList(BadgeClass);
            hoverArea.style.width = DotSize + 8f;
            hoverArea.style.height = BadgeHeight;
            hoverArea.style.alignSelf = Align.Center;
            hoverArea.style.alignItems = Align.Center;
            hoverArea.style.justifyContent = Justify.Center;
            hoverArea.style.marginRight = 2f;

            VisualElement dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.style.width = DotSize;
            dot.style.height = DotSize;
            dot.style.backgroundColor = HierarchySavedBadge.DotColor;
            SetRadius(dot.style, DotSize / 2f);
            hoverArea.Add(dot);
            return hoverArea;
        }

        static void SetRadius(IStyle style, float radius)
        {
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
        }
    }
}
#endif
