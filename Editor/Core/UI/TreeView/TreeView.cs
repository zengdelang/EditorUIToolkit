using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;

namespace EUTK
{
    public class TreeView
    {
        private readonly TreeViewItemExpansionAnimator m_ExpansionAnimator = new TreeViewItemExpansionAnimator();
        private const float kSpaceForScrollBar = 16f;

        private List<int> m_DragSelection = new List<int>();
        private bool m_UseScrollView = true;
        private bool m_AllowRenameOnMouseUp = true;
        private bool m_UseExpansionAnimation = false;

        private EditorWindow m_EditorWindow;
        private AnimFloat m_FramingAnimFloat;
        private bool m_StopIteratingItems;
        private bool m_GrabKeyboardFocus;
        private Rect m_TotalRect;
        private bool m_HadFocusLastEvent;
        private int m_KeyboardControlID;

        public Action<int[]> selectionChangedCallback { get; set; }
        public Action<int> itemDoubleClickedCallback { get; set; }
        public Action<int[], bool> dragEndedCallback { get; set; }
        public Action<int> contextClickItemCallback { get; set; }
        public Action contextClickOutsideItemsCallback { get; set; }
        public Action keyboardInputCallback { get; set; }
        public Action expandedStateChanged { get; set; }
        public Action<string> searchChanged { get; set; }
        public Action<Vector2> scrollChanged { get; set; }
        public Action<int, Rect> onGUIRowCallback { get; set; }
        public ITreeViewDataSource data { get; set; }
        public ITreeViewDragging dragging { get; set; }
        public ITreeViewGUI gui { get; set; }
        public TreeViewState state { get; set; }

        public TreeViewItemExpansionAnimator expansionAnimator
        {
            get
            {
                return m_ExpansionAnimator;
            }
        }

        public bool deselectOnUnhandledMouseDown { get; set; }

        public bool useExpansionAnimation
        {
            get
            {
                return m_UseExpansionAnimation;
            }
            set
            {
                m_UseExpansionAnimation = value;
            }
        }

        public bool isSearching
        {
            get
            {
                return !string.IsNullOrEmpty(state.searchString);
            }
        }

        public string searchString
        {
            get
            {
                return state.searchString;
            }
            set
            {
                state.searchString = value;
                if (data != null)
                    data.OnSearchChanged();
                if (searchChanged == null)
                    return;
                searchChanged(state.searchString);
            }
        }

        private bool animatingExpansion
        {
            get
            {
                if (m_UseExpansionAnimation)
                    return m_ExpansionAnimator.isAnimating;
                return false;
            }
        }

        public TreeView(EditorWindow editorWindow, TreeViewState treeViewState)
        {
            m_EditorWindow = editorWindow;
            state = treeViewState;
        }

        public void Init(Rect rect, ITreeViewDataSource data, ITreeViewGUI gui, ITreeViewDragging dragging)
        {
            this.data = data;
            this.gui = gui;
            this.dragging = dragging;
            m_TotalRect = rect;
            data.OnInitialize();
            gui.OnInitialize();
            if (dragging != null)
                dragging.OnInitialize();
            expandedStateChanged += ExpandedStateHasChanged;
            m_FramingAnimFloat = new AnimFloat(state.scrollPos.y, AnimatedScrollChanged);
        }

        private void ExpandedStateHasChanged()
        {
            m_StopIteratingItems = true;
        }

        public bool IsSelected(int id)
        {
            return state.selectedIDs.Contains(id);
        }

        public bool HasSelection()
        {
            return Enumerable.Count(state.selectedIDs) > 0;
        }

        public int[] GetSelection()
        {
            return state.selectedIDs.ToArray();
        }

        public int[] GetRowIDs()
        {
            return Enumerable.ToArray(Enumerable.Select(data.GetRows(), item => item.id));
        }

        public void SetSelection(int[] selectedIDs, bool revealSelectionAndFrameLastSelected)
        {
            SetSelection(selectedIDs, revealSelectionAndFrameLastSelected, false);
        }

        public void SetSelection(int[] selectedIDs, bool revealSelectionAndFrameLastSelected, bool animatedFraming)
        {
            if (selectedIDs.Length > 0)
            {
                if (revealSelectionAndFrameLastSelected)
                {
                    foreach (int id in selectedIDs)
                        data.RevealItem(id);
                }

                state.selectedIDs = new List<int>(selectedIDs);
                bool flag = state.selectedIDs.IndexOf(state.lastClickedID) >= 0;
                if (!flag)
                {
                    int id = Enumerable.Last(selectedIDs);
                    if (data.GetRow(id) != -1)
                    {
                        state.lastClickedID = id;
                        flag = true;
                    }
                    else
                        state.lastClickedID = 0;
                }

                if (!revealSelectionAndFrameLastSelected || !flag)
                    return;
                Frame(state.lastClickedID, true, false, animatedFraming);
            }
            else
            {
                state.selectedIDs.Clear();
                state.lastClickedID = 0;
            }
        }

        public TreeViewItem FindItem(int id)
        {
            return data.FindItem(id);
        }

        public void SetUseScrollView(bool useScrollView)
        {
            m_UseScrollView = useScrollView;
        }

        public void Repaint()
        {
            if (m_EditorWindow == null)
                return;
            m_EditorWindow.Repaint();
        }

        public void ReloadData()
        {
            data.ReloadData();
            Repaint();
            m_StopIteratingItems = true;
        }

        public bool HasFocus()
        {
            if (m_EditorWindow != null && EditorWindowWrap.HasFocus(m_EditorWindow))
                return GUIUtility.keyboardControl == m_KeyboardControlID;
            return false;
        }

        internal static int GetItemControlID(TreeViewItem item, TreeView treeView)
        {
            return (item == null ? 0 : item.GetHashCode()) + treeView.GetHashCode();
        }

        public void HandleUnusedMouseEventsForItem(Rect rect, TreeViewItem item, bool firstItem)
        {
            int itemControlId = GetItemControlID(item, this);
            Event current = Event.current;
            EventType typeForControl = current.GetTypeForControl(itemControlId);
            switch (typeForControl)
            {
                case EventType.MouseDown:
                    if (!rect.Contains(Event.current.mousePosition))
                        break;
                    if (Event.current.button == 0)
                    {
                        GUIUtility.keyboardControl = m_KeyboardControlID;
                        Repaint();
                        if (Event.current.clickCount == 2)
                        {
                            if (itemDoubleClickedCallback != null)
                                itemDoubleClickedCallback(item.id);
                        }
                        else
                        {
                            if (dragging == null || dragging.CanStartDrag(item, m_DragSelection, Event.current.mousePosition))
                            {
                                m_DragSelection = GetNewSelection(item, true, false);
                                ((DragAndDropDelay)GUIUtility.GetStateObject(typeof(DragAndDropDelay), itemControlId)).mouseDownPosition = Event.current.mousePosition;
                            }
                            GUIUtility.hotControl = itemControlId;
                        }
                        current.Use();
                        break;
                    }
                    if (Event.current.button != 1)
                        break;
                    bool keepMultiSelection = true;
                    SelectionClick(item, keepMultiSelection);
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl != itemControlId)
                        break;
                    GUIUtility.hotControl = 0;
                    m_DragSelection.Clear();
                    current.Use();
                    if (!rect.Contains(current.mousePosition))
                        break;
                    float contentIndent = gui.GetContentIndent(item);
                    Rect rect1 = new Rect(rect.x + contentIndent, rect.y, rect.width - contentIndent, rect.height);
                    List<int> selectedIds = state.selectedIDs;
                    if (m_AllowRenameOnMouseUp && selectedIds != null && (selectedIds.Count == 1 && selectedIds[0] == item.id) && (rect1.Contains(current.mousePosition) && !EditorGUIUtilityWrap.HasHolddownKeyModifiers(current)))
                    {
                        BeginNameEditing(0.5f);
                        break;
                    }
                    SelectionClick(item, false);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != itemControlId || dragging == null)
                        break;
                    DragAndDropDelay dragAndDropDelay = (DragAndDropDelay)GUIUtility.GetStateObject(typeof(DragAndDropDelay), itemControlId);
                    if (dragAndDropDelay.CanStartDrag() && dragging.CanStartDrag(item, m_DragSelection, dragAndDropDelay.mouseDownPosition))
                    {
                        dragging.StartDrag(item, m_DragSelection);
                        GUIUtility.hotControl = 0;
                    }
                    current.Use();
                    break;
                default:
                    if (typeForControl != EventType.DragUpdated && typeForControl != EventType.DragPerform)
                    {
                        if (typeForControl != EventType.ContextClick || !rect.Contains(current.mousePosition) || contextClickItemCallback == null)
                            break;
                        contextClickItemCallback(item.id);
                        break;
                    }
                    if (dragging == null || !dragging.DragElement(item, rect, firstItem))
                        break;
                    GUIUtility.hotControl = 0;
                    break;
            }
        }

        public void GrabKeyboardFocus()
        {
            m_GrabKeyboardFocus = true;
        }

        public void NotifyListenersThatSelectionChanged()
        {
            if (selectionChangedCallback == null)
                return;
            selectionChangedCallback(state.selectedIDs.ToArray());
        }

        public void NotifyListenersThatDragEnded(int[] draggedIDs, bool draggedItemsFromOwnTreeView)
        {
            if (dragEndedCallback == null)
                return;
            dragEndedCallback(draggedIDs, draggedItemsFromOwnTreeView);
        }

        public Vector2 GetContentSize()
        {
            return gui.GetTotalSize();
        }

        public Rect GetTotalRect()
        {
            return m_TotalRect;
        }

        public bool IsItemDragSelectedOrSelected(TreeViewItem item)
        {
            if (m_DragSelection.Count > 0)
                return m_DragSelection.Contains(item.id);
            return state.selectedIDs.Contains(item.id);
        }

        private void DoItemGUI(TreeViewItem item, int row, float rowWidth, bool hasFocus)
        {
            if (row < 0 || row >= data.rowCount)
            {
                Debug.LogError(string.Concat("Invalid. Org row: ", row, " Num rows ", data.rowCount));
            }
            else
            {
                bool selected = IsItemDragSelectedOrSelected(item);
                Rect rect1 = gui.GetRowRect(row, rowWidth);
                if (animatingExpansion)
                    rect1 = m_ExpansionAnimator.OnBeginRowGUI(row, rect1);
                if (animatingExpansion)
                    m_ExpansionAnimator.OnRowGUI(row);
                gui.OnRowGUI(rect1, item, row, selected, hasFocus);
                if (animatingExpansion)
                    m_ExpansionAnimator.OnEndRowGUI(row);
                if (onGUIRowCallback != null)
                {
                    float contentIndent = gui.GetContentIndent(item);
                    Rect rect2 = new Rect(rect1.x + contentIndent, rect1.y, rect1.width - contentIndent, rect1.height);
                    onGUIRowCallback(item.id, rect2);
                }
                HandleUnusedMouseEventsForItem(rect1, item, row == 0);
            }
        }

        public void OnGUI(Rect rect, int keyboardControlID)
        {
            m_TotalRect = rect;

            m_KeyboardControlID = keyboardControlID;
            Event current = Event.current;

            if (m_GrabKeyboardFocus || current.type == EventType.MouseDown && m_TotalRect.Contains(current.mousePosition))
            {
                m_GrabKeyboardFocus = false;
                GUIUtility.keyboardControl = m_KeyboardControlID;
                m_AllowRenameOnMouseUp = true;
                Repaint();
            }
            bool hasFocus = HasFocus();
            if (hasFocus != m_HadFocusLastEvent && current.type != EventType.Layout)
            {
                m_HadFocusLastEvent = hasFocus;
                if (hasFocus && current.type == EventType.MouseDown)
                    m_AllowRenameOnMouseUp = false;
            }
            if (animatingExpansion)
                m_ExpansionAnimator.OnBeforeAllRowsGUI();
            data.InitIfNeeded();
            Vector2 totalSize = gui.GetTotalSize();
            Rect viewRect = new Rect(0.0f, 0.0f, totalSize.x, totalSize.y);
            if (m_UseScrollView)
                state.scrollPos = GUI.BeginScrollView(m_TotalRect, state.scrollPos, viewRect);
            else
                GUI.BeginClip(m_TotalRect);
            gui.BeginRowGUI();
            int firstRowVisible;
            int lastRowVisible;
            gui.GetFirstAndLastRowVisible(out firstRowVisible, out lastRowVisible);
            if (lastRowVisible >= 0)
            {
                int numVisibleRows = lastRowVisible - firstRowVisible + 1;
                float rowWidth = Mathf.Max(GUIClipWrap.visibleRect.width, viewRect.width);
                IterateVisibleItems(firstRowVisible, numVisibleRows, rowWidth, hasFocus);
            }

            if (animatingExpansion)
                m_ExpansionAnimator.OnAfterAllRowsGUI();
            gui.EndRowGUI();
            if (m_UseScrollView)
                GUI.EndScrollView();
            else
                GUI.EndClip();
            HandleUnusedEvents();
            KeyboardGUI();
        }

        private void IterateVisibleItems(int firstRow, int numVisibleRows, float rowWidth, bool hasFocus)
        {
            m_StopIteratingItems = false;
            int num = 0;
            for (int index = 0; index < numVisibleRows; ++index)
            {
                int row = firstRow + index;
                if (animatingExpansion)
                {
                    int endRow = m_ExpansionAnimator.endRow;
                    if (m_ExpansionAnimator.CullRow(row, gui))
                    {
                        ++num;
                        row = endRow + num;
                    }
                    else
                        row += num;
                    if (row >= data.rowCount)
                        continue;
                }
                else if (gui.GetRowRect(row, rowWidth).y - state.scrollPos.y > m_TotalRect.height)
                    continue;

                DoItemGUI(data.GetItem(row), row, rowWidth, hasFocus);
                if (m_StopIteratingItems)
                    break;
            }
        }

        private List<int> GetVisibleSelectedIds()
        {
            int firstRowVisible;
            int lastRowVisible;
            gui.GetFirstAndLastRowVisible(out firstRowVisible, out lastRowVisible);
            if (lastRowVisible < 0)
                return new List<int>();
            List<int> list = new List<int>(lastRowVisible - firstRowVisible);
            for (int row = firstRowVisible; row < lastRowVisible; ++row)
            {
                TreeViewItem treeViewItem = data.GetItem(row);
                list.Add(treeViewItem.id);
            }
            return Enumerable.ToList(Enumerable.Where(list, id => state.selectedIDs.Contains(id)));
        }

        private void ExpansionAnimationEnded(TreeViewAnimationInput setup)
        {
            if (setup.expanding)
                return;
            ChangeExpandedState(setup.item, false);
        }

        private float GetAnimationDuration(float height)
        {
            if (height > 60.0)
                return 0.1f;
            return height * 0.1f / 60.0f;
        }

        public void UserInputChangedExpandedState(TreeViewItem item, int row, bool expand)
        {
            if (useExpansionAnimation)
            {
                if (expand)
                    ChangeExpandedState(item, true);
                int num = row + 1;
                int lastChildRowUnder = GetLastChildRowUnder(row);
                float width = GUIClipWrap.visibleRect.width;
                Rect rectForRows = GetRectForRows(num, lastChildRowUnder, width);
                float animationDuration = GetAnimationDuration(rectForRows.height);
                expansionAnimator.BeginAnimating(new TreeViewAnimationInput()
                {
                    animationDuration = animationDuration,
                    startRow = num,
                    endRow = lastChildRowUnder,
                    startRowRect = gui.GetRowRect(num, width),
                    rowsRect = rectForRows,
                    startTime = EditorApplication.timeSinceStartup,
                    expanding = expand,
                    animationEnded = ExpansionAnimationEnded,
                    item = item,
                    treeView = this
                });
            }
            else
                ChangeExpandedState(item, expand);
        }

        private void ChangeExpandedState(TreeViewItem item, bool expand)
        {
            if (Event.current.alt)
                data.SetExpandedWithChildren(item, expand);
            else
                data.SetExpanded(item, expand);

            if (!expand)
                return;
            UserExpandedItem(item);
        }

        private int GetLastChildRowUnder(int row)
        {
            List<TreeViewItem> rows = data.GetRows();
            int depth = rows[row].depth;
            for (int index = row + 1; index < rows.Count; ++index)
            {
                if (rows[index].depth <= depth)
                    return index - 1;
            }
            return rows.Count - 1;
        }

        protected virtual Rect GetRectForRows(int startRow, int endRow, float rowWidth)
        {
            Rect rowRect1 = gui.GetRowRect(startRow, rowWidth);
            Rect rowRect2 = gui.GetRowRect(endRow, rowWidth);
            return new Rect(rowRect1.x, rowRect1.y, rowWidth, rowRect2.yMax - rowRect1.yMin);
        }

        private void HandleUnusedEvents()
        {
            EventType type = Event.current.type;
            switch (type)
            {
                case EventType.DragUpdated:
                    if (dragging == null || !m_TotalRect.Contains(Event.current.mousePosition))
                        break;
                    dragging.DragElement(null, new Rect(), false);
                    Repaint();
                    Event.current.Use();
                    break;
                case EventType.DragPerform:
                    if (dragging == null || !m_TotalRect.Contains(Event.current.mousePosition))
                        break;
                    m_DragSelection.Clear();
                    dragging.DragElement(null, new Rect(), false);
                    Repaint();
                    Event.current.Use();
                    break;
                case EventType.DragExited:
                    if (dragging == null)
                        break;
                    m_DragSelection.Clear();
                    dragging.DragCleanup(true);
                    Repaint();
                    break;
                case EventType.ContextClick:
                    if (!m_TotalRect.Contains(Event.current.mousePosition) || contextClickOutsideItemsCallback == null)
                        break;
                    contextClickOutsideItemsCallback();
                    break;
                default:
                    if (type != EventType.MouseDown || !deselectOnUnhandledMouseDown || (Event.current.button != 0 || !m_TotalRect.Contains(Event.current.mousePosition)) || state.selectedIDs.Count <= 0)
                        break;
                    SetSelection(new int[0], false);
                    NotifyListenersThatSelectionChanged();
                    break;
            }
        }

        public void OnEvent()
        {
            state.renameOverlay.OnEvent();
        }

        public bool BeginNameEditing(float delay)
        {
            if (state.selectedIDs.Count == 0)
                return false;

            List<TreeViewItem> rows = data.GetRows();
            TreeViewItem treeViewItem1 = null;
            var enumerator = state.selectedIDs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                TreeViewItem treeViewItem2 = rows.Find((itemView) => { return itemView == data.FindItem(enumerator.Current); });
                if (treeViewItem1 == null)
                    treeViewItem1 = treeViewItem2;
                else if (treeViewItem2 != null)
                    return false;
            }

            if (treeViewItem1 != null && data.IsRenamingItemAllowed(treeViewItem1))
                return gui.BeginRename(treeViewItem1, delay);
            return false;
        }

        public void EndNameEditing(bool acceptChanges)
        {
            if (!state.renameOverlay.IsRenaming())
                return;
            state.renameOverlay.EndRename(acceptChanges);
            gui.EndRename();
        }

        private TreeViewItem GetItemAndRowIndex(int id, out int row)
        {
            row = data.GetRow(id);
            if (row == -1)
                return null;
            return data.GetItem(row);
        }

        private void HandleFastCollapse(TreeViewItem item, int row)
        {
            if (item.depth == 0)
            {
                for (int row1 = row - 1; row1 >= 0; --row1)
                {
                    if (data.GetItem(row1).hasChildren)
                    {
                        OffsetSelection(row1 - row);
                        break;
                    }
                }
            }
            else
            {
                if (item.depth <= 0)
                    return;
                for (int row1 = row - 1; row1 >= 0; --row1)
                {
                    if (data.GetItem(row1).depth < item.depth)
                    {
                        OffsetSelection(row1 - row);
                        break;
                    }
                }
            }
        }

        private void HandleFastExpand(TreeViewItem item, int row)
        {
            int rowCount = data.rowCount;
            for (int row1 = row + 1; row1 < rowCount; ++row1)
            {
                if (data.GetItem(row1).hasChildren)
                {
                    OffsetSelection(row1 - row);
                    break;
                }
            }
        }

        private void KeyboardGUI()
        {
            if (m_KeyboardControlID != GUIUtility.keyboardControl || !GUI.enabled)
                return;
            if (keyboardInputCallback != null)
                keyboardInputCallback();
            if (Event.current.type != EventType.KeyDown)
                return;
            KeyCode keyCode = Event.current.keyCode;
            switch (keyCode)
            {
                case KeyCode.KeypadEnter:
                    if (Application.platform != RuntimePlatform.OSXEditor || !BeginNameEditing(0.0f))
                        break;
                    Event.current.Use();
                    break;
                case KeyCode.UpArrow:
                    Event.current.Use();
                    OffsetSelection(-1);
                    break;
                case KeyCode.DownArrow:
                    Event.current.Use();
                    OffsetSelection(1);
                    break;
                case KeyCode.RightArrow:
                    using (List<int>.Enumerator enumerator = state.selectedIDs.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            int row;
                            TreeViewItem itemAndRowIndex = GetItemAndRowIndex(enumerator.Current, out row);
                            if (itemAndRowIndex != null)
                            {
                                if (data.IsExpandable(itemAndRowIndex) && !data.IsExpanded(itemAndRowIndex))
                                    UserInputChangedExpandedState(itemAndRowIndex, row, true);
                                else if (state.selectedIDs.Count == 1)
                                    HandleFastExpand(itemAndRowIndex, row);
                            }
                        }
                    }
                    Event.current.Use();
                    break;
                case KeyCode.LeftArrow:
                    using (List<int>.Enumerator enumerator = state.selectedIDs.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            int row;
                            TreeViewItem itemAndRowIndex = GetItemAndRowIndex(enumerator.Current, out row);
                            if (itemAndRowIndex != null)
                            {
                                if (data.IsExpandable(itemAndRowIndex) && data.IsExpanded(itemAndRowIndex))
                                    UserInputChangedExpandedState(itemAndRowIndex, row, false);
                                else if (state.selectedIDs.Count == 1)
                                    HandleFastCollapse(itemAndRowIndex, row);
                            }
                        }
                    }
                    Event.current.Use();
                    break;
                case KeyCode.Home:
                    Event.current.Use();
                    OffsetSelection(-1000000);
                    break;
                case KeyCode.End:
                    Event.current.Use();
                    OffsetSelection(1000000);
                    break;
                case KeyCode.PageUp:
                    Event.current.Use();
                    TreeViewItem fromItem1 = data.FindItem(state.lastClickedID);
                    if (fromItem1 == null)
                        break;
                    OffsetSelection(-gui.GetNumRowsOnPageUpDown(fromItem1, true, m_TotalRect.height));
                    break;
                case KeyCode.PageDown:
                    Event.current.Use();
                    TreeViewItem fromItem2 = data.FindItem(state.lastClickedID);
                    if (fromItem2 == null)
                        break;
                    OffsetSelection(gui.GetNumRowsOnPageUpDown(fromItem2, true, m_TotalRect.height));
                    break;
                case KeyCode.F2:
                    if (Application.platform != RuntimePlatform.WindowsEditor || !BeginNameEditing(0.0f))
                        break;
                    Event.current.Use();
                    break;
                default:
                    if (keyCode != KeyCode.Return)
                    {
                        //if (Event.current.keyCode <= KeyCode.A || Event.current.keyCode >= KeyCode.Z)
                        //    break;
                        break;
                    }
                    goto case KeyCode.KeypadEnter;
            }
        }

        internal static int GetIndexOfID(List<TreeViewItem> items, int id)
        {
            for (int index = 0; index < items.Count; ++index)
            {
                if (items[index].id == id)
                    return index;
            }
            return -1;
        }

        public bool IsLastClickedPartOfRows()
        {
            List<TreeViewItem> rows = data.GetRows();
            if (rows.Count == 0)
                return false;
            return GetIndexOfID(rows, state.lastClickedID) >= 0;
        }

        public void OffsetSelection(int offset)
        {
            List<TreeViewItem> rows = data.GetRows();
            if (rows.Count == 0)
                return;
            Event.current.Use();
            int row = Mathf.Clamp(GetIndexOfID(rows, state.lastClickedID) + offset, 0, rows.Count - 1);
            EnsureRowIsVisible(row, true);
            SelectionByKey(rows[row]);
        }

        private bool GetFirstAndLastSelected(List<TreeViewItem> items, out int firstIndex, out int lastIndex)
        {
            firstIndex = -1;
            lastIndex = -1;
            for (int index = 0; index < items.Count; ++index)
            {
                if (state.selectedIDs.Contains(items[index].id))
                {
                    if (firstIndex == -1)
                        firstIndex = index;
                    lastIndex = index;
                }
            }
            if (firstIndex != -1)
                return lastIndex != -1;
            return false;
        }

        private List<int> GetNewSelection(TreeViewItem clickedItem, bool keepMultiSelection, bool useShiftAsActionKey)
        {
            List<TreeViewItem> rows = data.GetRows();
            List<int> allInstanceIDs = new List<int>(rows.Count);
            for (int index = 0; index < rows.Count; ++index)
                allInstanceIDs.Add(rows[index].id);
            List<int> selectedIds = state.selectedIDs;
            int lastClickedId = state.lastClickedID;
            bool allowMultiSelection = data.CanBeMultiSelected(clickedItem);
            return InternalEditorUtility.GetNewSelection(clickedItem.id, allInstanceIDs, selectedIds, lastClickedId, keepMultiSelection, useShiftAsActionKey, allowMultiSelection);
        }

        private void SelectionByKey(TreeViewItem itemSelected)
        {
            state.selectedIDs = GetNewSelection(itemSelected, false, true);
            state.lastClickedID = itemSelected.id;
            NotifyListenersThatSelectionChanged();
        }

        public void RemoveSelection()
        {
            if (state.selectedIDs.Count <= 0)
                return;
            state.selectedIDs.Clear();
            NotifyListenersThatSelectionChanged();
        }

        public void SelectionClick(TreeViewItem itemClicked, bool keepMultiSelection)
        {
            state.selectedIDs = GetNewSelection(itemClicked, keepMultiSelection, false);
            state.lastClickedID = itemClicked == null ? 0 : itemClicked.id;
            NotifyListenersThatSelectionChanged();
        }

        private float GetTopPixelOfRow(int row)
        {
            return gui.GetRowRect(row, 1f).y;
        }

        private void EnsureRowIsVisible(int row, bool animated)
        {
            if (row < 0)
                return;
            Rect rectForFraming = gui.GetRectForFraming(row);
            float y = rectForFraming.y;
            float targetScrollPos = rectForFraming.yMax - m_TotalRect.height;
            if (state.scrollPos.y < targetScrollPos)
            {
                ChangeScrollValue(targetScrollPos, animated);
            }
            else
            {
                if (state.scrollPos.y <= y)
                    return;
                ChangeScrollValue(y, animated);
            }
        }

        private void AnimatedScrollChanged()
        {
            Repaint();
            state.scrollPosY = m_FramingAnimFloat.value;
        }

        private void ChangeScrollValue(float targetScrollPos, bool animated)
        {
            if (m_UseExpansionAnimation && animated)
            {
                m_FramingAnimFloat.value = state.scrollPos.y;
                m_FramingAnimFloat.target = targetScrollPos;
                m_FramingAnimFloat.speed = 3f;
            }
            else
                state.scrollPosY = targetScrollPos;
        }

        public void Frame(int id, bool frame, bool ping)
        {
            Frame(id, frame, ping, false);
        }

        public void Frame(int id, bool frame, bool ping, bool animated)
        {
            float topPixelOfRow = -1f;
            if (frame)
            {
                data.RevealItem(id);
                int row = data.GetRow(id);
                if (row >= 0)
                {
                    topPixelOfRow = GetTopPixelOfRow(row);
                    EnsureRowIsVisible(row, animated);
                }
            }

            if (!ping)
                return;

            int row1 = data.GetRow(id);
            if (topPixelOfRow == -1.0 && row1 >= 0)
                topPixelOfRow = GetTopPixelOfRow(row1);

            if (topPixelOfRow < 0.0 || row1 < 0 || row1 >= data.rowCount)
                return;

            TreeViewItem treeViewItem = data.GetItem(row1);
            float num = GetContentSize().y <= m_TotalRect.height ? 0.0f : -16f;
            gui.BeginPingItem(treeViewItem, topPixelOfRow, m_TotalRect.width + num);
        }

        public void EndPing()
        {
            gui.EndPingItem();
        }

        public void UserExpandedItem(TreeViewItem item)
        {

        }

        public List<int> SortIDsInVisiblityOrder(List<int> ids)
        {
            if (ids.Count <= 1)
                return ids;

            List<TreeViewItem> rows = data.GetRows();
            List<int> list = new List<int>();
            for (int index1 = 0; index1 < rows.Count; ++index1)
            {
                int id = rows[index1].id;
                for (int index2 = 0; index2 < ids.Count; ++index2)
                {
                    if (ids[index2] == id)
                    {
                        list.Add(id);
                        break;
                    }
                }
            }

            if (ids.Count != list.Count)
            {
                list.AddRange(Enumerable.Except(ids, list));
                if (ids.Count != list.Count)
                {
                    Debug.LogError((string.Concat("SortIDsInVisiblityOrder failed: ", ids.Count.ToString(), " != ", list.Count.ToString())));
                }
            }
            return list;
        }
    }
}