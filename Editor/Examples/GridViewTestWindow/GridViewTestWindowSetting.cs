using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    public class GridViewTestWindowSetting1 : FileConfigSource
    {
        [JsonMember] [SerializeField] protected string name1;
        [JsonMember] [SerializeField] protected GridViewConfig m_GvConfig;
        [JsonMember] [SerializeField] protected ItemDataSource gvds;

        public string Name
        {
            get { return name1; }
            set { name1 = value; }
        }


        public GridViewConfig GvConfig
        {
            get { return m_GvConfig; }
            set { m_GvConfig = value; }
        }

        public ItemDataSource Gvds
        {
            get { return gvds; }
            set
            {
                if (value != gvds)
                {
                    gvds = value;
                }
            }
        }
    }

    public class GridViewTestWindowSetting2 : AssetConfigSource
    {
        [JsonMember] [SerializeField] protected string name1;
        [JsonMember] [SerializeField] protected GridViewConfig m_GvConfig;
        [JsonMember] [SerializeField] protected ItemDataSource gvds;

        public string Name
        {
            get { return name1; }
            set { name1 = value; }
        }

        public GridViewConfig GvConfig
        {
            get { return m_GvConfig; }
            set { m_GvConfig = value; }
        }

        public ItemDataSource Gvds
        {
            get { return gvds; }
        }
    }
}