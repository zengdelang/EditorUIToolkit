using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

namespace EUTK
{
    public abstract class LazyTreeViewDataSource : TreeViewDataSource
    {
        public LazyTreeViewDataSource(TreeView treeView)
            : base(treeView)
        {
        }

        public static List<TreeViewItem> CreateChildListForCollapsedParent()
        {
            return new List<TreeViewItem>()
            {
                null
            };
        }

        public static bool IsChildListForACollapsedParent(List<TreeViewItem> childList)
        {
            if (childList != null && childList.Count == 1)
                return childList[0] == null;
            return false;
        }

        protected abstract HashSet<int> GetParentsAbove(int id);

        protected abstract HashSet<int> GetParentsBelow(int id);

        public override void RevealItem(int itemID)
        {
            HashSet<int> hashSet = new HashSet<int>(expandedIDs);
            int count = hashSet.Count;
            HashSet<int> parentsAbove = GetParentsAbove(itemID);
            hashSet.UnionWith(parentsAbove);
            if (count == hashSet.Count)
                return;

            SetExpandedIDs(Enumerable.ToArray(hashSet));
            if (!m_NeedRefreshVisibleFolders)
                return;
            FetchData();
        }

        public override TreeViewItem FindItem(int itemID)
        {
            RevealItem(itemID);
            return base.FindItem(itemID);
        }

        public override void SetExpandedWithChildren(TreeViewItem item, bool expand)
        {
            HashSet<int> hashSet = new HashSet<int>(expandedIDs);
            HashSet<int> parentsBelow = GetParentsBelow(item.id);
            if (expand)
                hashSet.UnionWith(parentsBelow);
            else
                hashSet.ExceptWith(parentsBelow);

            SetExpandedIDs(Enumerable.ToArray(hashSet));
        }

        public override bool SetExpanded(int id, bool expand)
        {
            if (!base.SetExpanded(id, expand))
                return false;
            InternalEditorUtility.expandedProjectWindowItems = expandedIDs.ToArray();
            return true;
        }

        public override void InitIfNeeded()
        {
            if (m_VisibleRows != null && !m_NeedRefreshVisibleFolders)
                return;
            FetchData();
            m_NeedRefreshVisibleFolders = false;
            if (onVisibleRowsChanged != null)
                onVisibleRowsChanged();
            m_TreeView.Repaint();
        }

        public override List<TreeViewItem> GetRows()
        {
            InitIfNeeded();
            return m_VisibleRows;
        }
    }
}
