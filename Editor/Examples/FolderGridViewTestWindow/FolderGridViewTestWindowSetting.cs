using JsonFx.U3DEditor;

namespace EUTK
{
    public class FolderGridViewTestWindowSetting1 : FileConfigSource
    {
        [JsonMember] protected string m_SearchText;
        [JsonMember] protected GridViewConfig m_GvConfig;
        [JsonMember] protected float m_SplitLineX;
        [JsonMember] protected TreeViewState m_TreeViewStateConfig;
        [JsonMember] protected FolderTreeItemContainer m_TreeViewDataContainer;
        [JsonMember] protected FolderGridItem m_BottomSelectedItem;

        public FolderGridItem BottomSelectedItem
        {
            get { return m_BottomSelectedItem; }
            set { m_BottomSelectedItem = value; }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set { m_SearchText = value; }
        }

        public GridViewConfig GvConfig
        {
            get { return m_GvConfig; }
            set { m_GvConfig = value; }
        }

        public float splitLineX
        {
            get { return m_SplitLineX; }
            set { m_SplitLineX = value; }
        }

        public TreeViewState TreeViewStateConfig
        {
            get { return m_TreeViewStateConfig; }
            set
            {
                m_TreeViewStateConfig = value;
            }
        }

        public FolderTreeItemContainer TreeViewDataContainer
        {
            get { return m_TreeViewDataContainer; }
            set
            {
                m_TreeViewDataContainer = value;
            }
        }
    }

    public class FolderGridViewTestWindowSetting2 : AssetConfigSource
    {
        [JsonMember] protected string m_SearchText;
        [JsonMember] protected GridViewConfig m_GvConfig;
        [JsonMember] protected float m_SplitLineX;
        [JsonMember] protected TreeViewState m_TreeViewStateConfig;
        [JsonMember] protected FolderTreeItemContainer m_TreeViewDataContainer;
        [JsonMember] protected FolderGridItem m_BottomSelectedItem;

        public FolderGridItem BottomSelectedItem
        {
            get { return m_BottomSelectedItem; }
            set { m_BottomSelectedItem = value; }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set { m_SearchText = value; }
        }

        public GridViewConfig GvConfig
        {
            get { return m_GvConfig; }
            set { m_GvConfig = value; }
        }

        public float splitLineX
        {
            get { return m_SplitLineX; }
            set { m_SplitLineX = value; }
        }

        public TreeViewState TreeViewStateConfig
        {
            get { return m_TreeViewStateConfig; }
            set
            {
                m_TreeViewStateConfig = value;
            }
        }

        public FolderTreeItemContainer TreeViewDataContainer
        {
            get { return m_TreeViewDataContainer; }
            set
            {
                m_TreeViewDataContainer = value;
            }
        }
    }
}