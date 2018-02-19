using System;
using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class ObjectTreeViewGroup : ViewGroup
    {
        private TreeViewState m_TreeViewState;
        private ObjectTreeViewDataSource m_DataSource;
        private ObjectTreeViewGUI m_TreeViewGUI;
        private ObjectTreeViewDragging m_TreeViewDragging;
        private TreeView m_TreeView;

        private bool m_Init;
        private EditorWindowConfigSource m_ConfigSource;
        private TreeItemContainer m_TreeItemContainer;

        public Action DuplicateItemsAction { get; set; }
        public Action<bool> DeleteItemsAction { get; set; }

        public ObjectTreeViewGroup(ViewGroupManager owner, EditorWindowConfigSource configSource, string stateConfigName, string containerConfigName, string dragId = null) : base(owner)
        {
            m_ConfigSource = configSource;
            if (configSource != null)
            {
                m_TreeViewState = configSource.GetValue<TreeViewState>(stateConfigName);
                if (m_TreeViewState == null)
                {
                    m_TreeViewState = new TreeViewState();
                    configSource.SetValue(stateConfigName, m_TreeViewState);
                    configSource.SetConfigDirty();
                }
                m_TreeViewState.SetConfigSource(configSource);

                m_TreeItemContainer = configSource.GetValue<TreeItemContainer>(containerConfigName);
                if (m_TreeItemContainer == null)
                {
                    m_TreeItemContainer = ScriptableObject.CreateInstance<TreeItemContainer>();
                    m_TreeItemContainer.ConfigSource = configSource;
                    configSource.SetValue(containerConfigName, m_TreeItemContainer);
                    configSource.SetConfigDirty();
                }
                else
                {
                    m_TreeItemContainer.ConfigSource = configSource;
                    m_TreeItemContainer.UpdateItemsParent();
                }
            }

            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            m_TreeView = new TreeView(owner.WindowOwner, m_TreeViewState);
            m_DataSource = new ObjectTreeViewDataSource(m_TreeView, m_TreeItemContainer, m_ConfigSource);
            m_TreeViewGUI = new ObjectTreeViewGUI(m_TreeView);
            m_TreeViewDragging = new ObjectTreeViewDragging(m_TreeView, dragId != null ? dragId : m_TreeView.GetHashCode().ToString());

            DeleteItemsAction += DeleteItems;
            DuplicateItemsAction += DuplicateItems;

            m_TreeViewGUI.BeginRenameAction += () =>
            {
                Undo.RegisterCompleteObjectUndo(m_TreeItemContainer, "Rename Item");
            };
            m_TreeViewDragging.PrepareDoDrag += () =>
            {
                Undo.RegisterCompleteObjectUndo(m_TreeItemContainer, "Reparent Item");
            };
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (!m_Init)
            {
                m_Init = true;
                m_TreeView.Init(rect, m_DataSource, m_TreeViewGUI, m_TreeViewDragging);
                m_TreeView.ReloadData();
            }

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                m_TreeView.EndPing();
            }

            m_TreeView.OnEvent();
            m_TreeView.OnGUI(rect, GUIUtility.GetControlID(FocusType.Keyboard));
            HandleCommandEventsForTreeView();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            if (m_TreeView != null)
            {
                m_TreeView.EndNameEditing(true);
                m_TreeView.EndPing();
            }
        }

        public TreeView GetTreeView()
        {
            return m_TreeView;
        }

        public TreeItemContainer GetDataContainer()
        {
            return m_TreeItemContainer;
        }

        private void HandleCommandEventsForTreeView()
        {
            EventType type = Event.current.type;
            switch (type)
            {
                case EventType.ExecuteCommand:
                case EventType.ValidateCommand:
                    bool flag = type == EventType.ExecuteCommand;
                    int[] selection = this.m_TreeView.GetSelection();
                    if (selection.Length == 0)
                        return;
                    if (Event.current.commandName == "Delete" || Event.current.commandName == "SoftDelete")
                    {
                        Event.current.Use();
                        if (flag)
                        {
                            bool askIfSure = Event.current.commandName == "SoftDelete";
                            if (DeleteItemsAction != null)
                                DeleteItemsAction(askIfSure);
                        }
                        GUIUtility.ExitGUI();
                    }
                    else if (Event.current.commandName == "Duplicate")
                    {
                        if (flag)
                        {
                            if (DuplicateItemsAction != null)
                            {
                                Event.current.Use();
                                DuplicateItemsAction();
                            }
                        }
                        else
                            Event.current.Use();
                    }
                    break;
            }
            return;
        }


        private void DeleteItems(bool ask)
        {
            if (m_TreeView.state.selectedIDs != null &&
                m_TreeView.state.selectedIDs.Count > 0)
            {
                Undo.RecordObject(m_TreeItemContainer, "Delete Items");
                /*if (ask)
                {
                    if (!EditorUtility.DisplayDialog("删除操作", "确定删除所选的Item吗?", "Delete", "Cancel"))
                        return;
                }*/

                foreach (var id in m_TreeView.state.selectedIDs)
                {
                    var item = m_TreeView.data.FindItem(id);
                    if (item != null)
                    {
                        item.parent.children.Remove(item);
                    }
                }
                m_TreeView.data.RefreshData();
                SetDirty();
            }
        }

        private void DuplicateItems()
        {
            if (m_TreeView.state.selectedIDs != null &&
                m_TreeView.state.selectedIDs.Count > 0)
            {
                Undo.RecordObject(m_TreeItemContainer, "Duplicate Items");
                m_TreeView.EndNameEditing(true);
                List<int> idList = new List<int>();
                foreach (var id in m_TreeView.state.selectedIDs)
                {
                    var item = m_TreeView.data.FindItem(id);
                    var newItem = JsonReader.Deserialize(JsonWriter.Serialize(item, new JsonWriterSettings() { MaxDepth = Int32.MaxValue }), true) as TreeViewItem;
                    item.parent.AddChild(newItem);
                    UpdateItemInfo(newItem, idList, true);
                }
                m_TreeView.SetSelection(idList.ToArray(), true);
                m_TreeView.data.RefreshData();
                SetDirty();
            }
        }

        private void UpdateItemInfo(TreeViewItem item, List<int> idList, bool first)
        {
            var id = m_TreeItemContainer.GetAutoID();
            item.id = id;
            if (first)
                idList.Add(id);
            item.SetConfigSource(m_ConfigSource);

            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    UpdateItemInfo(child, idList, false);
                }
            }
        }

        private void SetDirty()
        {
            if (m_ConfigSource != null)
                m_ConfigSource.SetConfigDirty();
        }
    }
}