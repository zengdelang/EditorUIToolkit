namespace EUTK
{
    public class ObjectTreeViewDataSource : TreeViewDataSource
    {
        private TreeItemContainer m_DataContainer;
        private EditorWindowConfigSource m_ConfigSource;
        private bool m_CanBeParent = true;

        public bool canBeParent
        {
            get { return m_CanBeParent; }
            set { m_CanBeParent = value; }
        }

        public TreeItemContainer DataContainer
        {
            get { return m_DataContainer; }
            set { m_DataContainer = value; }
        }

        public EditorWindowConfigSource ConfigSource
        {
            get { return m_ConfigSource; }
            set { m_ConfigSource = value; }
        }

        public ObjectTreeViewDataSource(TreeView treeView, TreeItemContainer dataContainer = null, EditorWindowConfigSource configSource = null) : base(treeView)
        {
            showRootItem = false;
            rootIsCollapsable = false;
            m_DataContainer = dataContainer;
            if (m_DataContainer == null)
            {
                m_DataContainer = new TreeItemContainer();
            }

            m_ConfigSource = configSource;
        }

        public override bool SetExpanded(int id, bool expand)
        {
            if (!base.SetExpanded(id, expand))
                return false;
            return true;
        }

        public override bool IsExpandable(TreeViewItem item)
        {
            return item.hasChildren && (item != this.m_RootItem || this.rootIsCollapsable);
        }

        public override bool CanBeParent(TreeViewItem item)
        {
            return canBeParent;
        }

        public bool IsVisibleRootNode(TreeViewItem item)
        {
            return item.parent == null;
        }

        public override bool IsRenamingItemAllowed(TreeViewItem item)
        {
            if (IsVisibleRootNode(item))
                 return false;
            return base.IsRenamingItemAllowed(item);
        }

        public override void FetchData()
        {
            m_RootItem = new TreeViewItem(int.MaxValue, -1, null, "Invisible Root Item");
            SetExpanded(m_RootItem, true);

            m_RootItem.SetConfigSource(ConfigSource);

            if (m_RootItem.children == null)
            {
                m_RootItem.children = m_DataContainer.ItemList;
            }

            foreach (var item in m_DataContainer.ItemList)
            {
                item.parent = m_RootItem;
            }
        }
    }
}


