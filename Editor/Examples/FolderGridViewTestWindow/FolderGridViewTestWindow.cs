using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderGridViewTestWindow : ViewGroupEditorWindow
    {
        [MenuItem("Tools/Eaxamples/FolderGridViewTestWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            var window = GetWindow<FolderGridViewTestWindow>();
            window.minSize = new Vector2(230, 250);
        }

        private static string DragID = "FolderGridViewTestWindowDrag";
        private static string[] ExtNames = { ".txt", };

        private FolderGridViewGroup m_FolderGridViewGroup;
        private TipsViewGroup m_TipsViewGroup;

        private TreeView m_TreeView;
        private FolderTreeItemContainer m_DataContainer;

        protected override void InitData()
        {
            m_WindowConfigSource = FileConfigSource.CreateFileConfigSource("ViewConfig/TestWindow/config5.txt", true, typeof(FolderGridViewTestWindowSetting1));
            //m_WindowConfigSource = AssetConfigSource.CreateAssetConfigSource("config", true, typeof(FolderGridViewTestWindowSetting2)); 

            m_FolderGridViewGroup = new FolderGridViewGroup(m_LayoutGroupMgr, m_WindowConfigSource, "TreeViewStateConfig", "TreeViewDataContainer", null, DragID);
            m_FolderGridViewGroup.Active = false;

            var gridView = m_FolderGridViewGroup.GetGridView();
            var searchBar = m_FolderGridViewGroup.GetSearchBar();
            m_TreeView = m_FolderGridViewGroup.GetFolderGridViewGroup().GetTreeView();

            m_DataContainer = m_FolderGridViewGroup.GetDataContainer();
            m_DataContainer.ExtNames = ExtNames;
            m_DataContainer.UpdateValidItems();

            gridView.LoadConfig("GvConfig", WindowConfigSource);
            searchBar.LoadConfig("SearchText", WindowConfigSource);

            gridView.GirdItemPopupMenuAction += GirdItemPopupMenuAction;
            gridView.GridViewPopupMenuAction += GridViewPopupMenuAction;

            m_TreeView = m_FolderGridViewGroup.GetFolderGridViewGroup().GetTreeView();
            m_TreeView.useExpansionAnimation = true;
            m_TreeView.contextClickItemCallback = ContextClickItemCallback;

            m_FolderGridViewGroup.UpdateSelectionItemsAction += UpdateSelectionItems;
            m_FolderGridViewGroup.UpdateSearchItemsAction += UpdateSearchItems;
            m_FolderGridViewGroup.UpdateItemsBySearchText();

            m_LayoutGroupMgr.AddViewGroup(m_FolderGridViewGroup);

            m_TipsViewGroup = new TipsViewGroup(m_LayoutGroupMgr);
            m_TipsViewGroup.Active = false;
            m_TipsViewGroup.TipStr = "当前根目录路径不存在,请配置根目录路径";
            m_TipsViewGroup.DrawAction += () =>
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("选择目录"))
                {
                    var path = EditorUtility.OpenFolderPanel("选择目录", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (m_DataContainer.RootFolderPath != path)
                        {
                            m_DataContainer.RootFolderPath = path;
                            m_TreeView.state.expandedIDs = new List<int>();
                            m_TreeView.state.selectedIDs = new List<int>();
                            m_TreeView.state.lastClickedID = Int32.MinValue;
                            m_TreeView.state.scrollPos = Vector2.zero;
                            if (m_TreeView.data != null)
                                m_TreeView.data.ReloadData();
                            m_WindowConfigSource.SetConfigDirty();
                        }
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            };
            m_LayoutGroupMgr.AddViewGroup(m_TipsViewGroup);

            CheckViewVisible();
        }

        protected override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            CheckViewVisible();
        }

        private void CheckViewVisible()
        {
            if (m_FolderGridViewGroup != null)
            {
                if (m_DataContainer != null)
                {
                    var rootPath = m_DataContainer.RootFolderPath;
                    if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                    {
                        m_TipsViewGroup.Active = true;
                        m_FolderGridViewGroup.Active = false;
                    }
                    else
                    {
                        m_TipsViewGroup.Active = false;
                        m_FolderGridViewGroup.Active = true;
                    }
                }
            }
        }

        #region 弹出菜单处理

        private void GridViewPopupMenuAction()
        {
            if (m_TreeView.state.selectedIDs.Count == 0)
                return;

            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Create Folder"), false, () =>
            {
                CreateFolder();
            });

            g.AddItem(new GUIContent("Create txt"), false, () =>
            {
                CreateTxtFile();
            });

            g.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                var item = (m_TreeView.isSearching ? m_TreeView.data.root : m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0])) as FolderTreeViewItem;
                WindowsOSUtility.ExploreDirectory(item.Path);
            });

            g.ShowAsContext();
        }

        private void GirdItemPopupMenuAction(GridItem item)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Ping Item"), false, () =>
            {
                m_FolderGridViewGroup.GetGridView().BeginPing(item.Id);
            });

            g.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                WindowsOSUtility.ExploreDirectory((item as FolderGridItem).Path);
            });

            g.ShowAsContext();
        }

        #endregion

        private void ContextClickItemCallback(int itemId)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Ping Item"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]);
                m_TreeView.Frame(item.id, true, true);

            });

            g.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]) as FolderTreeViewItem;
                WindowsOSUtility.ExploreDirectory(item.Path);
            });

            g.AddItem(new GUIContent("Create Folder"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]) as FolderTreeViewItem;
                CreateFolder(item);
            });

            g.AddItem(new GUIContent("Create txt"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]) as FolderTreeViewItem;
                CreateTxtFile(item);
            });
            g.ShowAsContext();
            Event.current.Use();
        }

        private void UpdateSelectionItems()
        {
            var treeView = m_FolderGridViewGroup.GetFolderGridViewGroup().GetTreeView();
            if (treeView.data == null)
                return;

            var gridViewDataSource = m_FolderGridViewGroup.GetGridViewDataSource();
            var itemList = gridViewDataSource.ItemList;
            itemList.Clear();
            if (treeView.state.selectedIDs != null)
            {
                foreach (var id in treeView.state.selectedIDs)
                {
                    var folderTreeItem = treeView.data.FindItem(id) as FolderTreeViewItem;

                    if (folderTreeItem != null && folderTreeItem.hasChildren)
                    {
                        foreach (var child in folderTreeItem.children)
                        {
                            var gridItem = new FolderGridItem();
                            var childFolderItem = child as FolderTreeViewItem;
                            gridItem.Id = child.id;
                            gridItem.DisplayName = child.displayName;
                            gridItem.Path = childFolderItem.Path;
                            gridItem.ParentId = folderTreeItem.id;
                            gridItem.IsFolder = true;
                            gridItem.IsChildItem = false;
                            itemList.Add(gridItem);
                        }
                    }

                    if (folderTreeItem != null && folderTreeItem.FileList != null)
                    {
                        foreach (var child in folderTreeItem.FileList)
                        {
                            var gridItem = new FileGridItem();
                            gridItem.Id = child.id;
                            gridItem.DisplayName = child.displayName;
                            gridItem.Path = child.Path;
                            gridItem.ParentId = folderTreeItem.id;
                            gridItem.IsFolder = false;
                            gridItem.IsChildItem = false;
                            itemList.Add(gridItem);
                        }
                    }
                }
            }

            SortGridViewItem();
        }

        private void SortGridViewItem()
        {
            var comparator = new AlphanumComparator.AlphanumComparator();
            var gridViewDataSource = m_FolderGridViewGroup.GetGridViewDataSource();
            gridViewDataSource.ItemList.Sort((item, gridItem) =>
            {
                var folderItem1 = item as FolderGridItem;
                var folderItem2 = gridItem as FolderGridItem;
                if (folderItem1.IsFolder != folderItem2.IsFolder)
                    return folderItem2.IsFolder.CompareTo(folderItem1.IsFolder);

                return comparator.Compare(item.DisplayName, gridItem.DisplayName);
            });
        }

        private void UpdateSearchItems(string searchText)
        {
            var gridViewDataSource = m_FolderGridViewGroup.GetGridViewDataSource();
            var itemList = gridViewDataSource.ItemList;
            itemList.Clear();
            GetSearchItems(searchText, m_DataContainer.RootItem, itemList);

            SortGridViewItem();
        }

        private void GetSearchItems(string searchText, FolderTreeViewItem item, List<GridItem> itemList)
        {
            if (item != m_DataContainer.RootItem)
            {
                if (item.displayName.ToLower().Contains(searchText))
                {
                    var gridItem = new FolderGridItem();
                    gridItem.Id = item.id;
                    gridItem.DisplayName = item.displayName;
                    gridItem.Path = item.Path;
                    gridItem.ParentId = item.parent.id;
                    gridItem.IsFolder = true;
                    itemList.Add(gridItem);
                }
            }

            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    GetSearchItems(searchText, child as FolderTreeViewItem, itemList);
                }
            }

            if (item.FileList != null)
            {
                foreach (var child in item.FileList)
                {
                    if (child.displayName.ToLower().Contains(searchText))
                    {
                        var gridItem = new FileGridItem();
                        gridItem.Id = child.id;
                        gridItem.DisplayName = child.displayName;
                        gridItem.Path = child.Path;
                        gridItem.ParentId = item.id;
                        gridItem.IsFolder = false;
                        itemList.Add(gridItem);
                    }
                }
            }
        }

        #region 创建Item

        private void CreateFolder(FolderTreeViewItem parentItem = null)
        {
            var dataContainer = m_FolderGridViewGroup.GetDataContainer();
            var gridView = m_FolderGridViewGroup.GetGridView();

            if (parentItem == null)
            {
                if (!m_TreeView.isSearching)
                {
                    parentItem = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]) as FolderTreeViewItem;
                }
                else
                {
                    parentItem = m_TreeView.data.root as FolderTreeViewItem;
                }
            }

            var folderItem = new FolderGridItem();
            folderItem.IsFolder = true;
            folderItem.ParentId = parentItem.id;
            folderItem.Id = dataContainer.GetAutoID();
            var newPath = EditorFileUtility.CreateNewFolder(parentItem.Path, "New Folder");
            folderItem.Path = newPath;
            var di = new DirectoryInfo(newPath);
            folderItem.DisplayName = di.Name;

            //重命名完成后添加到TreeView中
            m_FolderGridViewGroup.RenameEndWhenCreatingAction = null;
            m_FolderGridViewGroup.RenameEndWhenCreatingAction += () =>
            {
                //添加到TreeView
                var child = new FolderTreeViewItem();
                child.Path = di.FullName.Replace("/", "\\"); ;
                child.IsFolder = true;
                child.id = folderItem.Id;
                child.displayName = di.Name;
                child.parent = parentItem;
                child.SetConfigSource(m_WindowConfigSource);
                parentItem.AddChild(child);
            };

            m_FolderGridViewGroup.GetGridViewDataSource().ItemList.Add(folderItem);

            SortGridViewItem();

            m_FolderGridViewGroup.IsCreatingItem = true;
            gridView.SetSelection(new[] { folderItem.Id }, false);
            gridView.BeginRename(0);
        }

        private void CreateTxtFile(FolderTreeViewItem parentItem = null)
        {
            var dataContainer = m_FolderGridViewGroup.GetDataContainer();
            var gridView = m_FolderGridViewGroup.GetGridView();

            if (parentItem == null)
            {
                if (!m_TreeView.isSearching)
                {
                    parentItem = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]) as FolderTreeViewItem;
                }
                else
                {
                    parentItem = m_TreeView.data.root as FolderTreeViewItem;
                }
            }

            var folderItem = new FileGridItem();
            folderItem.IsFolder = false;
            folderItem.ParentId = parentItem.id;
            folderItem.Id = dataContainer.GetAutoID();
            var newPath = EditorFileUtility.GetNewFile(parentItem.Path, "New Txt", "txt");
            folderItem.Path = newPath;
            var fi = new System.IO.FileInfo(newPath);
            File.Create(newPath).Close();
            folderItem.DisplayName = Path.GetFileNameWithoutExtension(fi.Name);

            //添加到TreeView
            var child = new FolderTreeViewItem();
            child.Path = fi.FullName.Replace("/", "\\"); ;
            child.IsFolder = false;
            child.id = folderItem.Id;
            child.displayName = folderItem.DisplayName;
            child.parent = parentItem;
            child.SetConfigSource(m_WindowConfigSource);

            if (parentItem.FileList == null)
                parentItem.FileList = new List<FolderTreeViewItem>();
            parentItem.FileList.Add(child);

            m_FolderGridViewGroup.GetGridViewDataSource().ItemList.Add(folderItem);

            SortGridViewItem();

            m_FolderGridViewGroup.IsCreatingItem = true;
            gridView.SetSelection(new[] { folderItem.Id }, false);
            gridView.BeginRename(0);
        }

        #endregion
    }
}