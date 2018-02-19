using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EUTK
{
    public abstract class TreeViewDataSource : ITreeViewDataSource
    {
        protected bool m_NeedRefreshVisibleFolders = true;
        protected readonly TreeView m_TreeView;
        protected TreeViewItem m_RootItem;
        protected List<TreeViewItem> m_VisibleRows;
        protected TreeViewItem m_FakeItem;
        public Action onVisibleRowsChanged;

        public bool showRootItem { get; set; }

        public bool rootIsCollapsable { get; set; }

        public bool alwaysAddFirstItemToSearchResult { get; set; }

        public TreeViewItem root
        {
            get
            {
                return m_RootItem;
            }
        }

        protected List<int> expandedIDs
        {
            get { return m_TreeView.state.expandedIDs; }
            set { m_TreeView.state.expandedIDs = value; }
        }

        public virtual int rowCount
        {
            get { return GetRows().Count; }
        }

        public TreeViewDataSource(TreeView treeView)
        {
            m_TreeView = treeView;
            showRootItem = true;
            rootIsCollapsable = false;
        }

        public virtual void OnInitialize()
        {
        }

        public abstract void FetchData();

        public void ReloadData()
        {
            m_FakeItem = null;
            FetchData();
        }

        public virtual TreeViewItem FindItem(int id)
        {
            return TreeViewUtility.FindItem(id, m_RootItem);
        }

        public virtual bool IsRevealed(int id)
        {
            return TreeView.GetIndexOfID(GetRows(), id) >= 0;
        }

        public virtual void RevealItem(int id)
        {
            if (IsRevealed(id))
                return;

            TreeViewItem treeViewItem = FindItem(id);
            if (treeViewItem == null)
                return;

            for (TreeViewItem parent = treeViewItem.parent; parent != null; parent = parent.parent)
                SetExpanded(parent, true);
        }

        public virtual void OnSearchChanged()
        {
            m_NeedRefreshVisibleFolders = true;
        }

        public void RefreshData()
        {
            m_NeedRefreshVisibleFolders = true;
        }

        protected void GetVisibleItemsRecursive(TreeViewItem item, List<TreeViewItem> items)
        {
            if (item != m_RootItem || showRootItem)
                items.Add(item);

            if (!item.hasChildren || !IsExpanded(item))
                return;


            foreach (var child in item.children)
            {
                GetVisibleItemsRecursive(child, items);
            }
        }

        protected void SearchRecursive(TreeViewItem item, string search, List<TreeViewItem> searchResult)
        {
            if (item.displayName.ToLower().Contains(search))
                searchResult.Add(item);

            if (item.children == null)
                return;

            foreach (var child in item.children)
            {
                SearchRecursive(child, search, searchResult);
            }
        }

        protected virtual List<TreeViewItem> ExpandedRows(TreeViewItem root)
        {
            List<TreeViewItem> items = new List<TreeViewItem>();
            GetVisibleItemsRecursive(m_RootItem, items);
            return items;
        }

        protected virtual List<TreeViewItem> Search(TreeViewItem root, string search)
        {
            List<TreeViewItem> searchResult = new List<TreeViewItem>();
            if (showRootItem)
            {
                SearchRecursive(root, search, searchResult);
                searchResult.Sort(new TreeViewItemAlphaNumericSort());
            }
            else
            {
                int num = !alwaysAddFirstItemToSearchResult ? 0 : 1;
                if (root.hasChildren)
                {
                    for (int index = num; index < root.children.Count; ++index)
                        SearchRecursive(root.children[index], search, searchResult);
                    searchResult.Sort((IComparer<TreeViewItem>)new TreeViewItemAlphaNumericSort());
                    if (alwaysAddFirstItemToSearchResult)
                        searchResult.Insert(0, root.children[0]);
                }
            }
            return searchResult;
        }

        public virtual int GetRow(int id)
        {
            List<TreeViewItem> rows = GetRows();
            for (int index = 0; index < rows.Count; ++index)
            {
                if (rows[index].id == id)
                    return index;
            }
            return -1;
        }

        public virtual TreeViewItem GetItem(int row)
        {
            return GetRows()[row];
        }

        public virtual List<TreeViewItem> GetRows()
        {
            InitIfNeeded();
            return m_VisibleRows;
        }

        public virtual void InitIfNeeded()
        {
            if (m_VisibleRows != null && !m_NeedRefreshVisibleFolders)
                return;
            if (m_RootItem != null)
            {
                m_VisibleRows = !m_TreeView.isSearching
                    ? ExpandedRows(m_RootItem)
                    : Search(m_RootItem, m_TreeView.searchString.ToLower());
            }
            else
            {
                Debug.LogError("TreeView root item is null. Ensure that your TreeViewDataSource sets up at least a root item.");
                m_VisibleRows = new List<TreeViewItem>();
            }

            m_NeedRefreshVisibleFolders = false;
            if (onVisibleRowsChanged != null)
                onVisibleRowsChanged();
            m_TreeView.Repaint();
        }

        public virtual int[] GetExpandedIDs()
        {
            return expandedIDs.ToArray();
        }

        public virtual void SetExpandedIDs(int[] ids)
        {
            expandedIDs = new List<int>(ids);
            expandedIDs.Sort();
            m_NeedRefreshVisibleFolders = true;
            OnExpandedStateChanged();
        }

        public virtual bool IsExpanded(int id)
        {
            return expandedIDs.BinarySearch(id) >= 0;
        }

        public virtual bool SetExpanded(int id, bool expand)
        {
            bool flag = IsExpanded(id);
            if (expand == flag)
                return false;

            if (expand)
            {
                expandedIDs.Add(id);
                expandedIDs.Sort();
            }
            else
                expandedIDs.Remove(id);

            m_NeedRefreshVisibleFolders = true;
            OnExpandedStateChanged();
            return true;
        }

        public virtual void SetExpandedWithChildren(TreeViewItem fromItem, bool expand)
        {
            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            stack.Push(fromItem);
            HashSet<int> hashSet1 = new HashSet<int>();
            while (stack.Count > 0)
            {
                TreeViewItem treeViewItem = stack.Pop();
                if (treeViewItem.hasChildren)
                {
                    hashSet1.Add(treeViewItem.id);
                    using (List<TreeViewItem>.Enumerator enumerator = treeViewItem.children.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            TreeViewItem current = enumerator.Current;
                            stack.Push(current);
                        }
                    }
                }
            }
            HashSet<int> hashSet2 = new HashSet<int>(expandedIDs);
            if (expand)
                hashSet2.UnionWith(hashSet1);
            else
                hashSet2.ExceptWith(hashSet1);
            SetExpandedIDs(Enumerable.ToArray(hashSet2));
        }

        public virtual void SetExpanded(TreeViewItem item, bool expand)
        {
            SetExpanded(item.id, expand);
        }

        public virtual bool IsExpanded(TreeViewItem item)
        {
            return IsExpanded(item.id);
        }

        public virtual bool IsExpandable(TreeViewItem item)
        {
            if (m_TreeView.isSearching)
                return false;
            return item.hasChildren;
        }

        public virtual bool CanBeMultiSelected(TreeViewItem item)
        {
            return true;
        }

        public virtual bool CanBeParent(TreeViewItem item)
        {
            return true;
        }

        public virtual void OnExpandedStateChanged()
        {
            if (m_TreeView.expandedStateChanged == null)
                return;
            m_TreeView.expandedStateChanged();
        }

        public virtual bool IsRenamingItemAllowed(TreeViewItem item)
        {
            return true;
        }

        public virtual void InsertFakeItem(int id, int parentID, string name, Texture2D icon)
        {
            Debug.LogError("InsertFakeItem missing implementation");
        }

        public virtual bool HasFakeItem()
        {
            return m_FakeItem != null;
        }

        public virtual void RemoveFakeItem()
        {
            if (!HasFakeItem())
                return;
            List<TreeViewItem> rows = GetRows();
            int indexOfId = TreeView.GetIndexOfID(rows, m_FakeItem.id);
            if (indexOfId != -1)
                rows.RemoveAt(indexOfId);
            m_FakeItem = null;
        }
    }
}