using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderGridViewGroup : ViewGroup
    {
        public string BottomSelectedItem = "BottomSelectedItem";

        private float HorizontalSplitLineMinX = 109;

        protected GridView m_GridView;
        protected GridViewDataSource m_GridViewDataSource;

        protected SearchBar m_SearchBar;
        protected BottomBar m_BottomBar;
        protected BreadcrumbBar m_BreadcrumbBar;

        protected FolderTreeViewGroup m_FolderTreeViewGroup;

        protected float m_SplitLineStartPosX = 146; //分割线初始的x位置
        protected HorizontalSplitLine m_HorizontalSplitLine; //区域分割线

        private bool m_Init;

        private bool m_IsCreatingItem;
        private ItemDataSource m_ItemDataSource;
        private EditorWindowConfigSource m_ConfigSource;

        private FolderGridItem m_BottomBarSelectedItem;
        private float m_BottomBarRectWidth;
        private TreeView m_TreeView;

        private GUIContent m_EmptyFolderText = new GUIContent("This folder is empty");
        public GUIStyle selectedPathLabel = "Label";

        public Action<bool> GridViewDeleteAction { get; set; }

        public Action DuplicateItemsAction { get; set; }
        public Action UpdateSelectionItemsAction { get; set; }
        public Action<string> UpdateSearchItemsAction { get; set; }
        public Action RenameEndWhenCreatingAction { get; set; }

        public TreeView FolderTreeView
        {
            get { return m_TreeView; }
        }

        public bool IsCreatingItem
        {
            get { return m_IsCreatingItem; }
            set
            {
                m_IsCreatingItem = value;
            }
        }

        public FolderGridViewGroup(ViewGroupManager owner, EditorWindowConfigSource configSource, string stateConfigName, string containerConfigName, string dragId = null) : base(owner)
        {
            m_ConfigSource = configSource;

            m_ItemDataSource = new ItemDataSource();
            m_SearchBar = new SearchBar(owner);
            m_SearchBar.UpOrDownArrowPressedAction += SearchBarUpOrDownArrowPressedAction;
            m_SearchBar.OnTextChangedAction += (str) => UpdateItemsBySearchText();

            dragId = dragId != null ? dragId : GetHashCode().ToString();
            m_FolderTreeViewGroup = new FolderTreeViewGroup(owner, configSource, stateConfigName, containerConfigName, dragId);

            m_GridViewDataSource = new GridViewDataSource();
            var layout = new GenericGridLayouter(m_GridViewDataSource);
            var viewHandler = new FolderGridViewHandler(layout.DataSource);
            viewHandler.TreeViewDragging = m_FolderTreeViewGroup.GetTreeViewDragging();
            m_GridView = new GridView(owner, layout, viewHandler);
            m_GridView.KeyboardCallback += ListAreaKeyboardCallback;
            m_GridView.ItemExpandedAction += ItemExpandedAction;
            m_GridView.BeginRenameAction += ItemBeginRenameAction;
            m_GridView.RenameEndAction += ItemRenameEndAction;
            m_GridView.ItemDoubleClickAction += ItemDoubleClick;
            m_GridView.ItemSelectedAction += GridViewItemSelected;
            m_GridView.ViewHandler.GenericDragId = dragId;
            m_GridView.GridSizeChangedAction += (size) =>
            {
                m_BottomBar.Value = size;
            };

            m_BreadcrumbBar = new BreadcrumbBar(owner);
            m_BreadcrumbBar.ShowFolderContentsAction += (id) =>
            {
                m_TreeView.SetSelection(new int[] { id }, false);
                UpdateBreadcrumbBarContents();
                UpdateGridViewContent();
            };

            m_BottomBar = new BottomBar(owner);
            m_BottomBar.Value = m_GridView.GridSize;
            m_BottomBar.MinValue = m_GridView.ViewLayouter.LayoutParams.MinGridSize;
            m_BottomBar.MaxValue = m_GridView.ViewLayouter.LayoutParams.MaxGridSize;
            m_BottomBar.OnValueChangedAction += (size) =>
            {
                m_GridView.GridSize = size;
            };

            GridViewDeleteAction += DeleteGridItems;

            m_TreeView = m_FolderTreeViewGroup.GetTreeView();
            m_BreadcrumbBar.FolderTreeView = m_TreeView;
            m_FolderTreeViewGroup.GetFolderTreeViewGUI().RenameEndAction += (item, name) =>
            {
                UpdateBreadcrumbBarContents();
            };

            m_HorizontalSplitLine = new HorizontalSplitLine(m_SplitLineStartPosX, HorizontalSplitLineMinX);
            m_HorizontalSplitLine.PositionChangedAction += RefreshSplittedSelectedPath;
            m_HorizontalSplitLine.ConfigSource = configSource;
            m_ItemDataSource.SetConfigSource(configSource);

            m_TreeView.selectionChangedCallback += FolderTreeViewSelectionChanged;
            m_FolderTreeViewGroup.GetTreeViewDragging().EndDragAction += (hasError) =>
            {
                UpdateGridViewContent();
            };
            m_FolderTreeViewGroup.DuplicateItemsDone += UpdateGridViewContent;

            GetDataContainer().UpdateItemChangedAction += () =>
            {
                UpdateGridViewContent();
                CheckBottomBarItemValidity();
                UpdateBreadcrumbBarContents();
            };

            DuplicateItemsAction += DuplicateItemGridView;
        }

        public GridView GetGridView()
        {
            return m_GridView;
        }

        public GridViewDataSource GetGridViewDataSource()
        {
            return m_GridViewDataSource;
        }

        public SearchBar GetSearchBar()
        {
            return m_SearchBar;
        }

        public BottomBar GetBottomBar()
        {
            return m_BottomBar;
        }

        public HorizontalSplitLine GetHorizontalSplitLine()
        {
            return m_HorizontalSplitLine;
        }

        public FolderTreeViewGroup GetFolderGridViewGroup()
        {
            return m_FolderTreeViewGroup;
        }

        public FolderTreeItemContainer GetDataContainer()
        {
            return m_FolderTreeViewGroup.GetDataContainer();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            var bottomHeight = GetBottomBarHeight();
            var listHeaderHeight = 18;

            var position = Owner.WindowOwner.position;
            var leftRect = new Rect(0, EditorStyles.toolbar.fixedHeight, m_HorizontalSplitLine.PositionX, position.height - EditorStyles.toolbar.fixedHeight);
            var searchBarRect = new Rect(0f, 0f, position.width, EditorStyles.toolbar.fixedHeight);
            var gridViewRect = new Rect(m_HorizontalSplitLine.PositionX, EditorStyles.toolbar.fixedHeight + listHeaderHeight, position.width - m_HorizontalSplitLine.PositionX, position.height - EditorStyles.toolbar.fixedHeight - bottomHeight - listHeaderHeight);
            m_BottomBarRectWidth = position.width - m_HorizontalSplitLine.PositionX;
            var bottomBarRect = new Rect(m_HorizontalSplitLine.PositionX, position.height - bottomHeight, position.width - m_HorizontalSplitLine.PositionX, bottomHeight);
            var listHeaderRect = new Rect(gridViewRect.x, EditorStyles.toolbar.fixedHeight, gridViewRect.width, listHeaderHeight);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                m_GridView.EndPing();
                m_GridView.EndRename(true);
                m_TreeView.EndPing();
                m_TreeView.EndNameEditing(true);
            }

            m_HorizontalSplitLine.ResizeHandling(0, position.width, position.height, 0, 2);

            m_FolderTreeViewGroup.OnGUI(leftRect);
            m_SearchBar.OnGUI(searchBarRect);
            m_GridView.OnGUI(gridViewRect);
            m_BottomBar.OnGUI(bottomBarRect);

            m_HorizontalSplitLine.OnGUI(0, EditorStyles.toolbar.fixedHeight, position.height);

            HandleCommandEvents();

            if (!m_Init)
            {
                m_Init = true;
                var state = m_FolderTreeViewGroup.GeTreeViewState();
                if (state.selectedIDs.Count == 0)
                    state.selectedIDs.Add(GetDataContainer().RootItem.id);

                UpdateGridViewContent();
                RefreshBottomBarSelectedItem(m_GridView.ViewConfig.SelectedItemIdList.ToArray());
                UpdateBreadcrumbBarContents();
            }

            m_BreadcrumbBar.KeyboardControlID = m_GridView.KeyboardControlID;
            m_BreadcrumbBar.OnGUI(listHeaderRect);

            if (!m_TreeView.isSearching && m_GridViewDataSource.Count == 0)
            {
                Vector2 vector2 = EditorStyles.label.CalcSize(m_EmptyFolderText);
                Rect position2 = new Rect(gridViewRect.x + 2f + Mathf.Max(0.0f, (gridViewRect.width - vector2.x) * 0.5f), gridViewRect.y + 10f, vector2.x, 20f);
                using (new EditorGUI.DisabledScope(true))
                    GUI.Label(position2, m_EmptyFolderText, EditorStyles.label);
            }
        }

        protected void UpdateGridViewContent()
        {
            if (string.IsNullOrEmpty(m_SearchBar.SearchText))
            {
                if (UpdateSelectionItemsAction != null)
                {
                    UpdateSelectionItemsAction();
                }
            }
            else
            {
                if (UpdateSearchItemsAction != null)
                {
                    UpdateSearchItemsAction(m_SearchBar.SearchText);
                }
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();

            if (m_FolderTreeViewGroup != null)
                m_FolderTreeViewGroup.OnFocus();

            CheckBottomBarItemValidity();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            if (m_TreeView != null)
            {
                m_TreeView.EndPing();
                m_TreeView.EndNameEditing(true);
            }

            if (m_GridView != null)
            {
                m_GridView.EndPing();
                m_GridView.EndRename(true);
            }
        }

        private void HandleCommandEvents()
        {
            EventType type = Event.current.type;
            switch (type)
            {
                case EventType.ExecuteCommand:
                case EventType.ValidateCommand:
                    bool flag = type == EventType.ExecuteCommand;
                    if (Event.current.commandName == "Delete" || Event.current.commandName == "SoftDelete")
                    {
                        Event.current.Use();
                        if (flag)
                        {
                            //Event.current.commandName == "Delete"       Shift + Delete
                            //Event.current.commandName == "SoftDelete"   Delete
                            bool askIfSure = Event.current.commandName == "SoftDelete";
                            if (GridViewDeleteAction != null)
                                GridViewDeleteAction(askIfSure);

                            if (askIfSure)
                                Owner.WindowOwner.Focus();
                        }
                        GUIUtility.ExitGUI();
                    }
                    else if (Event.current.commandName == "SelectAll")
                    {
                        if (flag)
                            m_GridView.SelectAll();
                        Event.current.Use();
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

        private void ListAreaKeyboardCallback()
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            KeyCode keyCode = Event.current.keyCode;

            switch (keyCode)
            {
                case KeyCode.KeypadEnter:
                    if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        if (m_GridView.BeginRename(0.0f))
                        {
                            Event.current.Use();
                        }
                        break;
                    }
                    Event.current.Use();
                    break;
                default:
                    if (keyCode != KeyCode.Backspace)
                    {
                        if (keyCode != KeyCode.Return)
                        {
                            if (keyCode == KeyCode.F2 && Application.platform != RuntimePlatform.OSXEditor && m_GridView.BeginRename(0.0f))
                            {
                                Event.current.Use();
                            }
                            break;
                        }
                        goto case KeyCode.KeypadEnter;
                    }
                    break;
            }
        }

        private void GridViewItemSelected(int[] idList)
        {
            if (!IsCreatingItem)
                RefreshBottomBarSelectedItem(idList);
        }

        private void RefreshBottomBarSelectedItem(int[] idList)
        {
            if (idList != null && idList.Length > 0)
            {
                var index = m_GridViewDataSource.GetItemIndexByItemId(idList[0]);
                if (index == -1)
                {
                    var item = m_ConfigSource.GetValue<FolderGridItem>(BottomSelectedItem);
                    var parentItem = m_TreeView.FindItem(item.ParentId) as FolderTreeViewItem;
                    if (parentItem == null)
                        return;

                    if (parentItem.hasChildren)
                    {
                        foreach (var child in parentItem.children)
                        {
                            if (child.id == item.Id)
                            {
                                m_BottomBarSelectedItem = item;
                                m_BottomBar.SelectedPathSplitted.Clear();
                                return;
                            }
                        }
                    }

                    if (parentItem.FileList != null)
                    {
                        foreach (var child in parentItem.FileList)
                        {
                            if (child.id == item.Id)
                            {
                                m_BottomBarSelectedItem = item;
                                m_BottomBar.SelectedPathSplitted.Clear();
                                return;
                            }
                        }
                    }
                    return;
                }

                m_BottomBarSelectedItem = m_GridViewDataSource.GetItemByIndex(index) as FolderGridItem;
                m_ConfigSource.SetValue(BottomSelectedItem, m_BottomBarSelectedItem);
                m_BottomBar.SelectedPathSplitted.Clear();
            }
            else
            {
                m_BottomBarSelectedItem = null;
                m_ConfigSource.SetValue<FolderGridItem>(BottomSelectedItem, null);
                m_BottomBar.SelectedPathSplitted.Clear();
            }
            m_ConfigSource.SetConfigDirty();
        }

        private float GetBottomBarHeight()
        {
            if (m_BottomBar.SelectedPathSplitted.Count == 0)
                RefreshSplittedSelectedPath();
            return 17f * m_BottomBar.SelectedPathSplitted.Count;
        }

        private void RefreshSplittedSelectedPath()
        {
            m_BottomBar.SelectedPathSplitted.Clear();
            if (m_BottomBarSelectedItem == null)
            {
                m_BottomBar.SelectedPathSplitted.Add(new GUIContent());
            }
            else
            {
                if (!m_TreeView.isSearching)
                {
                    m_BottomBar.SelectedPathSplitted.Add(new GUIContent(Path.GetFileName(m_BottomBarSelectedItem.Path), m_BottomBarSelectedItem.Texture));
                }
                else
                {
                    float num = m_BottomBarRectWidth - 55 - 16;
                    var parentItemList = GetSplittedParentItems();
                    var itemPathString = GetSplittedString(parentItemList);
                    if (selectedPathLabel.CalcSize(GUIContentWrap.Temp(itemPathString)).x + 25.0 > num)
                    {
                        for (int i = parentItemList.Count - 1; i >= 0; --i)
                        {
                            m_BottomBar.SelectedPathSplitted.Add(new GUIContent(parentItemList[i].displayName, EditorGUIUtility.FindTexture(EditorResourcesUtilityWrap.folderIconName)));
                        }
                        m_BottomBar.SelectedPathSplitted.Add(new GUIContent(m_BottomBarSelectedItem.DisplayName, m_BottomBarSelectedItem.Texture));
                    }
                    else
                    {
                        m_BottomBar.SelectedPathSplitted.Add(new GUIContent(itemPathString, m_BottomBarSelectedItem.Texture));
                    }
                }
            }
        }

        private List<FolderTreeViewItem> GetSplittedParentItems()
        {
            var parentId = m_BottomBarSelectedItem.ParentId;
            var parentItemList = new List<FolderTreeViewItem>();
            var item = m_TreeView.FindItem(parentId) as FolderTreeViewItem;
            if (item == m_TreeView.data.root)
                return parentItemList;
            parentItemList.Add(item);

            while (item.parent != null && item.parent != m_TreeView.data.root)
            {
                parentItemList.Add(item);
                item = item.parent as FolderTreeViewItem;
            }

            return parentItemList;
        }

        private string GetSplittedString(List<FolderTreeViewItem> parentItemList)
        {
            var sb = new StringBuilder();
            for (int i = parentItemList.Count - 1; i >= 0; --i)
            {
                sb.Append(parentItemList[i].displayName);
                sb.Append("/");
            }

            sb.Append(m_BottomBarSelectedItem.DisplayName);
            return sb.ToString();
        }

        private void ItemDoubleClick(GridItem item)
        {
            var folderGridItem = item as FolderGridItem;
            if (folderGridItem.IsFolder)
            {
                m_SearchBar.ClearSearchText();
                m_TreeView.SetSelection(new int[] { item.Id }, true);
                UpdateGridViewContent();
                UpdateBreadcrumbBarContents();
            }
            else
            {
                WindowsOSUtility.OpenFileWithApp(folderGridItem.Path);
            }
        }

        #region Item重命名处理

        private bool ItemBeginRenameAction(GridItem item)
        {
            m_GridView.GetRenameOverlay().isRenamingFilename = true;
            return true;
        }

        private void ItemRenameEndAction(GridItem item, string oldName, string newName)
        {
            var isCreating = IsCreatingItem;
            if (IsCreatingItem)
            {
                IsCreatingItem = false;
                m_SearchBar.ClearSearchText();
                if (RenameEndWhenCreatingAction != null)
                {
                    RenameEndWhenCreatingAction();
                    RenameEndWhenCreatingAction = null;
                }
            }

            if (oldName != newName)
            {
                var folderOrFileGridItem = item as FolderGridItem;
                if (folderOrFileGridItem.IsFolder)
                {
                    m_FolderTreeViewGroup.RenameItem(m_TreeView.FindItem(item.Id), newName);
                }
                else
                {
                    var parentItem = m_TreeView.FindItem(folderOrFileGridItem.ParentId) as FolderTreeViewItem;
                    if (parentItem.FileList != null)
                    {
                        foreach (var child in parentItem.FileList)
                        {
                            if (child.id == item.Id)
                            {
                                m_FolderTreeViewGroup.RenameItem(child, newName);
                                break;
                            }
                        }
                    }
                }

                UpdateGridViewContent();
                RefreshBottomBarSelectedItem(new int[] { item.Id });
                m_GridView.Frame(item.Id, true);
            }

            if (isCreating)
            {
                m_TreeView.data.RefreshData();
                RefreshBottomBarSelectedItem(new int[] { item.Id });
                UpdateBreadcrumbBarContents();
            }
        }

        #endregion

        #region SearchBar处理

        public void UpdateItemsBySearchText(bool isCreatingItem = false)
        {
            m_TreeView.searchString = m_SearchBar.SearchText;
            m_BreadcrumbBar.IsSearching = !string.IsNullOrEmpty(m_SearchBar.SearchText);
            UpdateGridViewContent();
            UpdateBreadcrumbBarContents();
        }

        private void SearchBarUpOrDownArrowPressedAction()
        {
            if (!m_GridView.IsLastClickedItemVisible())
                m_GridView.SelectFirst();
            GUIUtility.keyboardControl = m_GridView.KeyboardControlID;
        }

        #endregion

        #region Item增删改查处理

        private void ItemExpandedAction(int itemId, bool expanded)
        {
            var index = m_GridViewDataSource.GetItemIndexByItemId(itemId);
            var item = m_GridViewDataSource.GetItemByIndex(index);
            if (expanded)
            {
                var children = item.GetChildItemRange(0, item.ChildItemCount);
                if (children != null)
                {
                    m_GridViewDataSource.ItemList.InsertRange(index + 1, item.GetChildItemRange(0, item.ChildItemCount));
                }
            }
            else
            {
                int endIndex = index + 1;
                bool hasChild = false;
                for (; endIndex < m_GridViewDataSource.Count;)
                {
                    var nextItem = m_GridViewDataSource.GetItemByIndex(endIndex);
                    if (nextItem.IsChildItem)
                    {
                        hasChild = true;
                        ++endIndex;
                    }
                    else
                        break;
                }

                if (hasChild)
                {
                    m_GridViewDataSource.ItemList.RemoveRange(index + 1, endIndex - index - 1);
                }
            }
        }

        private void DeleteGridItems(bool askIfSure)
        {
            var selection = m_GridView.GetSelection();

            if (askIfSure)
            {
                if (!EditorUtility.DisplayDialog("删除操作", "确定删除所选的Item吗?", "Delete", "Cancel"))
                    return;
            }

            List<FolderTreeViewItem> itemList = new List<FolderTreeViewItem>();
            var dataSource = m_TreeView.data;
            foreach (var itemId in selection)
            {
                var index = m_GridViewDataSource.GetItemIndexByItemId(itemId);
                if (index == -1)
                    continue;

                var item = m_GridViewDataSource.GetItemByIndex(index) as FolderGridItem;
                if (item.IsFolder)
                {
                    itemList.Add(dataSource.FindItem(item.Id) as FolderTreeViewItem);
                }
                else
                {
                    var parentItem = dataSource.FindItem(item.ParentId) as FolderTreeViewItem;
                    if (parentItem.FileList != null)
                    {
                        foreach (var child in parentItem.FileList)
                        {
                            if (child.id == item.Id)
                            {
                                itemList.Add(child);
                                break;
                            }
                        }
                    }
                }
            }

            if (itemList.Count > 0)
            {
                m_FolderTreeViewGroup.DeleteItemList(itemList);
                UpdateGridViewContent();
            }

            CheckBottomBarItemValidity();
            UpdateBreadcrumbBarContents();
        }

        #endregion

        private void CheckBottomBarItemValidity()
        {
            if (m_BottomBarSelectedItem != null)
            {
                if (m_BottomBarSelectedItem.IsFolder)
                {
                    if (!Directory.Exists(m_BottomBarSelectedItem.Path))
                    {
                        m_BottomBarSelectedItem = null;
                        m_BottomBar.SelectedPathSplitted.Clear();
                    }
                }
                else
                {
                    if (!File.Exists(m_BottomBarSelectedItem.Path))
                    {
                        m_BottomBarSelectedItem = null;
                        m_BottomBar.SelectedPathSplitted.Clear();
                    }
                }
            }
        }

        private void FolderTreeViewSelectionChanged(int[] ids)
        {
            if (m_TreeView.isSearching)
            {
                m_TreeView.searchString = "";
                m_BreadcrumbBar.IsSearching = false;
                m_SearchBar.ClearSearchText();
            }

            UpdateBreadcrumbBarContents();
            UpdateGridViewContent();
        }

        private void UpdateBreadcrumbBarContents()
        {
            if (m_TreeView == null || m_TreeView.data == null)
                return;

            var selectedIDs = m_TreeView.state.selectedIDs;
            for (int i = selectedIDs.Count - 1; i >= 0; --i)
            {
                if (m_TreeView.FindItem(selectedIDs[i]) == null)
                    selectedIDs.RemoveAt(i);
            }

            m_BreadcrumbBar.ShowMultipleFolders = selectedIDs != null && selectedIDs.Count > 1;
            if (selectedIDs != null && selectedIDs.Count == 0)
            {
                selectedIDs.Add(m_TreeView.data.root.id);
                UpdateGridViewContent();
                m_ConfigSource.SetConfigDirty();
            }

            if (m_BreadcrumbBar.IsSearching)
            {
                m_BreadcrumbBar.InitSearchMenu(new GUIContent(m_TreeView.data.root.displayName), m_TreeView.data.root.id);
            }
            else
            {
                if (selectedIDs != null && selectedIDs.Count == 1)
                {
                    m_BreadcrumbBar.BreadCrumbs.Clear();
                    var curSelectedView = m_TreeView.FindItem(selectedIDs[0]);
                    m_BreadcrumbBar.LastFolderHasSubFolders = curSelectedView.hasChildren && curSelectedView.children.Count > 0;

                    while (curSelectedView != null)
                    {
                        m_BreadcrumbBar.BreadCrumbs.Add(new KeyValuePair<GUIContent, int>(new GUIContent(curSelectedView.displayName), curSelectedView.id));
                        curSelectedView = curSelectedView.parent;
                    }
                    m_BreadcrumbBar.BreadCrumbs.Reverse();
                }
            }
        }

        private void DuplicateItemGridView()
        {
            var selectedIdList = m_GridView.GetSelection();
            if (selectedIdList.Length == 0)
                return;

            List<int> idList = new List<int>();
            try
            {
                m_TreeView.EndNameEditing(true);
                m_GridView.EndRename(false);
                foreach (var id in selectedIdList)
                {
                    var item = m_GridViewDataSource.GetItemByIndex(m_GridViewDataSource.GetItemIndexByItemId(id)) as FolderGridItem;
                    FolderTreeViewItem treeItem = null;
                    FolderTreeViewItem parentTreeItem = null;
                    if (item.IsFolder)
                    {
                        treeItem = m_TreeView.FindItem(item.Id) as FolderTreeViewItem;
                        parentTreeItem = treeItem.parent as FolderTreeViewItem;
                    }
                    else
                    {
                        parentTreeItem = m_TreeView.FindItem(item.ParentId) as FolderTreeViewItem;
                        foreach (var child in parentTreeItem.FileList)
                        {
                            if (child.id == item.Id)
                            {
                                treeItem = child;
                                break;
                            }
                        }
                    }

                    if (item.IsFolder)
                    {
                        var newPath = EditorFileUtility.GetNewFolder(item.Path);
                        FileUtil.CopyFileOrDirectory(item.Path, newPath);
                        var newItem = JsonReader.Deserialize(JsonWriter.Serialize(treeItem, new JsonWriterSettings() { MaxDepth = Int32.MaxValue }), true) as FolderTreeViewItem;

                        newItem.id = m_FolderTreeViewGroup.GetDataContainer().GetAutoID();
                        newItem.FileList = null;
                        newItem.children = null;
                        idList.Add(newItem.id);

                        newItem.Path = newPath;
                        newItem.displayName = new DirectoryInfo(newPath).Name;

                        var newGridItem = JsonReader.Deserialize(JsonWriter.Serialize(item, new JsonWriterSettings() { MaxDepth = Int32.MaxValue }), item.GetType(), true) as FolderGridItem;
                        newGridItem.Id = newItem.id;
                        newGridItem.Path = newItem.Path;
                        newGridItem.DisplayName = newItem.displayName;
                        m_GridViewDataSource.ItemList.Add(newGridItem);

                        parentTreeItem.AddChild(newItem);
                        var comparator = new AlphanumComparator.AlphanumComparator();
                        parentTreeItem.children.Sort((viewItem, treeViewItem) =>
                        {
                            return comparator.Compare(viewItem.displayName, treeViewItem.displayName);
                        });
                    }
                    else
                    {
                        var newPath = EditorFileUtility.GetNewFile(item.Path);
                        FileUtil.CopyFileOrDirectory(item.Path, newPath);
                        var newItem = JsonReader.Deserialize(JsonWriter.Serialize(treeItem, new JsonWriterSettings() { MaxDepth = Int32.MaxValue }), true) as FolderTreeViewItem;
                        newItem.id = m_FolderTreeViewGroup.GetDataContainer().GetAutoID();
                        idList.Add(newItem.id);
                        newItem.Path = newPath;
                        newItem.displayName = Path.GetFileNameWithoutExtension(newPath);
                        parentTreeItem.FileList.Add(newItem);

                        var newGridItem = JsonReader.Deserialize(JsonWriter.Serialize(item, new JsonWriterSettings() { MaxDepth = Int32.MaxValue }), item.GetType(), true) as FolderGridItem;
                        newGridItem.Id = newItem.id;
                        newGridItem.Path = newItem.Path;
                        newGridItem.DisplayName = newItem.displayName;
                        m_GridViewDataSource.ItemList.Add(newGridItem);
                    }
                }

                m_GridView.SetSelection(idList.ToArray(), false);
            }
            catch (Exception e)
            {
                Debug.LogError("复制item出错:" + e);
            }
            finally
            {
                m_FolderTreeViewGroup.GetDataContainer().UpdateValidItems();
                m_TreeView.data.RefreshData();
                UpdateGridViewContent();
                if (m_ConfigSource != null)
                    m_ConfigSource.SetConfigDirty();
            }
        }
    }
}