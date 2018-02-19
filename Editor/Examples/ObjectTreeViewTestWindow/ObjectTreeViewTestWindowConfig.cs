using UnityEngine;

namespace EUTK
{
    public class ObjectTreeViewTestWindowConfig : FileConfigSource
    {
        [SerializeField] protected TreeViewState m_TreeViewStateConfig;
        [SerializeField] protected TreeItemContainer m_TreeViewDataContainer;

        public TreeViewState TreeViewStateConfig
        {
            get { return m_TreeViewStateConfig; }
            set
            {
                m_TreeViewStateConfig = value;
            }
        }

        public TreeItemContainer TreeViewDataContainer
        {
            get { return m_TreeViewDataContainer; }
            set
            {
                m_TreeViewDataContainer = value;
            }
        }
    }
}
