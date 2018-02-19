using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class GridViewGroup : ViewGroup
    {
        protected GridView m_GridView;
        protected GridViewDataSource m_GridViewDataSource;

        protected SearchBar m_SearchBar;
        protected BottomBar m_BottomBar;

        private bool m_IsCreatingItem;
        private ItemDataSource m_ItemDataSource;

        public Action DuplicateItemsAction { get; set; }
        public Action<bool> GridViewDeleteAction { get; set; }

        public bool IsCreatingItem
        {
            get { return m_IsCreatingItem; }
            set { m_IsCreatingItem = value; }
        }

        public GridViewGroup(ViewGroupManager owner, ItemDataSource itemDataSource, string dragId = null) : base(owner)
        {
            m_ItemDataSource = itemDataSource;
            m_SearchBar = new SearchBar(owner);
            m_SearchBar.UpOrDownArrowPressedAction += SearchBarUpOrDownArrowPressedAction;
            m_SearchBar.OnTextChangedAction += (str) => UpdateItemsBySearchText();

            m_GridViewDataSource = new GridViewDataSource();
            m_GridView = new GridView(owner, new GenericGridLayout(m_GridViewDataSource));
            m_GridView.KeyboardCallback += ListAreaKeyboardCallback;
            m_GridView.ItemExpandedAction += ItemExpandedAction;
            m_GridView.BeginRenameAction += ItemBeginRenameAction;
            m_GridView.RenameEndAction += ItemRenameEndAction;
            m_GridView.ViewHandler.GenericDragId = dragId != null ? dragId : GetHashCode().ToString();
            m_GridView.GridSizeChangedAction += (size) =>
            {
                m_BottomBar.Value = size;
            };

            m_BottomBar = new BottomBar(owner);
            m_BottomBar.Value = m_GridView.GridSize;
            m_BottomBar.MinValue = m_GridView.ViewLayout.LayoutParams.MinGridSize;
            m_BottomBar.MaxValue = m_GridView.ViewLayout.LayoutParams.MaxGridSize;
            m_BottomBar.OnValueChangedAction += (size) =>
            {
                m_GridView.GridSize = size;
            };


            GridViewDeleteAction += DeleteGridItems;
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

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            var position = Owner.WindowOwner.position;
            var searchBarRect = new Rect(0f, 0f, position.width, EditorStyles.toolbar.fixedHeight);
            var gridViewRect = new Rect(0, EditorStyles.toolbar.fixedHeight, position.width, position.height - EditorStyles.toolbar.fixedHeight - 17f);
            var bottomBarRect = new Rect(0, position.height - EditorStyles.toolbar.fixedHeight, position.width, 17f);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                m_GridView.EndPing();
            }

            m_SearchBar.OnGUI(searchBarRect);
            m_GridView.OnGUI(gridViewRect);
            m_BottomBar.OnGUI(bottomBarRect);

            HandleCommandEvents();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
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

        #region Item重命名处理

        private bool ItemBeginRenameAction(GridItem item)
        {
            m_GridView.GetRenameOverlay().isRenamingFilename = false;
            return true;
        }

        private void ItemRenameEndAction(GridItem item, string oldName, string newName)
        {
            if (m_IsCreatingItem)
            {
                m_IsCreatingItem = false;
                m_SearchBar.ClearSearchText();
            }

            if (oldName != newName)
            {
                UpdateItemsBySearchText();
            }
        }

        #endregion

        #region SearchBar处理

        public void UpdateItemsBySearchText(bool isCreatingItem = false)
        {
            var searchText = m_SearchBar.SearchText;
            var gridViewDataSource = m_GridViewDataSource;
            var gridViewHandler = m_GridView.ViewHandler;
            var gridViewConfig = m_GridView.ViewConfig;

            if (string.IsNullOrEmpty(searchText) || isCreatingItem)
            {
                gridViewHandler.SearchMode = false;
                gridViewDataSource.ItemList.Clear();
                for (int i = 0, count = m_ItemDataSource.Count; i < count; ++i)
                {
                    var item = m_ItemDataSource.GetItem(i);
                    item.IsChildItem = false;
                    gridViewDataSource.ItemList.Add(item);
                }

                gridViewDataSource.ItemList.Sort(((firstItem, secondItem) =>
                {
                    if (firstItem.DisplayName == secondItem.DisplayName)
                    {
                        return firstItem.Id.CompareTo(secondItem.Id);
                    }
                    return firstItem.DisplayName.CompareTo(secondItem.DisplayName);
                }));

                for (int i = gridViewDataSource.Count - 1; i > 0; --i)
                {
                    var item = gridViewDataSource.ItemList[i];
                    if (gridViewConfig.ExpandedItemIdList.Contains(item.Id) && item.HasChildren)
                    {
                        gridViewDataSource.ItemList.InsertRange(i + 1, item.GetAllChildItem());
                    }
                }
            }
            else
            {
                gridViewHandler.SearchMode = true;
                gridViewDataSource.ItemList.Clear();
                for (int i = 0, count = m_ItemDataSource.Count; i < count; ++i)
                {
                    var item = m_ItemDataSource.GetItem(i);
                    if (item.DisplayName.Contains(searchText))
                    {
                        gridViewDataSource.ItemList.Add(item);
                    }

                    for (int j = 0, childCount = item.ChildItemCount; j < childCount; ++j)
                    {
                        var childItem = item.GetChildItem(j);
                        if (childItem.DisplayName.Contains(searchText))
                        {
                            gridViewDataSource.ItemList.Add(childItem);
                        }
                    }
                }

                gridViewDataSource.ItemList.Sort(((firstItem, secondItem) =>
                {
                    if (firstItem.DisplayName == secondItem.DisplayName)
                    {
                        return firstItem.Id.CompareTo(secondItem.Id);
                    }
                    return firstItem.DisplayName.CompareTo(secondItem.DisplayName);
                }));
            }
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

            foreach (var itemId in selection)
            {
                var item = m_ItemDataSource.GetItem(m_ItemDataSource.GetItemIndexByItemId(itemId));
                if (item != null)
                {
                    var index = m_GridViewDataSource.GetItemIndexByItemId(item.Id);
                    if (index != -1)
                    {
                        m_GridViewDataSource.ItemList.RemoveAt(index);
                    }

                    for (int i = 0, count = item.ChildItemCount; i < count; ++i)
                    {
                        index = m_GridViewDataSource.GetItemIndexByItemId(item.GetChildItem(i).Id);
                        if (index != -1)
                        {
                            m_GridViewDataSource.ItemList.RemoveAt(index);
                        }
                    }

                    m_ItemDataSource.RemoveItem(item);
                }
            }
        }

        #endregion
    }
}