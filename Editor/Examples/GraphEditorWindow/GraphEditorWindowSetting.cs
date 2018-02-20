using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    public class GraphEditorWindowSetting : FileConfigSource
    {
        [JsonMember] protected string m_SearchText;
        [JsonMember] protected Graph m_Graph;
        [JsonMember] protected float m_SplitLineX;
        [JsonMember] protected bool m_IsMainButtonOn;

        public Graph graph
        {
            get { return m_Graph; }
            set { m_Graph = value; }
        }

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