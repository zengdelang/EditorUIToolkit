using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class ObjectTreeViewTestWindow : ViewGroupEditorWindow
    {
        [MenuItem("Tools/Eaxamples/ObjectTreeViewTestWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            GetWindow<ObjectTreeViewTestWindow>();
        }

        private ObjectTreeViewGroup m_ObjectTreeViewGroup;
        private TreeView m_TreeView;
        private TreeItemContainer m_DataContainer;

        protected override void InitData()
        {
            WindowConfigSource = FileConfigSource.CreateFileConfigSource("ViewConfig/TestWindow/config3.txt", true, typeof(ObjectTreeViewTestWindowConfig));

            m_ObjectTreeViewGroup = new ObjectTreeViewGroup(m_LayoutGroupMgr, WindowConfigSource, "TreeViewStateConfig", "TreeViewDataContainer");

            m_DataContainer = m_ObjectTreeViewGroup.GetDataContainer();
            m_TreeView = m_ObjectTreeViewGroup.GetTreeView();
            m_TreeView.useExpansionAnimation = true;
            m_TreeView.deselectOnUnhandledMouseDown = true;
            m_TreeView.contextClickItemCallback = ContextClickItemCallback;
            m_TreeView.contextClickOutsideItemsCallback = ContextClickOutsideItemsCallback;

            m_LayoutGroupMgr.AddViewGroup(m_ObjectTreeViewGroup);

            Undo.undoRedoPerformed += UndoRedoPerformedAction;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Undo.undoRedoPerformed -= UndoRedoPerformedAction;
        }

        private void UndoRedoPerformedAction()
        {
            WindowConfigSource.SetConfigDirty();
            m_TreeView.data.RefreshData();
        }

        private void ContextClickItemCallback(int itemId)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Create Item"), false, () =>
            {
                Undo.RecordObject(m_DataContainer, "Create Item");
                var item = m_TreeView.data.FindItem(itemId);
                var id = m_DataContainer.GetAutoID();
                var newItem = new TreeViewItem(id, item.depth + 1, item, "New Item");
                newItem.SetConfigSource(WindowConfigSource);
                item.AddChild(newItem);
                m_TreeView.SetSelection(new int[] { newItem.id }, true);
                m_TreeView.data.RefreshData();
            });

            g.AddItem(new GUIContent("Ping Item"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]);
                m_TreeView.Frame(item.id, true, true);

            });
            g.ShowAsContext();
            Event.current.Use();
        }

        private void ContextClickOutsideItemsCallback()
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Create Item"), false, () =>
            {
                Undo.RecordObject(m_DataContainer, "Create Item");
                TreeViewItem item = null;
                if (m_TreeView.state.selectedIDs != null &&
                    m_TreeView.state.selectedIDs.Count > 0)
                    item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]);
                if (item == null)
                {
                    item = m_TreeView.data.root;
                }
                var id = m_DataContainer.GetAutoID();
                var newItem = new TreeViewItem(id, 0, item, "New Item");
                newItem.SetConfigSource(WindowConfigSource);
                item.AddChild(newItem);
                m_TreeView.SetSelection(new int[] { newItem.id }, true);
                m_TreeView.data.RefreshData();
                WindowConfigSource.SetConfigDirty();
            });
            g.ShowAsContext();
        }
    }
}