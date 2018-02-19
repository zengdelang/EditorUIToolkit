using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace EUTK
{
    public abstract class TreeViewDragging : ITreeViewDragging
    {
        protected DropData m_DropData = new DropData();
        private const double k_DropExpandTimeout = 0.7;
        protected TreeView m_TreeView;

        public bool drawRowMarkerAbove { get; set; }
        public Action PrepareDoDrag { get; set; }

        public TreeViewDragging(TreeView treeView)
        {
            m_TreeView = treeView;
        }

        public virtual void OnInitialize()
        {

        }

        public int GetDropTargetControlID()
        {
            return m_DropData.dropTargetControlID;
        }

        public int GetRowMarkerControlID()
        {
            return m_DropData.rowMarkerControlID;
        }

        public virtual bool CanStartDrag(TreeViewItem targetItem, List<int> draggedItemIDs, Vector2 mouseDownPosition)
        {
            return true;
        }

        public abstract void StartDrag(TreeViewItem draggedItem, List<int> draggedItemIDs);

        public abstract DragAndDropVisualMode DoDrag(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, DropPosition dropPosition);

        public virtual bool DragElement(TreeViewItem targetItem, Rect targetItemRect, bool firstItem)
        {
            if (targetItem == null)
            {
                if (m_DropData != null)
                {
                    m_DropData.dropTargetControlID = 0;
                    m_DropData.rowMarkerControlID = 0;
                }

                bool perform = Event.current.type == EventType.DragPerform;
                DragAndDrop.visualMode = DoDrag(null, null, perform, DropPosition.Below);
                Debug.LogError(DoDrag(null, null, perform, DropPosition.Below));
                if (DragAndDrop.visualMode != DragAndDropVisualMode.None && perform)
                    FinalizeDragPerformed(true);
                return false;
            }

            Vector2 mousePosition = Event.current.mousePosition;
            bool flag = m_TreeView.data.CanBeParent(targetItem);
            Rect rect = targetItemRect;
            float betweenHalfHeight = !flag ? targetItemRect.height * 0.5f : m_TreeView.gui.halfDropBetweenHeight;
            if (firstItem)
                rect.yMin -= betweenHalfHeight;
            rect.yMax += betweenHalfHeight;

            if (!rect.Contains(mousePosition))
                return false;

            DropPosition dropPosition = mousePosition.y < targetItemRect.yMax - betweenHalfHeight ? (!firstItem || mousePosition.y > targetItemRect.yMin + betweenHalfHeight ? (!flag ? DropPosition.Above : DropPosition.Upon) : DropPosition.Above) : DropPosition.Below;
            TreeViewItem parentItem = !m_TreeView.data.IsExpanded(targetItem) || !targetItem.hasChildren ? targetItem.parent : targetItem;
            DragAndDropVisualMode andDropVisualMode1 = DragAndDropVisualMode.None;
            if (Event.current.type == EventType.DragPerform)
            {
                if (dropPosition == DropPosition.Upon)
                    andDropVisualMode1 = DoDrag(targetItem, targetItem, true, dropPosition);

                if (andDropVisualMode1 == DragAndDropVisualMode.None && parentItem != null)
                    andDropVisualMode1 = DoDrag(parentItem, targetItem, true, dropPosition);

                if (andDropVisualMode1 != DragAndDropVisualMode.None)
                {
                    FinalizeDragPerformed(false);
                }
                else
                {
                    DragCleanup(true);
                    m_TreeView.NotifyListenersThatDragEnded(null, false);
                }
            }
            else
            {
                if (m_DropData == null)
                    m_DropData = new DropData();
                m_DropData.dropTargetControlID = 0;
                m_DropData.rowMarkerControlID = 0;
                int itemControlId = TreeView.GetItemControlID(targetItem, m_TreeView);
                HandleAutoExpansion(itemControlId, targetItem, targetItemRect, betweenHalfHeight, mousePosition);

                if (dropPosition == DropPosition.Upon)
                    andDropVisualMode1 = DoDrag(targetItem, targetItem, false, dropPosition);

                if (andDropVisualMode1 != DragAndDropVisualMode.None)
                {
                    m_DropData.dropTargetControlID = itemControlId;
                    DragAndDrop.visualMode = andDropVisualMode1;
                }
                else if (parentItem != null)
                {
                    DragAndDropVisualMode andDropVisualMode2 = DoDrag(parentItem, targetItem, false, dropPosition);
                    if (andDropVisualMode2 != DragAndDropVisualMode.None)
                    {
                        drawRowMarkerAbove = dropPosition == DropPosition.Above;
                        m_DropData.rowMarkerControlID = itemControlId;
                        m_DropData.dropTargetControlID = !drawRowMarkerAbove ? TreeView.GetItemControlID(parentItem, m_TreeView) : 0;
                        DragAndDrop.visualMode = andDropVisualMode2;
                    }
                }
            }
            Event.current.Use();
            return true;
        }

        protected void FinalizeDragPerformed(bool revertExpanded)
        {
            DragCleanup(revertExpanded);
            DragAndDrop.AcceptDrag();
            List<UnityEngine.Object> list = new List<UnityEngine.Object>(DragAndDrop.objectReferences);

            bool draggedItemsFromOwnTreeView = true;
            if (list.Count > 0 && list[0] != null && TreeViewUtility.FindItemInList(list[0].GetInstanceID(), m_TreeView.data.GetRows()) == null)
                draggedItemsFromOwnTreeView = false;

            int[] draggedIDs = new int[list.Count];
            for (int index = 0; index < list.Count; ++index)
            {
                if (list[index] != null)
                    draggedIDs[index] = list[index].GetInstanceID();
            }
            m_TreeView.NotifyListenersThatDragEnded(draggedIDs, draggedItemsFromOwnTreeView);
        }

        protected virtual void HandleAutoExpansion(int itemControlID, TreeViewItem targetItem, Rect targetItemRect, float betweenHalfHeight, Vector2 currentMousePos)
        {
            float contentIndent = m_TreeView.gui.GetContentIndent(targetItem);
            bool flag1 = new Rect(targetItemRect.x + contentIndent, targetItemRect.y + betweenHalfHeight, targetItemRect.width - contentIndent, targetItemRect.height - betweenHalfHeight * 2f).Contains(currentMousePos);
            if (itemControlID != m_DropData.lastControlID || !flag1 || m_DropData.expandItemBeginPosition != currentMousePos)
            {
                m_DropData.lastControlID = itemControlID;
                m_DropData.expandItemBeginTimer = Time.realtimeSinceStartup;
                m_DropData.expandItemBeginPosition = currentMousePos;
            }

            bool flag2 = Time.realtimeSinceStartup - m_DropData.expandItemBeginTimer > k_DropExpandTimeout;
            bool flag3 = flag1 && flag2;
            if (targetItem == null || !flag3 || (!targetItem.hasChildren || m_TreeView.data.IsExpanded(targetItem)))
                return;
            if (m_DropData.expandedArrayBeforeDrag == null)
                m_DropData.expandedArrayBeforeDrag = GetCurrentExpanded().ToArray();

            m_TreeView.data.SetExpanded(targetItem, true);
            m_DropData.expandItemBeginTimer = Time.realtimeSinceStartup;
            m_DropData.lastControlID = 0;
        }

        public virtual void DragCleanup(bool revertExpanded)
        {
            if (m_DropData == null)
                return;
            if (m_DropData.expandedArrayBeforeDrag != null && revertExpanded)
                RestoreExpanded(new List<int>(m_DropData.expandedArrayBeforeDrag));
            m_DropData = new DropData();
        }

        public List<int> GetCurrentExpanded()
        {
            return Enumerable.ToList(Enumerable.Select(Enumerable.Where(m_TreeView.data.GetRows(), item => m_TreeView.data.IsExpanded(item)), item => item.id));
        }

        public void RestoreExpanded(List<int> ids)
        {
            using (List<TreeViewItem>.Enumerator enumerator = m_TreeView.data.GetRows().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TreeViewItem current = enumerator.Current;
                    m_TreeView.data.SetExpanded(current, ids.Contains(current.id));
                }
            }
        }

        protected class DropData
        {
            public int[] expandedArrayBeforeDrag;
            public int lastControlID;
            public int dropTargetControlID;
            public int rowMarkerControlID;
            public double expandItemBeginTimer;
            public Vector2 expandItemBeginPosition;
        }

        public enum DropPosition
        {
            Upon,
            Below,
            Above,
        }
    }
}
