using UnityEngine;

namespace EUTK
{
    public class GridViewTestWindowSetting1 : FileConfigSource
    {
        [SerializeField] protected string name1;

        [SerializeField] protected GridViewConfig m_GvConfig;

        [SerializeField] protected ItemDataSource gvds;

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
        [SerializeField] protected string name1;

        [SerializeField] protected GridViewConfig m_GvConfig;

        [SerializeField] protected ItemDataSource gvds;

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