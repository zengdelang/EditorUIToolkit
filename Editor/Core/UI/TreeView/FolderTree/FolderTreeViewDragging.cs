using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderTreeViewDragging : TreeViewDragging
    {
        private string m_DragID;

        public Action<bool> EndDragAction { get; set; }

        public FolderTreeViewDragging(TreeView treeView, string dragID) : base(treeView)
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
            if (dragItems == null)
            {
                List<GridItem> dragGirdItems = DragAndDrop.GetGenericData(m_DragID) as List<GridItem>;
                if (dragGirdItems != null && dragGirdItems.Count > 0)
                    dragItems = GetDragItems(dragGirdItems);
            }

            if (dragItems != null && dragItems.Count > 0)
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
                    bool aboveFirstItem = m_TreeView.data.root.children != null && m_TreeView.data.root.children[0].id == targetItem.id && dropPos == DropPosition.Above;
                    ReparentSelection(parentItem, targetItem, dragItems, aboveFirstItem);

                    m_TreeView.ReloadData();
                    m_TreeView.SetSelection(m_TreeView.state.selectedIDs.ToArray(), true);
                }
                return flag ? DragAndDropVisualMode.Link : DragAndDropVisualMode.None;
            }
            return DragAndDropVisualMode.None;
        }

        public DragAndDropVisualMode DoDragForGridView(TreeViewItem parentItem, TreeViewItem targetItem, bool perform)
        {
            List<TreeViewItem> dragItems = DragAndDrop.GetGenericData(m_DragID) as List<TreeViewItem>;
            if (dragItems == null)
            {
                List<GridItem> dragGirdItems = DragAndDrop.GetGenericData(m_DragID) as List<GridItem>;
                if (dragGirdItems != null && dragGirdItems.Count > 0)
                    dragItems = GetDragItems(dragGirdItems);
            }

            if (dragItems != null && dragItems.Count > 0)
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
                    ReparentSelection(parentItem, targetItem, dragItems);

                    m_TreeView.ReloadData();
                    m_TreeView.SetSelection(m_TreeView.state.selectedIDs.ToArray(), true);
                }
                return flag ? DragAndDropVisualMode.Link : DragAndDropVisualMode.None;
            }
            return DragAndDropVisualMode.None;
        }

        public List<TreeViewItem> GetDragItems(List<GridItem> dragGirdItems)
        {
            List<TreeViewItem> itemList = new List<TreeViewItem>();

            foreach (var item in dragGirdItems)
            {
                var folderItem = item as FolderGridItem;
                if (folderItem.IsFolder)
                {
                    itemList.Add(m_TreeView.FindItem(folderItem.Id));
                }
                else
                {
                    var parentItem = m_TreeView.FindItem(folderItem.ParentId) as FolderTreeViewItem;
                    foreach (var child in parentItem.FileList)
                    {
                        if (child.id == item.Id)
                        {
                            itemList.Add(child);
                            break;
                        }
                    }
                }
            }
            return itemList;
        }

        public static bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            //拖拽的items里面不能有parent的祖先
            for (TreeViewItem treeViewItem = parent; treeViewItem != null; treeViewItem = treeViewItem.parent)
            {
                if (draggedItems.Contains(treeViewItem))
                    return false;
            }

            for (int i = 0; i < draggedItems.Count; i++)
            {
                if (parent == null || draggedItems[i].parent.id == parent.id)
                    return false;
            }

            //parent下面是否有相同命名的文件或者文件夹存在
            var parentFolderItem = parent as FolderTreeViewItem;
            foreach (var item in draggedItems)
            {
                var folderItem = item as FolderTreeViewItem;
                if (folderItem.IsFolder)
                {
                    if (parent.hasChildren)
                    {
                        foreach (var child in parentFolderItem.children)
                        {
                            if (child.displayName == folderItem.displayName)
                                return false;
                        }
                    }
                }
                else
                {
                    if (parentFolderItem.FileList != null)
                    {
                        foreach (var child in parentFolderItem.FileList)
                        {
                            if (Path.GetFileName(child.Path) == Path.GetFileName(folderItem.Path))
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        public void ReparentSelection(TreeViewItem parentItem, TreeViewItem insertAfterItem, List<TreeViewItem> draggedItems, bool aboveFirstItem = false)
        {
            if (PrepareDoDrag != null)
            {
                PrepareDoDrag();
            }

            //如果insertAfterItem在draggedItems中，剔除draggedItems中insertAfterItem以及之后的item
            if (aboveFirstItem)
            {
                insertAfterItem = m_TreeView.data.root;
            }
            int i = draggedItems.IndexOf(insertAfterItem);
            if (i >= 0)
            {
                for (var j = draggedItems.Count - 1; j >= 0; --j)
                {
                    if (j >= i)
                    {
                        draggedItems.RemoveAt(j);
                    }
                    else
                    {
                        break;
                    }
                }
                ReparentSelection(parentItem, insertAfterItem, draggedItems, aboveFirstItem);
                return;
            }

            try
            {
                foreach (var item in draggedItems)
                {
                    var folderItem = item as FolderTreeViewItem;
                    if (folderItem != null)
                        folderItem.Reparent(parentItem as FolderTreeViewItem);
                }

                foreach (var item in draggedItems)
                {
                    var folderItem = item as FolderTreeViewItem;
                    if (folderItem != null)
                    {
                        if (folderItem.parent != null)
                        {
                            if (folderItem.IsFolder)
                                folderItem.parent.children.Remove(item);
                            else
                            {
                                var parentFolderItem = folderItem.parent as FolderTreeViewItem;
                                parentFolderItem.FileList.Remove(folderItem);
                            }
                        }
                        item.parent = parentItem;
                    }
                }

                var folderParentItem = parentItem as FolderTreeViewItem;
                if (folderParentItem.FileList == null)
                    folderParentItem.FileList = new List<FolderTreeViewItem>();

                for (int j = 0; j < draggedItems.Count; j++)
                {
                    var item = draggedItems[j] as FolderTreeViewItem;
                    if (item != null && item.IsFolder)
                    {
                        parentItem.AddChild(item);
                    }
                    else if (item != null && !item.IsFolder)
                    {
                        item.parent = parentItem;
                        folderParentItem.FileList.Add(item);
                    }
                }

                var comparator = new AlphanumComparator.AlphanumComparator();
                if (folderParentItem.hasChildren)
                {
                    folderParentItem.children.Sort((obj1, obj2) =>
                    {
                        return comparator.Compare(obj1.displayName, obj2.displayName);
                    });
                }

                folderParentItem.FileList.Sort((obj1, obj2) =>
                {
                    return comparator.Compare(obj1.displayName, obj2.displayName);
                });

                if (EndDragAction != null)
                {
                    EndDragAction(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("拖拽操作出错:" + e);
                if (EndDragAction != null)
                {
                    EndDragAction(true);
                }
            }
        }

        private List<TreeViewItem> GetItemsFromIDs(IEnumerable<int> draggedItemIDs)
        {
            return TreeViewUtility.FindItemsInList(draggedItemIDs, m_TreeView.data.GetRows());
        }

        public TreeViewItem GetItem(int id)
        {
            return m_TreeView.FindItem(id);
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
