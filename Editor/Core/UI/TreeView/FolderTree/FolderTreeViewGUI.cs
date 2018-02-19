using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderTreeViewGUI : TreeViewGUI
    {
        public FolderTreeItemContainer DataContainer { get; set; }
        public Action<TreeViewItem, string> RenameEndAction { get; set; }

        public FolderTreeViewGUI(TreeView treeView, FolderTreeItemContainer dataContainer = null) : base(treeView)
        {
            DataContainer = dataContainer;
        }

        protected override void RenameEnded()
        {
            string name = !string.IsNullOrEmpty(GetRenameOverlay().name) ? GetRenameOverlay().name : GetRenameOverlay().originalName;
            int userData = GetRenameOverlay().userData;
            if (!GetRenameOverlay().userAcceptedRename)
                return;

            try
            {
                TreeViewItem treeViewItem = m_TreeView.data.FindItem(userData);
                if (treeViewItem != null && treeViewItem.displayName != name)
                {
                    if (RenameEndAction != null)
                    {
                        RenameEndAction(treeViewItem, name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("重命名出错:" + e);
            }
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
                if (selected && !m_TreeView.isSearching)
                    s_Styles.selectionStyle.Draw(rect, false, false, true, focused);
                if (flag1)
                    s_Styles.lineStyle.Draw(rect, GUIContent.none, true, true, false, false);
                DrawIconAndLabel(rect, item, label, selected && !m_TreeView.isSearching, focused, DataContainer != null && item == DataContainer.RootItem, false);
                if (m_TreeView.dragging != null && m_TreeView.dragging.GetRowMarkerControlID() == itemControlId)
                    m_DraggingInsertionMarkerRect = new Rect(rect.x + foldoutIndent + k_FoldoutWidth, rect.y, rect.width - foldoutIndent, rect.height);
            }
            if (flag3)
                DoFoldout(rect, item, row);
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }


        public override void BeginPingItem(TreeViewItem item, float topPixelOfRow, float availableWidth)
        {
            if (item == null || topPixelOfRow < 0.0)
                return;

            m_Ping.m_TimeStart = Time.realtimeSinceStartup;
            m_Ping.m_PingStyle = s_Styles.ping;

            Vector2 vector2 = m_Ping.m_PingStyle.CalcSize(GUIContentWrap.Temp(item.displayName));
            m_Ping.m_ContentRect = new Rect(GetContentIndent(item), topPixelOfRow, k_IconWidth + k_SpaceBetweenIconAndText + vector2.x + iconTotalPadding, vector2.y);
            m_Ping.m_AvailableWidth = availableWidth;

            bool useBoldFont = DataContainer != null && item == DataContainer.RootItem;
            m_Ping.m_ContentDraw = (rect) => { DrawIconAndLabel(rect, item, item.displayName, false, false, useBoldFont, true); };
            m_TreeView.Repaint();
        }

        protected override Texture GetIconForItem(TreeViewItem item)
        {
            return EditorGUIUtility.FindTexture(EditorResourcesUtilityWrap.folderIconName);
        }

        public override float GetFoldoutIndent(TreeViewItem item)
        {
            return k_BaseIndent + item.depth * indentWidth;
        }
    }
}
