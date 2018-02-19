using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class GenericGridLayouter : GridLayouter
    {
        private GUIContent m_Content = new GUIContent();
        private List<int> m_DragSelection = new List<int>();
        private int m_DropTargetControlID;

        public GenericGridLayouter(GridViewDataSource dataSource) : base(dataSource)
        {

        }

        protected void DrawSubAssetRowBg(int startSubAssetIndex, int endSubAssetIndex, bool continued)
        {
            Rect rect1 = LayoutParams.CalculateItemRect(startSubAssetIndex);
            Rect rect2 = LayoutParams.CalculateItemRect(endSubAssetIndex);
            float num1 = 30f;
            float num2 = 128f;
            float num3 = rect1.width / num2;
            float num4 = 9f * num3;
            float num5 = 4f;
            float num6 = startSubAssetIndex % this.LayoutParams.Columns != 0
                ? this.LayoutParams.HorizontalSpacing + num3 * 10f
                : 18f * num3;
            Rect position1 = new Rect(rect1.x - num6, rect1.y + num5, num1 * num3,
                (float) ((double) rect1.width - (double) num5 * 2.0 + (double) num4 - 1.0));
            position1.y = Mathf.Round(position1.y);
            position1.height = Mathf.Ceil(position1.height);
            GridView.s_Styles.subAssetBg.Draw(position1, GUIContent.none, false, false, false, false);
            float width = num1 * num3;
            bool flag = endSubAssetIndex % this.LayoutParams.Columns == this.LayoutParams.Columns - 1;
            float num7 = continued || flag ? 16f * num3 : 8f * num3;
            Rect position2 = new Rect(rect2.xMax - width + num7, rect2.y + num5, width, position1.height);
            position2.y = Mathf.Round(position2.y);
            position2.height = Mathf.Ceil(position2.height);
            (!continued ? GridView.s_Styles.subAssetBgCloseEnded : GridView.s_Styles.subAssetBgOpenEnded).Draw(
                position2, GUIContent.none, false, false, false, false);
            position1 = new Rect(position1.xMax, position1.y, position2.xMin - position1.xMax, position1.height);
            position1.y = Mathf.Round(position1.y);
            position1.height = Mathf.Ceil(position1.height);
            GridView.s_Styles.subAssetBgMiddle.Draw(position1, GUIContent.none, false, false, false, false);
        }

        private void DrawSubAssetBackground(int beginIndex, int endIndex)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            int columns = LayoutParams.Columns;
            int num = (endIndex - beginIndex) / columns + 1;
            for (int index1 = 0; index1 < num; ++index1)
            {
                int startSubAssetIndex = -1;
                int endSubAssetIndex = -1;
                for (int index2 = 0; index2 < columns; ++index2)
                {
                    int index3 = beginIndex + (index2 + index1 * columns);
                    if (index3 < ItemCount)
                    {
                        if (m_DataSource.GetItemByIndex(index3).IsChildItem)
                        {
                            if (startSubAssetIndex == -1)
                                startSubAssetIndex = index3;
                            endSubAssetIndex = index3;
                        }
                        else if (startSubAssetIndex != -1)
                        {
                            DrawSubAssetRowBg(startSubAssetIndex, endSubAssetIndex, false);
                            startSubAssetIndex = -1;
                            endSubAssetIndex = -1;
                        }
                    }
                    else
                        break;
                }

                if (startSubAssetIndex != -1)
                {
                    bool continued = false;
                    if (index1 < num - 1)
                    {
                        int index2 = beginIndex + (index1 + 1) * columns;
                        if (index2 < ItemCount)
                            continued = m_DataSource.GetItemByIndex(index2).IsChildItem;
                    }
                    DrawSubAssetRowBg(startSubAssetIndex, endSubAssetIndex, continued);
                }
            }
        }

        protected override void DrawInternal(int beginItemIndex, int endItemIndex)
        {
            if (!ListMode && !m_Owner.ViewHandler.SearchMode)
                DrawSubAssetBackground(beginItemIndex, endItemIndex);

            int itemIndex = beginItemIndex;
            while (true)
            {
                if (itemIndex > endItemIndex || itemIndex >= ItemCount)
                {
                    break;
                }

                DrawItem(LayoutParams.CalculateItemRect(itemIndex), m_DataSource.GetItemIdByIndex(itemIndex),
                    itemIndex);
                ++itemIndex;
            }
        }

        private void SelectAndFrameParentOf(int itemId, int index)
        {
            int parentId = 0;

            for (int i = index; i >= 0; --i)
            {
                var item = m_DataSource.GetItemByIndex(i);
                if (item.Id == itemId)
                {
                    parentId = item.Id;
                    break;
                }
            }

            if (parentId == 0)
                return;

            int[] selectedInstanceIDs = new int[1];
            selectedInstanceIDs[0] = parentId;
            m_Owner.SetSelection(selectedInstanceIDs, false);
            m_Owner.Frame(parentId, true);
        }

        public void ChangeExpandedState(int itemId, bool expanded)
        {
            var result = m_Owner.ViewConfig.ExpandedItemIdList.Remove(itemId);
            if (expanded)
            {
                m_Owner.ViewConfig.ExpandedItemIdList.Add(itemId);
                result = true;
            }

            if (result)
                m_Owner.ViewConfig.SetDirty();

            if (m_Owner.ItemExpandedAction != null)
                m_Owner.ItemExpandedAction(itemId, expanded);
        }

        private bool IsExpanded(int itemId)
        {
            return m_Owner.ViewConfig.ExpandedItemIdList.IndexOf(itemId) >= 0;
        }

        private void DrawItem(Rect itemRect, int itemId, int itemIndex)
        {
            Event current = Event.current;
            Rect selectionRect = itemRect;

            var item = m_DataSource.GetItemByIndex(itemIndex);
            var viewHanlder = m_Owner.ViewHandler;
            bool hasChild = item.HasChildren && viewHanlder.HasChildren(item) && !viewHanlder.SearchMode;
            int controlId = GetControlIdFromInstanceId(item);
            bool isSelected = !m_Owner.ViewConfig.AllowDragging
                ? m_Owner.IsSelected(itemId)
                : (m_DragSelection.Count <= 0 ? m_Owner.IsSelected(itemId) : m_DragSelection.Contains(itemId));

            //网格模式下如果对象有子资源的话，生成展开按钮的位置
            Rect expandBtnRect = new Rect(itemRect.x + 2f, itemRect.y, 16f, 16f);
            ;
            if (hasChild && !ListMode)
            {
                float ratio = itemRect.height / 128f;
                float width = 28f * ratio;
                float height = 32f * ratio;
                expandBtnRect = new Rect(itemRect.xMax - width * 0.5f,
                    itemRect.y + ((itemRect.height - GridView.s_Styles.resultsGridLabel.fixedHeight) * 0.5f -
                                  width * 0.5f), width, height);
            }

            //快捷键展开Item的子资源的事件处理
            bool changeState = false;
            if (isSelected && !m_Owner.ViewHandler.SearchMode && current.type == EventType.KeyDown &&
                m_Owner.HasFocus())
            {
                switch (current.keyCode)
                {
                    case KeyCode.RightArrow:
                        if (ListMode || Event.current.alt)
                        {
                            if (!IsExpanded(itemId) && !item.IsChildItem)
                                changeState = true;
                            current.Use();
                        }
                        break;
                    case KeyCode.LeftArrow:
                        if (ListMode || Event.current.alt)
                        {
                            if (IsExpanded(itemId) && !item.IsChildItem)
                                changeState = true;
                            else
                                SelectAndFrameParentOf(itemId, itemIndex);
                            current.Use();
                        }
                        break;
                }
            }

            //点击到了网格模式下Item的展开按钮
            if (hasChild && !item.IsChildItem && current.type == EventType.MouseDown &&
                (current.button == 0 && expandBtnRect.Contains(current.mousePosition)))
                changeState = true;

            if (changeState)
            {
                ChangeExpandedState(itemId, !IsExpanded(itemId));
                current.Use();
                GUIUtility.ExitGUI();
            }

            bool isRenaming = IsRenaming(itemId);

            //网格模式下DisplayName的显示位置
            Rect displayNameRect = itemRect;
            if (!ListMode)
                displayNameRect = new Rect(itemRect.x,
                    itemRect.yMax + 1f - GridView.s_Styles.resultsGridLabel.fixedHeight, itemRect.width - 1f,
                    GridView.s_Styles.resultsGridLabel.fixedHeight);

            float x = 16;
            if (ListMode)
            {
                selectionRect.x = x;
                if (item.IsChildItem && !m_Owner.ViewHandler.SearchMode)
                {
                    x = 28f;
                    selectionRect.x = x;
                }
                selectionRect.width -= selectionRect.x;
            }

            var itemName = item.DisplayName;
            if (Event.current.type == EventType.Repaint)
            {
                if (m_DropTargetControlID == controlId && !itemRect.Contains(current.mousePosition))
                    m_DropTargetControlID = 0;

                bool isDraging = controlId == m_DropTargetControlID &&
                                 m_DragSelection.IndexOf(m_DropTargetControlID) == -1;
                if (ListMode) //列表模式的item绘制
                {
                    if (isRenaming)
                    {
                        isSelected = false;
                        itemName = string.Empty;
                    }

                    itemRect.width = Mathf.Max(itemRect.width, 500f);
                    m_Content.text = itemName;
                    m_Content.image = item.Texture;

                    if (isSelected)
                    {
                        //绘制选中背景
                        GridView.s_Styles.resultsLabel.Draw(itemRect, GUIContent.none, false, false, true,
                            m_Owner.HasFocus());
                    }

                    if (isDraging)
                    {
                        //绘制外圈发光
                        GridView.s_Styles.resultsLabel.Draw(itemRect, GUIContent.none, true, true, false, false);
                    }

                    //绘制图标和文本
                    DrawIconAndLabel(new Rect(x, itemRect.y, itemRect.width - x, itemRect.height), itemName,
                        m_Content.image as Texture2D, isSelected, m_Owner.HasFocus());

                    //在列表模式中，如果一个对象有子对象可以有一个箭头按钮指示
                    if (hasChild && !item.IsChildItem)
                    {
                        //绘制最左边的展开和收缩小箭头
                        GridView.s_Styles.groupFoldout.Draw(expandBtnRect, !ListMode, !ListMode, IsExpanded(itemId),
                            false);
                    }
                }
                else
                {
                    m_Content.image = item.Texture;

                    float gap = m_Content.image == null ? 0.0f : 2f;
                    itemRect.height -= GridView.s_Styles.resultsGridLabel.fixedHeight + 2f * gap;
                    itemRect.y += gap;

                    Rect rect = m_Content.image != null
                        ? GetImageDrawPosition(itemRect, m_Content.image.width, m_Content.image.height)
                        : new Rect();
                    m_Content.text = null;

                    Color oldColor = GUI.color;

                    //图标被选中时的颜色
                    if (isSelected)
                        GUI.color *= new Color(0.85f, 0.9f, 1f);

                    //绘制图标
                    if (m_Content.image != null)
                    {
                        GridView.s_Styles.resultsGrid.Draw(rect, m_Content, false, false, isSelected,
                            m_Owner.HasFocus());
                    }

                    //文本被选中时的颜色
                    if (isSelected)
                        GUI.color = oldColor;

                    if (!isRenaming)
                    {
                        if (isDraging)
                        {
                            //绘制外圈发光
                            GridView.s_Styles.resultsLabel.Draw(
                                new Rect(displayNameRect.x - 10f, displayNameRect.y, displayNameRect.width + 20f,
                                    displayNameRect.height), GUIContent.none, true, true, false, false);
                        }

                        string croppedLabelText = m_Owner.GetCroppedLabelText(itemId, itemName, itemRect.width);
                        GridView.s_Styles.resultsGridLabel.Draw(displayNameRect, croppedLabelText, false, false,
                            isSelected, m_Owner.HasFocus());
                    }

                    if (hasChild && !item.IsChildItem)
                    {
                        //绘制展开按钮
                        GridView.s_Styles.subAssetExpandButton.Draw(expandBtnRect, !ListMode, !ListMode,
                            IsExpanded(itemId) && !item.IsChildItem, false);
                    }
                }
            }

            if (isRenaming)
            {
                if (ListMode)
                {
                    displayNameRect.x = selectionRect.x + 16;
                    displayNameRect.width -= displayNameRect.x;
                }
                else
                {
                    displayNameRect.x -= 4f;
                    displayNameRect.width += 8f;
                }

                m_Owner.GetRenameOverlay().editFieldRect = displayNameRect;
                m_Owner.HandleRenameOverlay();
            }

            if (m_Owner.ViewConfig.AllowDragging)
            {
                HandleMouseWithDragging(itemId, controlId, itemRect, item);
            }
            else
            {
                HandleMouseWithoutDragging(itemId, controlId, itemRect, item);
            }
        }

        protected override void HandleUnusedDragEvents(float yOffset)
        {
            if (!m_Owner.ViewConfig.AllowDragging)
                return;

            Event current = Event.current;
            switch (current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!new Rect(m_Owner.m_TotalRect.x, m_Owner.m_TotalRect.y, m_Owner.m_TotalRect.width,
                        m_Owner.m_TotalRect.height <= LayoutParams.Height
                            ? LayoutParams.Height
                            : m_Owner.m_TotalRect.height).Contains(current.mousePosition))
                        break;

                    bool perform = current.type == EventType.DragPerform;
                    DragAndDropVisualMode andDropVisualMode = m_Owner.ViewHandler.DoDrag(int.MinValue, perform);
                    if (perform && andDropVisualMode != DragAndDropVisualMode.None)
                        DragAndDrop.AcceptDrag();

                    DragAndDrop.visualMode = andDropVisualMode;
                    current.Use();
                    break;
            }
        }

        private void HandleMouseWithDragging(int itmeId, int controlId, Rect rect, GridItem item)
        {
            Event current = Event.current;
            EventType typeForControl = current.GetTypeForControl(controlId);
            switch (typeForControl)
            {
                case EventType.MouseDown:
                    if (Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        if (current.clickCount == 2)
                        {
                            m_Owner.SetSelection(new int[1]
                            {
                                itmeId
                            }, true);
                            if (m_Owner.ItemDoubleClickAction != null)
                            {
                                m_Owner.ItemDoubleClickAction(item);
                            }
                            m_DragSelection.Clear();
                        }
                        else
                        {
                            m_DragSelection = GetNewSelection(itmeId, true, false);
                            GUIUtility.hotControl = controlId;
                            ((DragAndDropDelay) GUIUtility.GetStateObject(typeof(DragAndDropDelay), controlId))
                                .mouseDownPosition = Event.current.mousePosition;
                            m_Owner.ScrollToPosition(GridView.AdjustRectForFraming(rect));
                        }
                        current.Use();
                        break;
                    }
                    if (Event.current.button != 1 || !rect.Contains(Event.current.mousePosition))
                        break;

                    m_Owner.SetSelection(GetNewSelection(itmeId, true, false).ToArray(), false);
                    Event.current.Use();
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl != controlId)
                        break;

                    if (rect.Contains(current.mousePosition))
                    {
                        bool flag;
                        if (ListMode)
                        {
                            rect.x += 28f;
                            rect.width += 28f;
                            flag = rect.Contains(current.mousePosition);
                        }
                        else
                        {
                            rect.y = rect.y + rect.height - GridView.s_Styles.resultsGridLabel.fixedHeight;
                            rect.height = GridView.s_Styles.resultsGridLabel.fixedHeight;
                            flag = rect.Contains(current.mousePosition);
                        }
                        List<int> selectedInstanceIds = m_Owner.ViewConfig.SelectedItemIdList;
                        if (flag && m_Owner.ViewConfig.AllowRenaming &&
                            (m_Owner.AllowRenameOnMouseUp && selectedInstanceIds.Count == 1) &&
                            (selectedInstanceIds[0] == itmeId &&
                             !EditorGUIUtilityWrap.HasHolddownKeyModifiers(current)))
                            m_Owner.BeginRename(0.5f);
                        else
                            m_Owner.SetSelection(GetNewSelection(itmeId, false, false).ToArray(), false);
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    m_DragSelection.Clear();
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    if (((DragAndDropDelay) GUIUtility.GetStateObject(typeof(DragAndDropDelay), controlId))
                        .CanStartDrag())
                    {
                        m_Owner.ViewHandler.StartDrag(itmeId, m_DragSelection);
                        GUIUtility.hotControl = 0;
                    }
                    current.Use();
                    break;
                default:
                    if (typeForControl != EventType.DragUpdated && typeForControl != EventType.DragPerform)
                    {
                        if (typeForControl != EventType.DragExited)
                        {
                            if (typeForControl != EventType.ContextClick)
                                break;

                            if (!rect.Contains(current.mousePosition))
                                break;

                            if (m_Owner.GirdItemPopupMenuAction != null)
                            {
                                m_Owner.GirdItemPopupMenuAction(item);
                                current.Use();
                            }
                            break;
                        }
                        m_DragSelection.Clear();
                        break;
                    }
                    bool perform = current.type == EventType.DragPerform;
                    if (rect.Contains(current.mousePosition))
                    {
                        DragAndDropVisualMode andDropVisualMode = m_Owner.ViewHandler.DoDrag(itmeId, perform);
                        if (andDropVisualMode != DragAndDropVisualMode.None)
                        {
                            if (perform)
                                DragAndDrop.AcceptDrag();
                            m_DropTargetControlID = controlId;
                            DragAndDrop.visualMode = andDropVisualMode;
                            current.Use();
                        }
                        if (perform)
                            m_DropTargetControlID = 0;
                    }
                    if (!perform)
                        break;
                    m_DragSelection.Clear();
                    break;
            }
        }

        private void HandleMouseWithoutDragging(int itmeId, int controlId, Rect rect, GridItem item)
        {
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (current.button != 0 || !rect.Contains(current.mousePosition))
                        break;
                    m_Owner.Repaint();
                    if (current.clickCount == 1)
                        m_Owner.ScrollToPosition(GridView.AdjustRectForFraming(rect));
                    current.Use();
                    m_Owner.SetSelection(GetNewSelection(itmeId, false, false).ToArray(), current.clickCount == 2);
                    break;
                case EventType.ContextClick:
                    if (!rect.Contains(current.mousePosition))
                        break;
                    m_Owner.SetSelection(new int[1]
                    {
                        itmeId
                    }, false);

                    if (!rect.Contains(current.mousePosition))
                        break;

                    if (m_Owner.GirdItemPopupMenuAction != null)
                    {
                        m_Owner.GirdItemPopupMenuAction(item);
                        current.Use();
                    }
                    break;
            }
        }
    }
}