using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class GraphViewGroup : ViewGroup
    {
        public static readonly string mainButtonOnConfigKey = "isMainButtonOn";

        private float HorizontalSplitLineMinX = 109;

        protected float m_SplitLineStartPosX = 146; //分割线初始的x位置
        protected HorizontalSplitLine m_HorizontalSplitLine; //区域分割线

        protected GraphView m_GraphView;
        protected ObjectTreeViewGroup m_ObjectTreeViewGroup;
        protected SearchBar m_SearchBar;
        protected bool m_IsMainButtonOn;

        protected TreeView m_TreeView;
        protected EditorWindowConfigSource m_WindowConfig;
        protected TreeItemContainer m_TreeItemContainer;
        protected TreeViewState m_TreeViewState;

        public GraphView graphView
        {
            get { return m_GraphView; }
        }

        public TreeItemContainer treeItemContainer
        {
            get { return m_TreeItemContainer; }
        }

        public SearchBar searchBar
        {
            get { return m_SearchBar; }
        }

        public TreeView treeView
        {
            get { return m_ObjectTreeViewGroup.GetTreeView(); }
        }

        public bool isMainButtonOn
        {
            get { return m_IsMainButtonOn; }
            set
            {
                if (m_IsMainButtonOn != value)
                {
                    m_IsMainButtonOn = value;
                    if (m_WindowConfig != null)
                    {
                        if (m_WindowConfig.FindProperty(mainButtonOnConfigKey) != null)
                        {
                            m_WindowConfig.SetValue(mainButtonOnConfigKey, m_IsMainButtonOn);
                        }
                        m_WindowConfig.SetConfigDirty();
                    }
                }               
            }
        }

        public bool showMainButton { get; set; }

        public string mainButtonName { get; set; }

        public ObjectTreeViewGroup objectTreeViewGroup
        {
            get { return m_ObjectTreeViewGroup; }
        }

        public GraphViewGroup(ViewGroupManager owner, EditorWindowConfigSource configSource, string stateConfigName,
            string containerConfigName, string dragId = null) : base(owner)
        {
            m_WindowConfig = configSource;
            if (m_WindowConfig != null)
            {
                if (m_WindowConfig.FindProperty(mainButtonOnConfigKey) != null)
                {
                    m_IsMainButtonOn = m_WindowConfig.GetValue<bool>(mainButtonOnConfigKey);
                }

                if (configSource.FindProperty(stateConfigName) != null)
                    m_TreeViewState = configSource.GetValue<TreeViewState>(stateConfigName);
                if (m_TreeViewState == null)
                {
                    m_TreeViewState = new TreeViewState();
                    if (configSource.FindProperty(stateConfigName) != null)
                        configSource.SetValue(stateConfigName, m_TreeViewState);
                    configSource.SetConfigDirty();
                }
                m_TreeViewState.SetConfigSource(configSource);

                if (configSource.FindProperty(containerConfigName) != null)
                    m_TreeItemContainer = configSource.GetValue<TreeItemContainer>(containerConfigName);
                if (m_TreeItemContainer == null)
                {
                    m_TreeItemContainer = ScriptableObject.CreateInstance<TreeItemContainer>();
                    m_TreeItemContainer.ConfigSource = configSource;
                    if (configSource.FindProperty(containerConfigName) != null)
                        configSource.SetValue(containerConfigName, m_TreeItemContainer);
                    configSource.SetConfigDirty();
                }
                else
                {
                    m_TreeItemContainer.ConfigSource = configSource;
                    m_TreeItemContainer.UpdateItemsParent();
                }
            }

            m_GraphView = new GraphView(owner, configSource);

            m_HorizontalSplitLine = new HorizontalSplitLine(m_SplitLineStartPosX, HorizontalSplitLineMinX);
            m_HorizontalSplitLine.ConfigSource = configSource;

            m_ObjectTreeViewGroup = new ObjectTreeViewGroup(owner, configSource, stateConfigName, containerConfigName,
                dragId);
            m_TreeView = m_ObjectTreeViewGroup.GetTreeView();
 
            m_SearchBar = new SearchBar(owner);
            m_SearchBar.OnGUIAction += ShowBarGUI;
        }

        public override void OnFocus()
        {
            base.OnFocus();

            if (m_ObjectTreeViewGroup != null)
                m_ObjectTreeViewGroup.OnFocus();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            if (m_TreeView != null)
            {
                if (m_TreeView.gui != null)
                    m_TreeView.EndPing();
                m_TreeView.EndNameEditing(true);
            }
        }

        public override void Update()
        {
            base.Update();
            if(m_GraphView != null)
                m_GraphView.Update();
        }

        public override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            if (m_GraphView != null)
                m_GraphView.OnInspectorUpdate();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            var position = Owner.WindowOwner.position;
            var toolBarRect = new Rect(0f, 0f, position.width, EditorStyles.toolbar.fixedHeight);
            var graphViewRect = new Rect(0, EditorStyles.toolbar.fixedHeight, position.width,
                position.height - EditorStyles.toolbar.fixedHeight);
            var treeViewRect = new Rect();

            if (showMainButton && isMainButtonOn)
            {
                graphViewRect = new Rect(m_HorizontalSplitLine.PositionX, EditorStyles.toolbar.fixedHeight,
                    position.width - m_HorizontalSplitLine.PositionX,
                    position.height - EditorStyles.toolbar.fixedHeight);
                treeViewRect = new Rect(0, EditorStyles.toolbar.fixedHeight, m_HorizontalSplitLine.PositionX,
                    position.height - EditorStyles.toolbar.fixedHeight);
            }

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (m_TreeView != null)
                {
                    if (m_TreeView.gui != null)
                        m_TreeView.EndPing();
                    m_TreeView.EndNameEditing(true);
                }
            }

            if (showMainButton && isMainButtonOn)
            {
                m_HorizontalSplitLine.ResizeHandling(0, position.width, position.height, 0, 2);
                m_ObjectTreeViewGroup.OnGUI(treeViewRect);
                m_HorizontalSplitLine.OnGUI(0, EditorStyles.toolbar.fixedHeight, position.height);
            }

            m_SearchBar.OnGUI(toolBarRect);
            m_GraphView.OnGUI(graphViewRect);
        }

        protected void ShowBarGUI()
        {
            if (showMainButton)
            {
                isMainButtonOn = GUILayout.Toggle(m_IsMainButtonOn, mainButtonName, "ToolbarButton");
            }
        }
    }
}