using System;
using System.Collections.Generic;
using System.Linq;

namespace EUTK
{
    public static class TreeViewUtility
    {
        public static List<TreeViewItem> FindItemsInList(IEnumerable<int> itemIDs, List<TreeViewItem> treeViewItems)
        {
            return Enumerable.ToList(Enumerable.Where(treeViewItems, (item) => { return itemIDs.Contains(item.id); }));
        }

        public static TreeViewItem FindItemInList<T>(int id, List<T> treeViewItems) where T : TreeViewItem
        {
            return Enumerable.FirstOrDefault(treeViewItems, (item) => { return item.id == id; });
        }

        public static TreeViewItem FindItem(int id, TreeViewItem searchFromThisItem)
        {
            return FindItemRecursive(id, searchFromThisItem);
        }

        private static TreeViewItem FindItemRecursive(int id, TreeViewItem item)
        {
            if (item == null)
                return null;

            if (item.id == id)
                return item;

            if (!item.hasChildren)
                return null;

            using (List<TreeViewItem>.Enumerator enumerator = item.children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TreeViewItem current = enumerator.Current;
                    TreeViewItem itemRecursive = FindItemRecursive(id, current);
                    if (itemRecursive != null)
                        return itemRecursive;
                }
            }
            return null;
        }

        public static void DebugPrintToEditorLogRecursive(TreeViewItem item)
        {
            if (item == null)
                return;

            Console.WriteLine(new string(' ', item.depth * 3) + item.displayName);
            if (!item.hasChildren)
                return;

            using (List<TreeViewItem>.Enumerator enumerator = item.children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    DebugPrintToEditorLogRecursive(enumerator.Current);
            }
        }

        public static void SetChildParentReferences(List<TreeViewItem> visibleItems, TreeViewItem root)
        {
            for (int index = 0; index < visibleItems.Count; ++index)
                visibleItems[index].parent = null;

            int capacity = 0;
            for (int parentIndex = 0; parentIndex < visibleItems.Count; ++parentIndex)
            {
                SetChildParentReferences(parentIndex, visibleItems);
                if (visibleItems[parentIndex].parent == null)
                    ++capacity;
            }

            if (capacity <= 0)
                return;

            List<TreeViewItem> list = new List<TreeViewItem>(capacity);
            for (int index = 0; index < visibleItems.Count; ++index)
            {
                if (visibleItems[index].parent == null)
                {
                    list.Add(visibleItems[index]);
                    visibleItems[index].parent = root;
                }
            }
            root.children = list;
        }

        private static void SetChildren(TreeViewItem item, List<TreeViewItem> newChildList)
        {
            if (LazyTreeViewDataSource.IsChildListForACollapsedParent(item.children) && newChildList == null)
                return;
            item.children = newChildList;
        }

        private static void SetChildParentReferences(int parentIndex, List<TreeViewItem> visibleItems)
        {
            TreeViewItem treeViewItem = visibleItems[parentIndex];
            if (treeViewItem.children != null && treeViewItem.children.Count > 0 && treeViewItem.children[0] != null)
                return;

            int depth = treeViewItem.depth;
            int capacity = 0;
            for (int index = parentIndex + 1; index < visibleItems.Count; ++index)
            {
                if (visibleItems[index].depth == depth + 1)
                    ++capacity;
                if (visibleItems[index].depth <= depth)
                    break;
            }

            List<TreeViewItem> newChildList = null;
            if (capacity != 0)
            {
                newChildList = new List<TreeViewItem>(capacity);
                int num = 0;
                for (int index = parentIndex + 1; index < visibleItems.Count; ++index)
                {
                    if (visibleItems[index].depth == depth + 1)
                    {
                        visibleItems[index].parent = treeViewItem;
                        newChildList.Add(visibleItems[index]);
                        ++num;
                    }
                    if (visibleItems[index].depth <= depth)
                        break;
                }
            }
            SetChildren(treeViewItem, newChildList);
        }
    }
}
