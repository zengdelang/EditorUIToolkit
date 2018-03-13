using EUTK;
using UnityEditor;
using UnityEngine;

namespace UGT
{
    public class GMCmdWindow : ViewGroupEditorWindow
    {
        [MenuItem("Tools/GameTools/GMCmdWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            var window =GetWindow<GMCmdWindow>();
            window.titleContent = new GUIContent("GMCmdWindow");
        }

        private TreeView m_TreeView;
        private GraphViewGroup m_GraphViewGroup;
        private GraphView m_GraphView;
        private TreeItemContainer m_DataContainer;

        protected override void InitData()
        {
            WindowConfigSource = AssetConfigSource.CreateAssetConfigSource("GMCmdWindowConfig", true, typeof(GMCmdWindowSetting));

            m_GraphViewGroup = new GraphViewGroup(m_LayoutGroupMgr, WindowConfigSource, "treeViewStateConfig", "treeViewDataContainer");
            m_GraphView = m_GraphViewGroup.graphView;

            m_GraphViewGroup.showMainButton = true;
            m_GraphViewGroup.mainButtonName = "Categories";

            m_DataContainer = m_GraphViewGroup.treeItemContainer;
            m_TreeView = m_GraphViewGroup.treeView;
            m_TreeView.useExpansionAnimation = true;
            m_TreeView.contextClickItemCallback = ContextClickItemCallback;
            m_TreeView.contextClickOutsideItemsCallback = ContextClickOutsideItemsCallback;
            m_TreeView.selectionChangedCallback += TreeViewSelectionChanged;

            m_LayoutGroupMgr.AddViewGroup(m_GraphViewGroup);

            m_GraphViewGroup.objectTreeViewGroup.needUndo = false;
            m_GraphViewGroup.objectTreeViewGroup.OnGUIInitAction += () =>
            {
                TreeViewSelectionChanged(m_TreeView.GetSelection());
            };
            m_GraphViewGroup.objectTreeViewGroup.GetTreeViewGUI().RenameEndAction += (item, text) =>
            {
                m_TreeView.data.OnSearchChanged();
            };

            m_GraphViewGroup.searchBar.OnTextChangedAction += OnTextChangedAction;
            m_GraphViewGroup.searchBar.LoadConfig("searchText", WindowConfigSource);
            m_TreeView.state.searchString = m_GraphViewGroup.searchBar.SearchText;
            m_TreeView.dragEndedCallback += DragEnedCallback;
        }

        private void DragEnedCallback(int[] draggedIDs, bool draggedItemsFromOwnTreeView)
        {
            TreeViewSelectionChanged(m_TreeView.GetSelection());
        }

        private void ContextClickItemCallback(int itemId)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Add Child Category"), false, () =>
            {
                var item = m_TreeView.data.FindItem(itemId);
                var id = m_DataContainer.GetAutoID();
                var newItem = new GMCategoryTreeItem(id, 0, item, "New Child Category");
                newItem.SetConfigSource(WindowConfigSource);

                item.AddChild(newItem);
                m_TreeView.SetSelection(new int[] { newItem.id }, true);
                m_TreeView.data.RefreshData();

                TreeViewSelectionChanged(new int[] { newItem.id });
                WindowConfigSource.SetConfigDirty();
            });

            g.ShowAsContext();
            Event.current.Use();
        }

        private void ContextClickOutsideItemsCallback()
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Add GM Category"), false, () =>
            {
                TreeViewItem item = m_TreeView.data.root; ;

                var id = m_DataContainer.GetAutoID();
                var newItem = new GMCategoryTreeItem(id, 0, item, "New Category");
                newItem.SetConfigSource(WindowConfigSource);

                item.AddChild(newItem);
                m_TreeView.SetSelection(new int[] { newItem.id }, true);
                m_TreeView.data.RefreshData();

                TreeViewSelectionChanged(new int[] { newItem.id });
                WindowConfigSource.SetConfigDirty();
            });
            g.ShowAsContext();
        }

        private void TreeViewSelectionChanged(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                m_GraphView.currentGraph = null;
                return;
            }

            foreach (var id in ids)
            {
                var item = m_TreeView.FindItem(id) as GMCategoryTreeItem;
                if (item != null && item.graph == m_GraphView.currentGraph)
                {
                    return;
                }
            }

            var firstItem = m_TreeView.FindItem(ids[0]) as GMCategoryTreeItem;
            if (firstItem == null)
            {
                m_GraphView.currentGraph = null;
            }
            else
            {
                m_GraphView.currentGraph = firstItem.graph;
            }
        }

        private void OnTextChangedAction(string searchText)
        {
            if (m_TreeView != null && m_TreeView.data != null)
            {
                m_TreeView.state.searchString = searchText;
                m_TreeView.data.OnSearchChanged();
            }
        }
    }
}

