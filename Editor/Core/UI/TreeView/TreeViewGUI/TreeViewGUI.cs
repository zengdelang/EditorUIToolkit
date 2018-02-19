using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public abstract class TreeViewGUI : ITreeViewGUI
    {
        protected PingData m_Ping = new PingData();
        private bool m_AnimateScrollBarOnExpandCollapse = true;
        protected float k_LineHeight = 16f;
        protected float k_BaseIndent = 2f;
        protected float k_IndentWidth = 14f;
        protected float k_FoldoutWidth = 14f;
        protected float k_IconWidth = 16f;
        protected float k_SpaceBetweenIconAndText = 2f;
        protected float k_HalfDropBetweenHeight = 4f;
        protected TreeView m_TreeView;
        protected Rect m_DraggingInsertionMarkerRect;
        protected bool m_UseHorizontalScroll;
        protected float k_TopRowMargin;
        protected float k_BottomRowMargin;
        protected static Styles s_Styles;

        public float iconLeftPadding { get; set; }

        public float iconRightPadding { get; set; }

        public float iconTotalPadding
        {
            get
            {
                return iconLeftPadding + iconRightPadding;
            }
        }

        public Action<TreeViewItem, Rect> iconOverlayGUI { get; set; }
        public Action BeginRenameAction { get; set; }

        protected float indentWidth
        {
            get
            {
                return k_IndentWidth + iconTotalPadding;
            }
        }

        public float halfDropBetweenHeight
        {
            get
            {
                return k_HalfDropBetweenHeight;
            }
        }

        public virtual float topRowMargin
        {
            get
            {
                return k_TopRowMargin;
            }
        }

        public virtual float bottomRowMargin
        {
            get
            {
                return k_BottomRowMargin;
            }
        }

        public TreeViewGUI(TreeView treeView)
        {
            m_TreeView = treeView;
        }

        public TreeViewGUI(TreeView treeView, bool useHorizontalScroll)
        {
            m_TreeView = treeView;
            m_UseHorizontalScroll = useHorizontalScroll;
        }

        public virtual void OnInitialize()
        {
        }

        protected virtual void InitStyles()
        {
            if (s_Styles != null)
                return;
            s_Styles = new Styles();
        }

        protected virtual Texture GetIconForItem(TreeViewItem item)
        {
            return item.icon;
        }

        public virtual Vector2 GetTotalSize()
        {
            InitStyles();
            float x = 1f;
            if (m_UseHorizontalScroll)
                x = GetMaxWidth(m_TreeView.data.GetRows());
            float y = m_TreeView.data.rowCount * k_LineHeight + topRowMargin + bottomRowMargin;
            if (m_AnimateScrollBarOnExpandCollapse && m_TreeView.expansionAnimator.isAnimating)
                y -= m_TreeView.expansionAnimator.deltaHeight;
            return new Vector2(x, y);
        }

        protected float GetMaxWidth(List<TreeViewItem> rows)
        {
            InitStyles();
            float num1 = 1f;
            using (List<TreeViewItem>.Enumerator enumerator = rows.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TreeViewItem current = enumerator.Current;
                    float num2 = 0.0f + GetContentIndent(current);
                    if (current.icon != null)
                        num2 += k_IconWidth;
                    float minWidth;
                    float maxWidth;
                    s_Styles.lineStyle.CalcMinMaxWidth(GUIContentWrap.Temp(current.displayName), out minWidth, out maxWidth);
                    float num3 = num2 + maxWidth + k_BaseIndent;
                    if (num3 > num1)
                        num1 = num3;
                }
            }
            return num1;
        }

        public virtual int GetNumRowsOnPageUpDown(TreeViewItem fromItem, bool pageUp, float heightOfTreeView)
        {
            return (int)Mathf.Floor(heightOfTreeView / k_LineHeight);
        }

        public virtual void GetFirstAndLastRowVisible(out int firstRowVisible, out int lastRowVisible)
        {
            if (m_TreeView.data.rowCount == 0)
            {
                firstRowVisible = lastRowVisible = -1;
            }
            else
            {
                float num = m_TreeView.state.scrollPos.y;
                float height = m_TreeView.GetTotalRect().height;
                firstRowVisible = (int)Mathf.Floor((num - topRowMargin) / k_LineHeight);
                lastRowVisible = firstRowVisible + (int)Mathf.Ceil(height / k_LineHeight);
                firstRowVisible = Mathf.Max(firstRowVisible, 0);
                lastRowVisible = Mathf.Min(lastRowVisible, m_TreeView.data.rowCount - 1);
                if (firstRowVisible < m_TreeView.data.rowCount || firstRowVisible <= 0)
                    return;
                m_TreeView.state.scrollPosY = 0.0f;
                GetFirstAndLastRowVisible(out firstRowVisible, out lastRowVisible);
            }
        }

        public virtual void BeginRowGUI()
        {
            InitStyles();
            m_DraggingInsertionMarkerRect.x = -1f;
            SyncFakeItem();
            if (Event.current.type == EventType.Repaint)
                return;
            DoRenameOverlay();
        }

        public virtual void EndRowGUI()
        {
            if (m_DraggingInsertionMarkerRect.x >= 0.0 && Event.current.type == EventType.Repaint)
            {
                if (m_TreeView.dragging.drawRowMarkerAbove)
                    s_Styles.insertionAbove.Draw(m_DraggingInsertionMarkerRect, false, false, false, false);
                else
                    s_Styles.insertion.Draw(m_DraggingInsertionMarkerRect, false, false, false, false);
            }
            if (Event.current.type == EventType.Repaint)
                DoRenameOverlay();
            HandlePing();
        }

        public virtual void OnRowGUI(Rect rowRect, TreeViewItem item, int row, bool selected, bool focused)
        {
            DoItemGUI(rowRect, row, item, selected, focused, false);
        }

        protected virtual void DoItemGUI(Rect rect, int row, TreeViewItem item, bool selected, bool focused, bool useBoldFont)
        {
            EditorGUIUtility.SetIconSize(new Vector2(k_IconWidth, k_IconWidth));
            float foldoutIndent = GetFoldoutIndent(item);
            int itemControlId = TreeView.GetItemControlID(item, m_TreeView);
            bool flag1 = false;
            if (m_TreeView.dragging != null)
                flag1 = m_TreeView.dragging.GetDropTargetControlID() == itemControlId && m_TreeView.data.CanBeParent(item);
            bool flag2 = IsRenaming(item.id);
            bool flag3 = m_TreeView.data.IsExpandable(item);
            if (flag2 && Event.current.type == EventType.Repaint)
            {
                float num1 = item.icon != null ? k_IconWidth : 0.0f;
                float num2 = (float)(foldoutIndent + k_FoldoutWidth + num1 + iconTotalPadding - 1.0) + 15f + 2;
                GetRenameOverlay().editFieldRect = new Rect(rect.x + num2, rect.y, rect.width - num2, rect.height);
            }
            if (Event.current.type == EventType.Repaint)
            {
                string label = item.displayName;
                if (flag2)
                {
                    selected = false;
                    label = string.Empty;
                }
                if (selected)
                    s_Styles.selectionStyle.Draw(rect, false, false, true, focused);
                if (flag1)
                    s_Styles.lineStyle.Draw(rect, GUIContent.none, true, true, false, false);
                DrawIconAndLabel(rect, item, label, selected, focused, useBoldFont, false);
                if (m_TreeView.dragging != null && m_TreeView.dragging.GetRowMarkerControlID() == itemControlId)
                    m_DraggingInsertionMarkerRect = new Rect(rect.x + foldoutIndent + k_FoldoutWidth, rect.y, rect.width - foldoutIndent, rect.height);
            }
            if (flag3)
                DoFoldout(rect, item, row);
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        private float GetTopPixelOfRow(int row)
        {
            return row * k_LineHeight + topRowMargin;
        }

        public virtual Rect GetRowRect(int row, float rowWidth)
        {
            return new Rect(0.0f, GetTopPixelOfRow(row), rowWidth, k_LineHeight);
        }

        public virtual Rect GetRectForFraming(int row)
        {
            return GetRowRect(row, 1f);
        }

        protected virtual Rect DoFoldout(Rect rowRect, TreeViewItem item, int row)
        {
            Rect position = new Rect(GetFoldoutIndent(item) + rowRect.x, rowRect.y, k_FoldoutWidth, rowRect.height);
            TreeViewItemExpansionAnimator expansionAnimator = m_TreeView.expansionAnimator;
            EditorGUI.BeginChangeCheck();
            bool expand;
            if (expansionAnimator.IsAnimating(item.id))
            {
                Matrix4x4 matrix = GUI.matrix;
                float num = Mathf.Min(1f, expansionAnimator.expandedValueNormalized * 2f);
                GUIUtility.RotateAroundPivot(expansionAnimator.isExpanding ? (float)((1.0 - num) * -90.0) : num * 90f, position.center);
                bool isExpanding = expansionAnimator.isExpanding;
                expand = GUI.Toggle(position, isExpanding, GUIContent.none, s_Styles.foldout);
                GUI.matrix = matrix;
            }
            else
                expand = GUI.Toggle(position, m_TreeView.data.IsExpanded(item), GUIContent.none, s_Styles.foldout);
            if (EditorGUI.EndChangeCheck())
                m_TreeView.UserInputChangedExpandedState(item, row, expand);
            return position;
        }

        protected virtual void DrawIconAndLabel(Rect rect, TreeViewItem item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
        {
            if (!isPinging)
            {
                float contentIndent = GetContentIndent(item);
                rect.x += contentIndent;
                rect.width -= contentIndent;
            }
            GUIStyle guiStyle = !useBoldFont ? s_Styles.lineStyle : s_Styles.lineBoldStyle;
            guiStyle.padding.left = (int)(k_IconWidth + iconTotalPadding + k_SpaceBetweenIconAndText);
            guiStyle.Draw(rect, label, false, false, selected, focused);
            Rect position = rect;
            position.width = k_IconWidth;
            position.height = k_IconWidth;
            position.x += iconLeftPadding;
            Texture iconForItem = GetIconForItem(item);

            if (iconForItem != null)
                GUI.DrawTexture(position, iconForItem);
            if (iconOverlayGUI == null)
                return;
            Rect rect1 = rect;
            rect1.width = k_IconWidth + iconTotalPadding;
            iconOverlayGUI(item, rect1);
        }

        public virtual void BeginPingItem(TreeViewItem item, float topPixelOfRow, float availableWidth)
        {
            if (item == null || topPixelOfRow < 0.0)
                return;

            m_Ping.m_TimeStart = Time.realtimeSinceStartup;
            m_Ping.m_PingStyle = s_Styles.ping;

            Vector2 vector2 = m_Ping.m_PingStyle.CalcSize(GUIContentWrap.Temp(item.displayName));
            m_Ping.m_ContentRect = new Rect(GetContentIndent(item) + k_FoldoutWidth, topPixelOfRow, k_IconWidth + k_SpaceBetweenIconAndText + vector2.x + iconTotalPadding, vector2.y);
            m_Ping.m_AvailableWidth = availableWidth;

            var useBoldFont = item.displayName.Equals("Assets");
            m_Ping.m_ContentDraw = (rect) => { DrawIconAndLabel(rect, item, item.displayName, false, false, useBoldFont, true); };
            m_TreeView.Repaint();
        }

        public virtual void EndPingItem()
        {
            m_Ping.m_TimeStart = -1f;
        }

        private void HandlePing()
        {
            m_Ping.HandlePing();
            if (!m_Ping.isPinging)
                return;
            m_TreeView.Repaint();
        }

        protected RenameOverlay GetRenameOverlay()
        {
            return m_TreeView.state.renameOverlay;
        }

        protected virtual bool IsRenaming(int id)
        {
            if (GetRenameOverlay().IsRenaming() && GetRenameOverlay().userData == id)
                return !GetRenameOverlay().isWaitingForDelay;
            return false;
        }

        public virtual bool BeginRename(TreeViewItem item, float delay)
        {
            if (BeginRenameAction != null)
                BeginRenameAction();
            return GetRenameOverlay().BeginRename(item.displayName, item.id, delay);
        }

        public virtual void EndRename()
        {
            if (GetRenameOverlay().HasKeyboardFocus())
                m_TreeView.GrabKeyboardFocus();
            RenameEnded();
            ClearRenameAndNewItemState();
        }

        protected virtual void RenameEnded()
        {

        }

        public virtual void DoRenameOverlay()
        {
            if (!GetRenameOverlay().IsRenaming() || GetRenameOverlay().OnGUI())
                return;
            EndRename();
        }

        protected virtual void SyncFakeItem()
        {

        }

        protected virtual void ClearRenameAndNewItemState()
        {
            m_TreeView.data.RemoveFakeItem();
            GetRenameOverlay().Clear();
        }

        public virtual float GetFoldoutIndent(TreeViewItem item)
        {
            if (m_TreeView.isSearching)
                return k_BaseIndent;
            return k_BaseIndent + item.depth * indentWidth;
        }

        public virtual float GetContentIndent(TreeViewItem item)
        {
            return GetFoldoutIndent(item) + k_FoldoutWidth;
        }

        public class Styles
        {
            public GUIStyle foldout = "IN Foldout";
            public GUIStyle insertion = "PR Insertion";
            public GUIStyle insertionAbove = "PR Insertion Above";
            public GUIStyle ping = new GUIStyle("PR Ping");
            public GUIStyle toolbarButton = "ToolbarButton";
            public GUIStyle lineStyle = new GUIStyle("PR Label");
            public GUIStyle selectionStyle = new GUIStyle("PR Label");
            public GUIContent content = new GUIContent(EditorGUIUtility.FindTexture(EditorResourcesUtilityWrap.folderIconName));
            public GUIStyle lineBoldStyle;

            public Styles()
            {
                Texture2D background = lineStyle.hover.background;
                lineStyle.onNormal.background = background;
                lineStyle.onActive.background = background;
                lineStyle.onFocused.background = background;
                lineStyle.alignment = TextAnchor.MiddleLeft;
                lineBoldStyle = new GUIStyle(lineStyle);
                lineBoldStyle.font = EditorStyles.boldLabel.font;
                lineBoldStyle.fontStyle = EditorStyles.boldLabel.fontStyle;
                ping.padding.left = 16;
                ping.padding.right = 16;
                ping.fixedHeight = 16f;
            }
        }
    }
}