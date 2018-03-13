using EUTK;
using JsonFx.U3DEditor;
using UnityEngine;

namespace UGT
{
    public class GMCmdWindowSetting : AssetConfigSource
    {
        [HideInInspector] [JsonMember] [SerializeField] protected string m_SearchText;
        [HideInInspector] [JsonMember] [SerializeField] protected float m_SplitLineX;
        [HideInInspector] [JsonMember] [SerializeField] protected bool m_IsMainButtonOn;
        [HideInInspector] [JsonMember] [SerializeField] protected TreeViewState m_TreeViewStateConfig;
        [HideInInspector] [JsonMember] [SerializeField] protected TreeItemContainer m_TreeViewDataContainer;

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

        public TreeViewState treeViewStateConfig
        {
            get { return m_TreeViewStateConfig; }
            set
            {
                m_TreeViewStateConfig = value;
            }
        }

        public TreeItemContainer treeViewDataContainer
        {
            get { return m_TreeViewDataContainer; }
            set
            {
                m_TreeViewDataContainer = value;
            }
        }
    }
}
