using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class ObjectTreeViewGUI : TreeViewGUI
    {
        public Action<TreeViewItem, string> RenameEndAction { get; set; }

        public ObjectTreeViewGUI(TreeView treeView) : base(treeView)
        {

        }

        protected override Texture GetIconForItem(TreeViewItem item)
        {
            return null;
        }

        protected override void DoItemGUI(Rect rect, int row, TreeViewItem item, bool selected, bool focused, bool useBoldFont)
        {
            EditorGUIUtility.SetIconSize(new Vector2(k_IconWidth, k_IconWidth));
            float foldoutIndent = GetFoldoutIndent(item);
            int itemControlId = TreeView.GetItemControlID(item, m_TreeView);
            bool flag1 = false;
            if (m_TreeView.dragging != null)
                flag1 = m_TreeView.dragging.GetDropTargetControlID() == itemControlId && m_TreeView.data.CanBeParent(item);
            bool flag2 = IsRenaming(item.id);
            bool flag3 = m_TreeView.data.IsExpandable(item) && !m_TreeView.isSearching;
            if (flag2 && Event.current.type == EventType.Repaint)
            {
                float num2 = (float)(foldoutIndent + iconTotalPadding - 1.0) + 12f;
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
                {
                    float contentIndent = GetContentIndent(item);
                    var paddingLeft = k_IconWidth + iconTotalPadding + k_SpaceBetweenIconAndText;
                    m_DraggingInsertionMarkerRect = new Rect(rect.x + contentIndent - 5 + paddingLeft, rect.y, rect.width - contentIndent + 5, rect.height);
                }
            }
            if (flag3)
                DoFoldout(rect, item, row);
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        protected override void DrawIconAndLabel(Rect rect, TreeViewItem item, string label, bool selected, bool focused, bool useBoldFont, bool isPinging)
        {
            GUIStyle guiStyle = !useBoldFont ? s_Styles.lineStyle : s_Styles.lineBoldStyle;
            if (!isPinging)
            {
                float contentIndent = GetContentIndent(item);
                rect.x += (contentIndent - 5);
                rect.width -= (contentIndent - 5);
                guiStyle.padding.left = (int)(k_IconWidth + iconTotalPadding + k_SpaceBetweenIconAndText);
            }
            else
            {
                guiStyle.padding.left = 0;

            }

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

        public override float GetContentIndent(TreeViewItem item)
        {
            return GetFoldoutIndent(item);
        }

        public override void BeginPingItem(TreeViewItem item, float topPixelOfRow, float availableWidth)
        {
            if (item == null || topPixelOfRow < 0.0)
                return;

            m_Ping.m_TimeStart = Time.realtimeSinceStartup;
            m_Ping.m_PingStyle = s_Styles.ping;

            Vector2 vector2 = m_Ping.m_PingStyle.CalcSize(GUIContentWrap.Temp(item.displayName));
            m_Ping.m_ContentRect = new Rect(GetContentIndent(item) + k_FoldoutWidth, topPixelOfRow, k_IconWidth + k_SpaceBetweenIconAndText + vector2.x + iconTotalPadding, vector2.y);
            m_Ping.m_AvailableWidth = availableWidth;

            m_Ping.m_ContentDraw = (rect) => { DrawIconAndLabel(rect, item, item.displayName, false, false, false, true); };
            m_TreeView.Repaint();
        }

        protected override void RenameEnded()
        {
            string name = !string.IsNullOrEmpty(GetRenameOverlay().name) ? GetRenameOverlay().name : GetRenameOverlay().originalName;
            int userData = GetRenameOverlay().userData;
            RenameOverlay.indentLevel = 0;
            if (!GetRenameOverlay().userAcceptedRename)
                return;

            TreeViewItem treeViewItem = m_TreeView.data.FindItem(userData);
            if (treeViewItem != null && treeViewItem.displayName != name)
            {
                treeViewItem.displayName = name;
            }

            if (RenameEndAction != null)
            {
                RenameEndAction(treeViewItem, name);
            }
        }
    }
}

