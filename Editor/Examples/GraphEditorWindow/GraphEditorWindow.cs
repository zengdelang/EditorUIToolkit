using System;
using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{ 
    public class Graph1 : Graph
    {
        public override Type baseNodeType
        {
            get { return typeof(Node1); }
        }
    }

    public abstract class Node1 : Node
    {
        public override Type outConnectionType { get { return typeof(Connection1); } }
    }

    [Category("Composites")]
    [Description("Works like a normal Selector, but when a child node returns Success, that child will be moved to the end.\nAs a result, previously Failed children will always be checked first and recently Successful children last")]
    public class Node11 : Node1
    {
        [JsonMember] [SerializeField] protected string haha;
        [JsonMember] [SerializeField] public Dictionary<string, string> ss;
        [JsonMember] [SerializeField] public List<string> dd;

        public string dsew;
        public int xxx;
        public sealed override int maxOutConnections { get { return -1; } }

        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Node12 : Node1
    {
        [JsonMember] [SerializeField] protected string haha11;

        public sealed override int maxOutConnections
        {
            get { return 5; }
        }

        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Node13 : Node1
    {
        [JsonMember] [SerializeField] protected string haha11;

        public sealed override int maxOutConnections
        {
            get { return 5; }
        }

        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Node14 : Node1
    {
        [JsonMember] [SerializeField] public string haha11 = "";

        public sealed override int maxOutConnections
        {
            get { return 0; }
        }

        public override int maxInConnections
        {
            get { return -1; }
        }

        protected override void OnNodeGUI()
        {
            base.OnNodeGUI();
            haha11 = EditorGUILayout.TextArea(haha11);
            if (GUILayout.Button("执行"))
            {
                EditorWindow.focusedWindow.ShowNotification(new GUIContent("11322"));
            }
        }
    }

    public class Connection1 : Connection
    {
    
    }

    public class GMCategoryItem : TreeViewItem
    {
        [JsonMember] [SerializeField] public Graph graph;

        public GMCategoryItem(int id, int depth, TreeViewItem parent, string displayName) : base(id, depth, parent, displayName)
        {
            graph = ScriptableObject.CreateInstance<Graph1>();
        }

        public GMCategoryItem()
        {
            
        }
    }

    public class GraphEditorWindow : ViewGroupEditorWindow
    {
        [MenuItem("Tools/Eaxamples/GraphEditorWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            GetWindow<GraphEditorWindow>();
        }

        private TreeView m_TreeView;
        private GraphViewGroup m_GraphViewGroup;
        private GraphView m_GraphView;
        private TreeItemContainer m_DataContainer;

        protected override void InitData()
        {
            WindowConfigSource = FileConfigSource.CreateFileConfigSource("ViewConfig/TestWindow/config6.txt", true, typeof(GraphEditorWindowSetting));

            m_GraphViewGroup = new GraphViewGroup(m_LayoutGroupMgr, WindowConfigSource, "TreeViewStateConfig", "TreeViewDataContainer");
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
            m_GraphViewGroup.objectTreeViewGroup.DeleteDoneAction += () =>
            {
                TreeViewSelectionChanged(m_TreeView.GetSelection());
            };
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
        }

        private void ContextClickItemCallback(int itemId)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Create Item"), false, () =>
            {
                var item = m_TreeView.data.FindItem(itemId);
                var id = m_DataContainer.GetAutoID();
                var newItem = new GMCategoryItem(id, 0, item, "New Item");
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
            g.AddItem(new GUIContent("Create Item"), false, () =>
            {
                TreeViewItem item = m_TreeView.data.root; ;

                var id = m_DataContainer.GetAutoID();
                var newItem = new GMCategoryItem(id, 0, item, "New Item");
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
                var item = m_TreeView.FindItem(id) as GMCategoryItem;
                if (item != null && item.graph == m_GraphView.currentGraph)
                {
                    return;
                }
            }

            var firstItem = m_TreeView.FindItem(ids[0]) as GMCategoryItem;
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