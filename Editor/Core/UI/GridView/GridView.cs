using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class GridView : View
    {
        public delegate bool BeginRenameDelegate(GridItem item);

        public static Styles s_Styles;

        /// <summary>
        /// 保存item的显示名由于显示宽度不足导致被裁剪后的文本
        /// </summary>
        private Dictionary<int, string> m_InstanceIDToCroppedNameMap = new Dictionary<int, string>();

        private const int kPageDown = Int32.MaxValue - 1;
        private const int kPageUp = Int32.MinValue + 1;

        protected int m_KeyboardControlID;
        protected int m_WidthUsedForCroppingName;
        protected bool m_HadKeyboardFocusLastEvent;

        public RenameOverlay m_RenameOverlay = new RenameOverlay();

        public Rect m_TotalRect;
        protected Rect m_VisibleRect;

        protected GridLayout m_GridLayout;
        protected GridViewDataSource m_DataSource;

        protected GridViewHandler m_GridViewHandler;

        //选择偏移，用于快捷选择，比如shift,PageUp等的选择
        protected int m_SelectionOffset;

        protected GridViewConfig m_ViewConfig;

        public GridViewConfig ViewConfig
        {
            get { return m_ViewConfig; }
        }

        public GridLayout ViewLayout
        {
            get { return m_GridLayout; }
        }

        public Rect VisibleRect
        {
            get { return m_VisibleRect; }
        }

        public int KeyboardControlID
        {
            get { return m_KeyboardControlID; }
        }

        public int GridSize
        {
            get { return m_ViewConfig.GridSize; }
            set
            {
                if (m_ViewConfig.GridSize != value)
                {
                    m_ViewConfig.GridSize = value;
                    if (GridSizeChangedAction != null)
                        GridSizeChangedAction(value);
                }
            }
        }

        public GridViewHandler ViewHandler
        {
            get { return m_GridViewHandler; }
            set { m_GridViewHandler = value; }
        }

        public bool AllowRenameOnMouseUp { get; protected set; }

        public Action<int[]> ItemSelectedAction { get; set; }

        public Action<GridItem> ItemDoubleClickAction { get; set; }

        public Action KeyboardCallback { get; set; }

        public BeginRenameDelegate BeginRenameAction { get; set; }
        public Action<GridItem, string, string> RenameEndAction { get; set; }

        public Action<GridItem> GirdItemPopupMenuAction { get; set; }

        public Action GridViewPopupMenuAction { get; set; }

        public Action<int> GridSizeChangedAction { get; set; }

        public Action<int, bool> ItemExpandedAction { get; set; }

        public GridView(ViewGroupManager owner, GridLayout gridLayout, GridViewHandler viewHandler = null) : base(owner)
        {
            m_ViewConfig = new GridViewConfig();
            m_GridLayout = gridLayout;
            m_DataSource = m_GridLayout.DataSource;
            m_GridViewHandler = viewHandler;
            if (m_GridViewHandler == null)
            {
                m_GridViewHandler = new GridViewHandler(m_DataSource);
            }
            m_GridLayout.Owner = this;
            m_RenameOverlay.Clear();
        }

        public void LoadConfig(string configName, EditorWindowConfigSource configSource)
        {
            if (configSource != null && !string.IsNullOrEmpty(configName))
            {
                var viewConfig = configSource.GetValue<GridViewConfig>(configName);
                if (viewConfig != null)
                {
                    viewConfig.SetConfigSource(configSource);
                    m_ViewConfig = viewConfig;
                    if (GridSizeChangedAction != null)
                        GridSizeChangedAction(m_ViewConfig.GridSize);
                }
                else
                {
                    viewConfig = new GridViewConfig();
                    viewConfig.SetConfigSource(configSource);
                    configSource.SetValue(configName, viewConfig);
                }
            }
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            EndRename(true);
        }

        public bool HasFocus()
        {
            if (!ViewConfig.AllowFocusRendering)
                return true;

            if (m_KeyboardControlID == GUIUtility.keyboardControl)
                return EditorWindowWrap.HasFocus(Owner.WindowOwner);
            return false;
        }

        public override void OnGUI(Rect rect)
        {
            m_KeyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

            base.OnGUI(rect);

            OnEvent();

            if (s_Styles == null)
                s_Styles = new Styles();


            m_TotalRect = rect;
            GUI.Label(m_TotalRect, GUIContent.none, s_Styles.iconAreaBg);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = m_KeyboardControlID;
                AllowRenameOnMouseUp = true;
            }

            bool flag = m_KeyboardControlID == GUIUtility.keyboardControl;
            if (flag != m_HadKeyboardFocusLastEvent)
            {
                m_HadKeyboardFocusLastEvent = flag;
                if (flag)
                {
                    if (Event.current.type == EventType.MouseDown)
                        AllowRenameOnMouseUp = false;
                }
            }

            HandleKeyboard(true);
            HandleZoomScrolling();
            HandleLayout();
            DoOffsetSelection();
            HandleUnusedEvents();
        }

        public void HandleKeyboard(bool checkKeyboardControl)
        {
            if (checkKeyboardControl && GUIUtility.keyboardControl != m_KeyboardControlID || !GUI.enabled)
                return;

            if (KeyboardCallback != null)
                KeyboardCallback();

            if (Event.current.type != EventType.KeyDown)
                return;

            int num = 0;
            if (IsLastClickedItemVisible()) //最后一个被点击的item的可见
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.UpArrow:
                        num = -m_GridLayout.LayoutParams.Columns;
                        break;
                    case KeyCode.DownArrow:
                        num = m_GridLayout.LayoutParams.Columns;
                        break;
                    case KeyCode.RightArrow:
                        if (AllowLeftRightArrowNavigation())
                        {
                            num = 1;
                        }
                        break;
                    case KeyCode.LeftArrow:
                        if (AllowLeftRightArrowNavigation())
                        {
                            num = -1;
                        }
                        break;
                    case KeyCode.Home:
                        num = int.MinValue;
                        break;
                    case KeyCode.End:
                        num = int.MaxValue;
                        break;
                    case KeyCode.PageUp:
                        num = kPageUp;
                        break;
                    case KeyCode.PageDown:
                        num = kPageDown;
                        break;
                }
            }
            else
            {
                bool flag = false;
                switch (Event.current.keyCode)
                {
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                    case KeyCode.Home:
                    case KeyCode.End:
                    case KeyCode.PageUp:
                    case KeyCode.PageDown:
                        flag = true;
                        break;
                    case KeyCode.RightArrow:
                    case KeyCode.LeftArrow:
                        flag = AllowLeftRightArrowNavigation();
                        break;
                }

                if (flag)
                {
                    SelectFirst();
                    Event.current.Use();
                }
            }

            if (num != 0)
            {
                if (GetSelectedItemIndex() < 0)
                    SetSelectedItemByIndex(0);
                else
                    m_SelectionOffset = num;
                Event.current.Use();
                GUI.changed = true;
            }
            else
            {
                if (!ViewConfig.AllowFindNextShortcut || !m_GridLayout.DoCharacterOffsetSelection())
                    return;
                Event.current.Use();
            }
        }

        private void HandleZoomScrolling()
        {
            if (!EditorGUI.actionKey || Event.current.type != EventType.ScrollWheel ||
                !m_TotalRect.Contains(Event.current.mousePosition))
                return;

            var minGridSize = m_GridLayout.LayoutParams.MinGridSize;
            var minIconSize = m_GridLayout.LayoutParams.MinIconSize;
            var maxGridSize = m_GridLayout.LayoutParams.MaxGridSize;
            int delta = Event.current.delta.y <= 0.0 ? 1 : -1;
            GridSize = Mathf.Clamp(GridSize + delta * 7, minGridSize, maxGridSize);

            if (delta < 0 && GridSize < minIconSize)
                GridSize = minGridSize;

            if (delta > 0 && GridSize < minIconSize)
                GridSize = minIconSize;

            Event.current.Use();
            GUI.changed = true;
        }

        private void HandleLayout()
        {
            if (Event.current.type == EventType.Repaint)
            {
                CalculateLayout();
            }

            Rect viewRect = new Rect(0.0f, 0.0f, 1f, m_GridLayout.LayoutParams.Height);
            bool moreHigh = m_GridLayout.LayoutParams.Height > m_TotalRect.height;

            m_VisibleRect = m_TotalRect;
            var scrollbarWidth = 16f;
            if (moreHigh)
                m_VisibleRect.width -= scrollbarWidth;

            m_ViewConfig.ScrollPosition = GUI.BeginScrollView(m_TotalRect, m_ViewConfig.ScrollPosition, viewRect);

            Vector2 scrollPos = m_ViewConfig.ScrollPosition;
            m_GridLayout.Draw(0, scrollPos);

            HandlePing();
            GUI.EndScrollView();
        }

        private void DoOffsetSelection()
        {
            if (m_SelectionOffset == 0)
                return;

            int maxIdx = GetMaxIdx();
            if (m_GridLayout.LayoutParams.MaxGridSize == -1)
                return;

            int selectedAssetIdx = GetSelectedItemIndex();
            int idx = selectedAssetIdx >= 0 ? selectedAssetIdx : 0;
            DoOffsetSelectionSpecialKeys(idx, maxIdx);
            if (m_SelectionOffset == 0)
                return;

            int a = idx + m_SelectionOffset;
            m_SelectionOffset = 0;
            SetSelectedItemByIndex(a >= 0 ? Mathf.Min(a, maxIdx) : idx);
        }

        private void HandleUnusedEvents()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (!ViewConfig.AllowDeselection || !m_TotalRect.Contains(Event.current.mousePosition))
                    {
                        return;
                    }
                    SetSelection(new int[0], false);
                    Event.current.Use();
                    break;
                case EventType.ContextClick:
                    if (m_TotalRect.Contains(Event.current.mousePosition) && GridViewPopupMenuAction != null)
                    {
                        GridViewPopupMenuAction();
                        Event.current.Use();
                    }
                    break;
            }
        }

        public void OnEvent()
        {
            GetRenameOverlay().OnEvent();
        }

        public RenameOverlay GetRenameOverlay()
        {
            return m_RenameOverlay;
        }

        private int GetMaxIdx()
        {
            return m_GridLayout.ItemCount > 0 ? m_GridLayout.ItemCount - 1 : -1;
        }

        private void DoOffsetSelectionSpecialKeys(int idx, int maxIndex)
        {
            float num = m_GridLayout.LayoutParams.ItemSize.y + m_GridLayout.LayoutParams.VerticalSpacing;
            int columns = m_GridLayout.LayoutParams.Columns;
            switch (m_SelectionOffset)
            {
                case kPageDown:
                    if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        m_ViewConfig.ScrollPositionY += m_TotalRect.height;
                        m_SelectionOffset = 0;
                        break;
                    }
                    m_SelectionOffset = Mathf.RoundToInt(m_TotalRect.height / num) * columns;
                    m_SelectionOffset = Mathf.Min(Mathf.FloorToInt((maxIndex - idx) / (float)columns) * columns, m_SelectionOffset);
                    break;
                case int.MaxValue:
                    m_SelectionOffset = maxIndex - idx;
                    break;
                case int.MinValue:
                    m_SelectionOffset = 0;
                    SetSelectedItemByIndex(0);
                    break;
                case kPageUp:
                    if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        m_ViewConfig.ScrollPositionY -= m_TotalRect.height;
                        m_SelectionOffset = 0;
                        break;
                    }
                    m_SelectionOffset = -Mathf.RoundToInt(m_TotalRect.height / num) * columns;
                    m_SelectionOffset = Mathf.Max(-Mathf.FloorToInt(idx / (float)columns) * columns, m_SelectionOffset);
                    break;
            }
        }

        public bool IsSelected(int itemId)
        {
            return m_ViewConfig.SelectedItemIdList.Contains(itemId);
        }

        public int[] GetSelection()
        {
            return m_ViewConfig.SelectedItemIdList.ToArray();
        }

        public bool IsLastClickedItemVisible()
        {
            return GetSelectedItemIndex() >= 0;
        }

        public void SelectAll()
        {
            SetSelection(m_DataSource.GetAllItemId().ToArray(), false);
        }

        public void SelectFirst()
        {
            int selectedIdx = 0;
            if (m_GridLayout.ItemCount > 0)
                selectedIdx = 0;
            SetSelectedItemByIndex(selectedIdx);
        }

        /// <summary>
        /// 得到选中的item的索引
        /// </summary>
        private int GetSelectedItemIndex()
        {
            return m_DataSource.GetItemIndexByItemId(m_ViewConfig.LastClickedItemId);
        }

        public void InitSelection(int[] selectedInstanceIDs)
        {
            m_ViewConfig.SelectedItemIdList = new List<int>(selectedInstanceIDs);
            if (m_ViewConfig.SelectedItemIdList.Count > 0)
            {
                if (!m_ViewConfig.SelectedItemIdList.Contains(m_ViewConfig.LastClickedItemId))
                    m_ViewConfig.LastClickedItemId = m_ViewConfig.SelectedItemIdList[m_ViewConfig.SelectedItemIdList.Count - 1];
            }
            else
                m_ViewConfig.LastClickedItemId = 0;
        }

        public void SetSelection(int[] selectedItemIdArray, bool doubleClicked)
        {
            InitSelection(selectedItemIdArray);
            if (!doubleClicked && ItemSelectedAction != null)
            {
                ItemSelectedAction(selectedItemIdArray);
            }
        }

        private void SetSelectedItemByIndex(int selectedIndex)
        {
            int itemId;
            if (m_GridLayout.ItemIdAtIndex(selectedIndex, out itemId))
            {
                ScrollToPosition(AdjustRectForFraming(m_GridLayout.LayoutParams.CalculateItemRect(selectedIndex)));
                int[] selectedInstanceIDs;
                if (IsItemCurrentlySelected())
                {
                    selectedInstanceIDs = m_GridLayout.GetNewSelection(itemId, false, true).ToArray();
                }
                else
                {
                    int[] numArray = new int[1];
                    numArray[0] = itemId;
                    selectedInstanceIDs = numArray;
                }
                SetSelection(selectedInstanceIDs, false);
                m_ViewConfig.LastClickedItemId = itemId;
            }
        }

        private bool IsItemCurrentlySelected()
        {
            int itemId = m_ViewConfig.SelectedItemIdList.FirstOrDefault();
            if (itemId != 0)
                return m_DataSource.GetItemIndexByItemId(itemId) != -1;
            return false;
        }

        public static Rect AdjustRectForFraming(Rect r)
        {
            r.height += s_Styles.resultsGridLabel.fixedHeight * 2f;
            r.y -= s_Styles.resultsGridLabel.fixedHeight;
            return r;
        }

        public void ScrollToPosition(Rect r)
        {
            float y = r.y;
            float yMax = r.yMax;
            float height = m_TotalRect.height;

            if (yMax > height + m_ViewConfig.ScrollPosition.y)
                m_ViewConfig.ScrollPositionY = yMax - height;

            if (y < m_ViewConfig.ScrollPosition.y)
                m_ViewConfig.ScrollPositionY = y;

            m_ViewConfig.ScrollPositionY = Mathf.Max(m_ViewConfig.ScrollPosition.y, 0.0f);
        }

        private PingData m_Ping = new PingData();
        private int m_pingIndex;
        private int m_LeftPaddingForPinging;
        public void BeginPing(int itemId)
        {
            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }

            var index = m_DataSource.GetItemIndexByItemId(itemId);
            if (index != -1)
            {
                var item = m_DataSource.ItemList[index];
                string fullText = item.DisplayName;
                if (fullText != null)
                {
                    m_Ping.m_TimeStart = Time.realtimeSinceStartup;
                    m_Ping.m_AvailableWidth = m_VisibleRect.width;
                    m_pingIndex = index;
                    GUIContent content = new GUIContent(!m_GridLayout.ListMode ? GetCroppedLabelText(itemId, fullText, m_WidthUsedForCroppingName) : fullText);

                    if (m_GridLayout.ListMode)
                    {
                        m_Ping.m_PingStyle = s_Styles.ping;
                        Vector2 vector = m_Ping.m_PingStyle.CalcSize(content);
                        m_Ping.m_ContentRect.width = vector.x + 16f;
                        m_Ping.m_ContentRect.height = vector.y;
                        m_LeftPaddingForPinging = item.IsChildItem ? 0x1c : 16;
                        m_Ping.m_ContentDraw = (rect) =>
                        {
                            GridLayout.DrawIconAndLabel(rect, item.DisplayName, (Texture2D)item.Texture, false, false);
                        };
                    }
                    else
                    {
                        m_Ping.m_PingStyle = s_Styles.miniPing;
                        Vector2 vector2 = m_Ping.m_PingStyle.CalcSize(content);
                        m_Ping.m_ContentRect.width = vector2.x;
                        m_Ping.m_ContentRect.height = vector2.y;
                        m_Ping.m_ContentDraw = (rect) =>
                        {
                            TextAnchor alignment = s_Styles.resultsGridLabel.alignment;
                            s_Styles.resultsGridLabel.alignment = TextAnchor.UpperLeft;
                            s_Styles.resultsGridLabel.Draw(rect, content.text, false, false, false, false);
                            s_Styles.resultsGridLabel.alignment = alignment;
                        };
                    }

                    Vector2 vector3 = CalculatePingPosition();
                    m_Ping.m_ContentRect.x = vector3.x;
                    m_Ping.m_ContentRect.y = vector3.y;
                    Repaint();
                }
            }
        }

        private Vector2 CalculatePingPosition()
        {
            Rect rect = m_GridLayout.LayoutParams.CalculateItemRect(m_pingIndex);
            if (m_GridLayout.ListMode)
            {
                return new Vector2(m_LeftPaddingForPinging, rect.y);
            }
            float width = m_Ping.m_ContentRect.width;
            return new Vector2((rect.center.x - (width / 2f)) + m_Ping.m_PingStyle.padding.left, (rect.yMax - s_Styles.resultsGridLabel.fixedHeight) + 3f);
        }

        public void EndPing()
        {
            m_Ping.m_TimeStart = -1f;
        }

        private void HandlePing()
        {
            if (m_Ping.isPinging && !m_GridLayout.ListMode)
            {
                Vector2 pingPosition = CalculatePingPosition();
                m_Ping.m_ContentRect.x = pingPosition.x;
                m_Ping.m_ContentRect.y = pingPosition.y;
            }
            m_Ping.HandlePing();
            if (!m_Ping.isPinging)
                return;
            Repaint();
        }

        private bool AllowLeftRightArrowNavigation()
        {
            if (!m_GridLayout.LayoutParams.ListMode && !Event.current.alt)
                return m_GridLayout.ItemCount > 1;
            return false;
        }

        private bool IsListMode()
        {
            if (ViewConfig.AllowMultiSelect)
                return GridSize == 16;
            return false;
        }

        private void CalculateLayout()
        {
            if (GridSize < 20)
                GridSize = m_GridLayout.LayoutParams.MinGridSize;
            else if (GridSize < m_GridLayout.LayoutParams.MinIconSize)
                GridSize = m_GridLayout.LayoutParams.MinIconSize;

            if (IsListMode())
            {
                m_GridLayout.ListMode = true;
                UpdateGroupSizes(m_GridLayout);
            }
            else
            {
                m_GridLayout.ListMode = false;
                UpdateGroupSizes(m_GridLayout);

                if (m_TotalRect.height < m_GridLayout.LayoutParams.Height)
                {
                    m_GridLayout.LayoutParams.FixedWidth = m_TotalRect.width - 16f;
                    m_GridLayout.LayoutParams.CalculateLayoutParams(m_GridLayout.ItemCount, m_GridLayout.LayoutParams.CalculateRows(m_GridLayout.ItemsWantedShown));
                }
            }
        }

        private void UpdateGroupSizes(GridLayout g)
        {
            if (g.ListMode)
            {
                g.LayoutParams.FixedWidth = m_VisibleRect.width;
                g.LayoutParams.ItemSize = new Vector2(m_VisibleRect.width, 16f);
                g.LayoutParams.TopMargin = 0.0f;
                g.LayoutParams.BottomMargin = 0.0f;
                g.LayoutParams.LeftMargin = 0.0f;
                g.LayoutParams.RightMargin = 0.0f;
                g.LayoutParams.VerticalSpacing = 0.0f;
                g.LayoutParams.MinHorizontalSpacing = 0.0f;
                g.LayoutParams.CalculateLayoutParams(g.ItemCount, g.ItemsWantedShown);
            }
            else
            {
                g.LayoutParams.FixedWidth = m_TotalRect.width;
                g.LayoutParams.ItemSize = new Vector2(GridSize, GridSize + 14);
                g.LayoutParams.TopMargin = 10;
                g.LayoutParams.BottomMargin = 10;
                g.LayoutParams.LeftMargin = 10;
                g.LayoutParams.RightMargin = 10;
                g.LayoutParams.VerticalSpacing = 15f;
                g.LayoutParams.MinHorizontalSpacing = 12f;
                g.LayoutParams.CalculateLayoutParams(g.ItemCount, g.LayoutParams.CalculateRows(g.ItemsWantedShown));
            }
        }

        #region 重命名Item

        public bool BeginRename(float delay)
        {
            if (!ViewConfig.AllowRenaming || m_ViewConfig.SelectedItemIdList.Count != 1)
                return false;

            int id = m_ViewConfig.SelectedItemIdList[0];
            GridItem item = null;
            bool findItem = false;
            for (int i = 0, count = m_GridLayout.ItemCount; i < count; ++i)
            {
                item = m_DataSource.GetItemByIndex(i);
                if (item.Id == id)
                {
                    findItem = true;
                    if (!m_GridViewHandler.AcceptRename(item))
                        return false;
                    break;
                }
            }

            if (!findItem)
            {
                return false;
            }

            if (BeginRenameAction != null)
            {
                if (!BeginRenameAction(item))
                {
                    return false;
                }
            }

            return GetRenameOverlay().BeginRename(item.DisplayName, id, delay);
        }

        public void EndRename(bool acceptChanges)
        {
            if (!GetRenameOverlay().IsRenaming())
                return;
            GetRenameOverlay().EndRename(acceptChanges);
            RenameEnded();
        }

        private void RenameEnded()
        {
            string name = !string.IsNullOrEmpty(GetRenameOverlay().name) ? GetRenameOverlay().name : GetRenameOverlay().originalName;
            int id = GetRenameOverlay().userData;

            if (GetRenameOverlay().userAcceptedRename)
            {
                if (m_InstanceIDToCroppedNameMap.ContainsKey(id))
                {
                    m_InstanceIDToCroppedNameMap.Remove(id);
                }

                for (int i = 0, count = m_GridLayout.ItemCount; i < count; ++i)
                {
                    var item = m_DataSource.GetItemByIndex(i);
                    if (item.Id == id)
                    {
                        var oldName = item.DisplayName;
                        item.DisplayName = name;
                        if (RenameEndAction != null)
                        {
                            RenameEndAction(item, oldName, name);
                        }
                        break;
                    }
                }
            }

            if (GetRenameOverlay().HasKeyboardFocus())
                GUIUtility.keyboardControl = m_KeyboardControlID;

            ClearRenameState();
        }

        private void ClearRenameState()
        {
            GetRenameOverlay().Clear();
        }

        internal void HandleRenameOverlay()
        {
            if (!GetRenameOverlay().IsRenaming() ||
                GetRenameOverlay().OnGUI(!IsListMode() ? s_Styles.miniRenameField : null))
                return;
            RenameEnded();
            GUIUtility.ExitGUI();
        }

        public bool Frame(int itemId, bool frame)
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            int itemIdx = m_DataSource.GetItemIndexByItemId(itemId);

            if (itemIdx == -1)
                return false;

            if (frame)
            {
                CenterRect(AdjustRectForFraming(m_GridLayout.LayoutParams.CalculateItemRect(itemIdx)));
            }
            return true;
        }

        private void CenterRect(Rect r)
        {
            m_ViewConfig.ScrollPositionY = (r.yMax + r.yMin) / 2.0f - m_TotalRect.height / 2f;
            ScrollToPosition(r);
        }

        #endregion

        #region 文字适应

        /// <summary>
        /// 清除被裁剪的文本名称的缓存
        /// </summary>
        private void ClearCroppedLabelCache()
        {
            m_InstanceIDToCroppedNameMap.Clear();
        }

        /// <summary>
        /// 获取文本被宽度裁剪后的文本
        /// </summary>
        public string GetCroppedLabelText(int instanceID, string fullText, float cropWidth)
        {
            if (m_WidthUsedForCroppingName != (int)cropWidth)
                ClearCroppedLabelCache();

            string str;
            if (!m_InstanceIDToCroppedNameMap.TryGetValue(instanceID, out str))
            {
                if (m_InstanceIDToCroppedNameMap.Count > GetMaxNumVisibleItems() * 2 + 30)
                    ClearCroppedLabelCache();

                int thatFitWithinWidth = GUIStyleWrap.GetNumCharactersThatFitWithinWidth(s_Styles.resultsGridLabel, fullText,
                    cropWidth);
                if (thatFitWithinWidth == -1)
                {
                    return fullText;
                }
                str = thatFitWithinWidth <= 1 || thatFitWithinWidth == fullText.Length
                    ? fullText
                    : fullText.Substring(0, thatFitWithinWidth - 1) + "…";
                m_InstanceIDToCroppedNameMap[instanceID] = str;
                m_WidthUsedForCroppingName = (int)cropWidth;
            }
            return str;
        }

        private int GetMaxNumVisibleItems()
        {
            return m_GridLayout.LayoutParams.GetMaxVisibleItems(m_TotalRect.height);
        }

        #endregion

        public class Styles
        {
            public GUIStyle resultsLabel = new GUIStyle("PR Label");
            public GUIStyle resultsGridLabel = GetStyle("ProjectBrowserGridLabel");
            public GUIStyle resultsGrid = "ObjectPickerResultsGrid";
            public GUIStyle background = "ObjectPickerBackground";
            public GUIStyle previewTextureBackground = "ObjectPickerPreviewBackground";
            public GUIStyle groupHeaderMiddle = GetStyle("ProjectBrowserHeaderBgMiddle");
            public GUIStyle groupHeaderTop = GetStyle("ProjectBrowserHeaderBgTop");
            public GUIStyle groupHeaderLabel = "Label";
            public GUIStyle groupHeaderLabelCount = "MiniLabel";
            public GUIStyle groupFoldout = "Foldout";
            public GUIStyle toolbarBack = "ObjectPickerToolbar";
            public GUIStyle miniRenameField = new GUIStyle("PR TextField");
            public GUIStyle ping = new GUIStyle("PR Ping");
            public GUIStyle miniPing = new GUIStyle("PR Ping");
            public GUIStyle iconDropShadow = GetStyle("ProjectBrowserIconDropShadow");
            public GUIStyle textureIconDropShadow = GetStyle("ProjectBrowserTextureIconDropShadow");
            public GUIStyle iconAreaBg = GetStyle("ProjectBrowserIconAreaBg");
            public GUIStyle previewBg = GetStyle("ProjectBrowserPreviewBg");
            public GUIStyle subAssetBg = GetStyle("ProjectBrowserSubAssetBg");
            public GUIStyle subAssetBgOpenEnded = GetStyle("ProjectBrowserSubAssetBgOpenEnded");
            public GUIStyle subAssetBgCloseEnded = GetStyle("ProjectBrowserSubAssetBgCloseEnded");
            public GUIStyle subAssetBgMiddle = GetStyle("ProjectBrowserSubAssetBgMiddle");
            public GUIStyle subAssetBgDivider = GetStyle("ProjectBrowserSubAssetBgDivider");
            public GUIStyle subAssetExpandButton = GetStyle("ProjectBrowserSubAssetExpandBtn");
            public GUIContent m_AssetStoreNotAvailableText = new GUIContent("The Asset Store is not available");
            public GUIStyle resultsFocusMarker;

            public Styles()
            {
                resultsFocusMarker = new GUIStyle(resultsGridLabel);
                GUIStyle guiStyle = resultsFocusMarker;
                float num1 = 0.0f;
                resultsFocusMarker.fixedWidth = num1;
                double num2 = num1;
                guiStyle.fixedHeight = (float)num2;
                miniRenameField.font = EditorStyles.miniLabel.font;
                miniRenameField.alignment = TextAnchor.LowerCenter;
                ping.fixedHeight = 16f;
                ping.padding.right = 10;
                miniPing.font = EditorStyles.miniLabel.font;
                miniPing.alignment = TextAnchor.MiddleCenter;
                resultsLabel.alignment = TextAnchor.MiddleLeft;
            }

            private static GUIStyle GetStyle(string styleName)
            {
                return styleName;
            }
        }
    }
}