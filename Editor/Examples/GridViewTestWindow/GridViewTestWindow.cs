using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class GenericGridItem : GridItem
    {
        [SerializeField] public bool isFolder;

        public override Texture Texture
        {
            get
            {
                return Resources.Load<Texture>("config");
            }
        }
    }

    public class GenericGridItem1 : GridItem
    {
        [SerializeField] public bool test;
        [SerializeField] public int flag;

        public override Texture Texture
        {
            get
            {
                return Resources.Load<Texture>("config");
            }
        }
    }

    public class GridViewTestWindow : ViewGroupEditorWindow
    {
        private ItemDataSource m_ItemDataSource;
        private GridViewGroup m_GridViewGroup;

        [MenuItem("Tools/Eaxamples/GridViewTestWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            GetWindow<GridViewTestWindow>();
        }

        protected override void InitData()
        {
            m_WindowConfigSource = FileConfigSource.CreateFileConfigSource("ViewConfig/TestWindow/config.txt", true, typeof(GridViewTestWindowSetting1));

            m_ItemDataSource = m_WindowConfigSource.GetValue<ItemDataSource>("Gvds");
            if (m_ItemDataSource == null)
            {
                m_ItemDataSource = new ItemDataSource();
                m_WindowConfigSource.SetValue("Gvds", m_ItemDataSource);
            }
            m_ItemDataSource.SetConfigSource(WindowConfigSource);

            m_GridViewGroup = new GridViewGroup(m_LayoutGroupMgr, m_ItemDataSource);

            var gridView = m_GridViewGroup.GetGridView();
            var searchBar = m_GridViewGroup.GetSearchBar();

            gridView.LoadConfig("GvConfig", WindowConfigSource);
            searchBar.LoadConfig("Name", WindowConfigSource);

            m_GridViewGroup.GetGridView().GirdItemPopupMenuAction += GirdItemPopupMenuAction;
            m_GridViewGroup.GetGridView().GridViewPopupMenuAction += GridViewPopupMenuAction;

            m_GridViewGroup.UpdateItemsBySearchText();

            m_LayoutGroupMgr.AddViewGroup(m_GridViewGroup);
        }

        #region 弹出菜单处理

        private void GridViewPopupMenuAction()
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("AddItem"), false, () =>
            {
                var item = new GenericGridItem1()
                {
                    DisplayName = "new Item",
                };
                AddGridItem(item);
            });

            g.AddItem(new GUIContent("AddItem1"), false, () =>
            {
                var item = new GenericGridItem()
                {
                    DisplayName = "new Item1",
                };
                AddGridItem(item);
            });

            g.ShowAsContext();
        }

        private void GirdItemPopupMenuAction(GridItem item)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Ping Item"), false, () =>
            {
                m_GridViewGroup.GetGridView().BeginPing(item.Id);
            });

            g.AddItem(new GUIContent("Add Child"), false, () =>
            {
                var item1 = new GenericGridItem()
                {
                    DisplayName = "new Item1",
                };

                AddGridItemChild(item, item1);
            });
            g.ShowAsContext();
        }

        #endregion

        private void AddGridItem(GridItem item)
        {
            m_ItemDataSource.AddItem(item);
            m_GridViewGroup.UpdateItemsBySearchText(true);
            m_GridViewGroup.GetGridView().SetSelection(new[] { item.Id }, false);
            m_GridViewGroup.GetGridView().BeginRename(0);
            m_GridViewGroup.IsCreatingItem = true;
        }

        private void AddGridItemChild(GridItem parent, GridItem child)
        {
            m_ItemDataSource.AddChildItem(parent, child);
            m_GridViewGroup.UpdateItemsBySearchText();
        }
    }

}