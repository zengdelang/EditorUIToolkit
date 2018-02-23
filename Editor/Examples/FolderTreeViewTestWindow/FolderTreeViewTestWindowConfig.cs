using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderTreeViewTestWindowConfig : FileConfigSource
    {
        [JsonMember] [SerializeField] protected TreeViewState m_TreeViewStateConfig;
        [JsonMember] [SerializeField] protected FolderTreeItemContainer m_TreeViewDataContainer;

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
