using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class ObjectTreeViewDragging : TreeViewDragging
    {
        private string m_DragID;

        public ObjectTreeViewDragging(TreeView treeView, string dragID) : base(treeView)
        {
            if (dragID == null)
                throw new NullReferenceException("dragID");
            m_DragID = dragID;
        }

        public override void StartDrag(TreeViewItem draggedItem, List<int> draggedItemIDs)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(m_DragID, GetItemsFromIDs(draggedItemIDs));
            DragAndDrop.objectReferences = new UnityEngine.Object[0];
            DragAndDrop.StartDrag(draggedItemIDs.Count + " " + m_DragID + (draggedItemIDs.Count <= 1 ? string.Empty : "s"));
        }

        public override DragAndDropVisualMode DoDrag(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, DropPosition dropPos)
        {
            List<TreeViewItem> dragItems = DragAndDrop.GetGenericData(m_DragID) as List<TreeViewItem>;
            if (dragItems != null)
            {
                if (parentItem == null || targetItem == null)
                {
                    if (dragItems.Count == 0)
                    {
                        return DragAndDropVisualMode.None;
                    }
                }

                bool flag = ValidDrag(parentItem, dragItems);
                if (perform && flag)
                {
                    ReparentSelection(parentItem, targetItem, dragItems, dropPos);
                    m_TreeView.ReloadData();
                    m_TreeView.SetSelection(m_TreeView.state.selectedIDs.ToArray(), true);
                }
                return flag ? DragAndDropVisualMode.Link : DragAndDropVisualMode.None;
            }

            return DragAndDropVisualMode.None;
        }

        public void ReparentSelection(TreeViewItem parentItem, TreeViewItem insertAfterItem, List<TreeViewItem> draggedItems, DropPosition dropPos)
        {
            if (PrepareDoDrag != null)
            {
                PrepareDoDrag();
            }

            int insertIndex = -1;
            List<int> selectedIds = new List<int>();
            foreach (var item in draggedItems)
            {
                selectedIds.Add(item.id);
                if ((item.parent == parentItem && dropPos != DropPosition.Below) ||
                    (item == insertAfterItem))
                {
                    continue;
                }
                bool ignore = false;
                var temp = parentItem;
                while (temp != null)
                {
                    if (item.id == temp.id)
                    {
                        ignore = true;
                        break;
                    }
                    temp = temp.parent;
                }

                if (!ignore)
                {
                    item.parent.children.Remove(item);
                    if (insertAfterItem != null && parentItem.children != null)
                    {
                        insertIndex = parentItem.children.IndexOf(insertAfterItem);
                    }

                    if ((insertAfterItem == parentItem && dropPos != DropPosition.Below) ||
                        insertIndex >= parentItem.children.Count)
                    {
                        parentItem.AddChild(item);
                    }
                    else
                    {
                        parentItem.AddChildAtIndex(item, insertIndex + 1);
                    }
                }
            }

            m_TreeView.data.RefreshData();
            m_TreeView.SetSelection(selectedIds.ToArray(), true);
        }

        public static bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            return true;
        }

        private List<TreeViewItem> GetItemsFromIDs(IEnumerable<int> draggedItemIDs)
        {
            return TreeViewUtility.FindItemsInList(draggedItemIDs, m_TreeView.data.GetRows());
        }



        public override bool DragElement(TreeViewItem targetItem, Rect targetItemRect, bool firstItem)
        {
            if (targetItem == null)
            {
                if (m_DropData != null)
                {
                    m_DropData.dropTargetControlID = 0;
                    m_DropData.rowMarkerControlID = 0;
                }

                bool perform = Event.current.type == EventType.DragPerform;
                DragAndDrop.visualMode = DoDrag(null, null, perform, TreeViewDragging.DropPosition.Below);
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

            TreeViewDragging.DropPosition dropPosition = mousePosition.y < targetItemRect.yMax - betweenHalfHeight ? (!firstItem || mousePosition.y > targetItemRect.yMin + betweenHalfHeight ? (!flag ? TreeViewDragging.DropPosition.Above : TreeViewDragging.DropPosition.Upon) : TreeViewDragging.DropPosition.Above) : TreeViewDragging.DropPosition.Below;
            TreeViewItem parentItem = !m_TreeView.data.IsExpanded(targetItem) || !targetItem.hasChildren ? targetItem.parent : targetItem;
            DragAndDropVisualMode andDropVisualMode1 = DragAndDropVisualMode.None;
            if (Event.current.type == EventType.DragPerform)
            {
                if (dropPosition == TreeViewDragging.DropPosition.Upon)
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
                    m_DropData = new TreeViewDragging.DropData();
                m_DropData.dropTargetControlID = 0;
                m_DropData.rowMarkerControlID = 0;
                int itemControlId = TreeView.GetItemControlID(targetItem, m_TreeView);
                HandleAutoExpansion(itemControlId, targetItem, targetItemRect, betweenHalfHeight, mousePosition);

                if (dropPosition == TreeViewDragging.DropPosition.Upon)
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
                        drawRowMarkerAbove = dropPosition == TreeViewDragging.DropPosition.Above;
                        m_DropData.rowMarkerControlID = itemControlId;
                        //m_DropData.dropTargetControlID = !drawRowMarkerAbove ? TreeView.GetItemControlID(parentItem) : 0;
                        DragAndDrop.visualMode = andDropVisualMode2;
                    }
                }
            }
            Event.current.Use();
            return true;
        }
    }
}
