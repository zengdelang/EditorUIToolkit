using UnityEngine;

namespace EUTK
{
    public class GraphEditorWindowSetting : FileConfigSource
    {
        [SerializeField] protected string m_SearchText;
        [SerializeField] protected float m_SplitLineX;
        [SerializeField] protected bool m_IsMainButtonOn;
        [SerializeField] protected TreeViewState m_TreeViewStateConfig;
        [SerializeField] protected TreeItemContainer m_TreeViewDataContainer;

        public string searchText
        {
            get { return m_SearchText; }
            set { m_SearchText = value; }
        }

        public float splitLineX
        {
            get { return m_SplitLineX; }
            set { m_SplitLineX = value; }
        }

        public bool isMainButtonOn
        {
            get { return m_IsMainButtonOn; }
            set { m_IsMainButtonOn = value; }
        }


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