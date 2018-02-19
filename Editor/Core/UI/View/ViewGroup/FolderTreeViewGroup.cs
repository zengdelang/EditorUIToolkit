using System;
using System.Collections.Generic;
using System.IO;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EUTK
{
    public class FolderTreeViewGroup : ViewGroup
    {
        private TreeViewState m_TreeViewState;
        private FolderTreeViewDataSource m_DataSource;
        private FolderTreeViewGUI m_TreeViewGUI;
        private FolderTreeViewDragging m_TreeViewDragging;
        private TreeView m_TreeView;

        private bool m_Init;

        private EditorWindowConfigSource m_ConfigSource;
        private FolderTreeItemContainer m_TreeItemContainer;

        public Action DuplicateItemsAction { get; set; }
        public Action<bool> DeleteItemsAction { get; set; }
        public Action DuplicateItemsDone { get; set; }

        public FolderTreeViewGroup(ViewGroupManager owner, EditorWindowConfigSource configSource, string stateConfigName, string containerConfigName, string dragId = null) : base(owner)
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

                m_TreeItemContainer = configSource.GetValue<FolderTreeItemContainer>(containerConfigName);
                if (m_TreeItemContainer == null)
                {
                    m_TreeItemContainer = new FolderTreeItemContainer();
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
            m_DataSource = new FolderTreeViewDataSource(m_TreeView, m_TreeItemContainer, m_ConfigSource);
            m_TreeViewGUI = new FolderTreeViewGUI(m_TreeView, m_TreeItemContainer);
            m_TreeViewDragging = new FolderTreeViewDragging(m_TreeView, dragId != null ? dragId : m_TreeView.GetHashCode().ToString());

            DeleteItemsAction += DeleteItems;
            DuplicateItemsAction += DuplicateItems;

            m_TreeViewGUI.RenameEndAction += RenameItem;
            m_TreeViewGUI.BeginRenameAction += () =>
            {
                m_TreeView.state.renameOverlay.isRenamingFilename = true;
            };

            m_TreeViewDragging.EndDragAction += (hasError) =>
            {
                if (hasError)
                {
                    m_TreeItemContainer.UpdateValidItems();
                }
                m_TreeView.data.RefreshData();
                m_ConfigSource.SetConfigDirty();
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
            var controllId = GUIUtility.GetControlID(FocusType.Keyboard);
            m_TreeView.OnGUI(rect, controllId);

            if (GUIUtility.keyboardControl == controllId)
            {
                HandleCommandEventsForTreeView();
            }
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

        public override void OnFocus()
        {
            base.OnFocus();

            if (m_TreeView != null && m_TreeView.data != null && m_DataSource != null && m_DataSource.DataContainer != null)
            {
                m_TreeItemContainer.UpdateValidItems();
                m_TreeView.data.RefreshData();
                SetDirty();
            }
        }

        public TreeView GetTreeView()
        {
            return m_TreeView;
        }

        public FolderTreeViewGUI GetFolderTreeViewGUI()
        {
            return m_TreeViewGUI;
        }

        public FolderTreeItemContainer GetDataContainer()
        {
            return m_TreeItemContainer;
        }

        public TreeViewState GeTreeViewState()
        {
            return m_TreeViewState;
        }

        public FolderTreeViewDragging GetTreeViewDragging()
        {
            return m_TreeViewDragging;
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
        }

        private void DeleteItems(bool ask)
        {
            if (m_TreeView.state.selectedIDs != null &&
                m_TreeView.state.selectedIDs.Count > 0)
            {
                var selectedList = m_TreeView.state.selectedIDs;
                bool existRootNode = false;

                foreach (var id in selectedList)
                {
                    if (id == m_TreeView.data.root.id)
                    {
                        existRootNode = true;
                        break;
                    }
                }

                if (existRootNode)
                {
                    EditorUtility.DisplayDialog("无法删除", "根路径不能被删除", "确定");
                }
                else
                {
                    List<FolderTreeViewItem> itemList = null;
                    List<int> parentIdList = new List<int>();
                    if (ask)
                    {
                        string str1 = "是否删除选定的资源?";
                        int num = 3;
                        string str2 = string.Empty;

                        itemList = new List<FolderTreeViewItem>();
                        foreach (var id in selectedList)
                        {
                            var folderItem = m_TreeView.data.FindItem(id) as FolderTreeViewItem;
                            parentIdList.Add(folderItem.parent.id);
                            itemList.Add(folderItem);
                        }
                        for (int index = 0; index < itemList.Count && index < num; ++index)
                        {
                            if (itemList[index] != null)
                                str2 = str2 + "   " + itemList[index].Path + "\n";
                        }
                        if (itemList.Count > 3)
                            str2 += "   ...\n";

                        string message = str2 + "\n该操作无法被撤销";
                        if (!EditorUtility.DisplayDialog(str1, message, " 删除 ", " 取消 "))
                            return;
                    }

                    DeleteItemList(itemList);
                    var selectedIDs = m_TreeView.state.selectedIDs;
                    foreach (var id in parentIdList)
                    {
                        if (!selectedIDs.Contains(id))
                            selectedIDs.Add(id);
                    }
                    if (GetDataContainer().UpdateItemChangedAction != null)
                        GetDataContainer().UpdateItemChangedAction();
                }
            }
        }

        public void DeleteItemList(List<FolderTreeViewItem> itemList)
        {
            try
            {
                if (itemList != null)
                {
                    foreach (var item in itemList)
                    {
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("删除Item出错:" + e);
                m_TreeItemContainer.UpdateValidItems();
            }

            m_TreeView.data.RefreshData();
            SetDirty();
        }

        private void DeleteItem(FolderTreeViewItem item)
        {
            if (item.IsFolder)
            {
                if (Directory.Exists(item.Path))
                {
                    EditorFileUtility.DeleteToTrash(item.Path);
                }

                if (item.parent != null)
                {
                    item.parent.children.Remove(item);
                }
            }
            else
            {
                if (File.Exists(item.Path))
                {
                    EditorFileUtility.DeleteToTrash(item.Path);
                }

                if (item.parent != null)
                {
                    (item.parent as FolderTreeViewItem).FileList.Remove(item);
                }
            }
        }

        private void DuplicateItems()
        {
            if (m_TreeView.state.selectedIDs != null &&
                m_TreeView.state.selectedIDs.Count > 0)
            {
                List<int> idList = new List<int>();
                try
                {
                    m_TreeView.EndNameEditing(true);
                    foreach (var id in m_TreeView.state.selectedIDs)
                    {
                        if (id == m_TreeView.data.root.id)
                            continue;

                        var item = m_TreeView.data.FindItem(id);
                        var newItem = JsonReader.Deserialize(JsonWriter.Serialize(item, new JsonWriterSettings() { MaxDepth = Int32.MaxValue }), true) as FolderTreeViewItem;
                        var newPath = EditorFileUtility.GetNewFolder(newItem.Path);

                        FileUtil.CopyFileOrDirectory(newItem.Path, newPath);
                        newItem.id = m_TreeItemContainer.GetAutoID();
                        idList.Add(newItem.id);
                        newItem.Path = newPath;
                        newItem.displayName = new DirectoryInfo(newPath).Name;
                        item.parent.AddChild(newItem);

                        newItem.FileList = null;
                        newItem.children = null;

                        var comparator = new AlphanumComparator.AlphanumComparator();
                        item.parent.children.Sort((viewItem, treeViewItem) =>
                        {
                            return comparator.Compare(viewItem.displayName, treeViewItem.displayName);
                        });
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("复制item出错:" + e);
                }
                finally
                {
                    m_TreeItemContainer.UpdateValidItems();
                    m_TreeView.SetSelection(idList.ToArray(), true);
                    m_TreeView.data.RefreshData();
                    SetDirty();

                    if (DuplicateItemsDone != null)
                        DuplicateItemsDone();
                }
            }
        }

        private void SetDirty()
        {
            if (m_ConfigSource != null)
                m_ConfigSource.SetConfigDirty();
        }


        public void RenameItem(TreeViewItem item, string name)
        {
            var result = (item as FolderTreeViewItem).Rename(name);
            if (!result)
                return;

            item.displayName = name;
            if (item.parent != null && item.parent.hasChildren)
            {
                var comparator = new AlphanumComparator.AlphanumComparator();
                item.parent.children.Sort((viewItem, treeViewItem) =>
                {
                    return comparator.Compare(viewItem.displayName, treeViewItem.displayName);
                });
                m_TreeView.data.RefreshData();
            }
        }
    }
}