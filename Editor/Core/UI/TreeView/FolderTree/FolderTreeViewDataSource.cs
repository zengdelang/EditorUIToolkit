using System.Collections.Generic;

namespace EUTK
{
    public class FolderTreeViewDataSource : TreeViewDataSource
    {
        private FolderTreeItemContainer m_DataContainer;
        private EditorWindowConfigSource m_ConfigSource;

        public FolderTreeItemContainer DataContainer
        {
            get { return m_DataContainer; }
            set { m_DataContainer = value; }
        }

        public EditorWindowConfigSource ConfigSource
        {
            get { return m_ConfigSource; }
            set { m_ConfigSource = value; }
        }

        public FolderTreeViewDataSource(TreeView treeView, FolderTreeItemContainer dataContainer = null, EditorWindowConfigSource configSource = null) : base(treeView)
        {
            //showRootItem = false;
            //rootIsCollapsable = false;
            m_DataContainer = dataContainer;
            if (m_DataContainer == null)
            {
                m_DataContainer = new FolderTreeItemContainer();
            }

            m_ConfigSource = configSource;
        }

        public override void FetchData()
        {
            m_RootItem = DataContainer.RootItem;
            if (m_RootItem != null)
                SetExpanded(m_RootItem, true);
            m_NeedRefreshVisibleFolders = true;
        }

        /*public override bool CanBeParent(TreeViewItem item)
        {
            return true;
        }*/

        public override bool IsRenamingItemAllowed(TreeViewItem item)
        {
            return item.id != 0;
        }

        protected override List<TreeViewItem> Search(TreeViewItem root, string search)
        {
            return ExpandedRows(root);
        }

        public override bool IsExpandable(TreeViewItem item)
        {
            return item.hasChildren;
        }
    }
}
