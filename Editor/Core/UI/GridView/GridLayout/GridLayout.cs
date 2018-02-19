using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace EUTK
{
    public abstract class GridLayout
    {
        protected GridView m_Owner;
        protected GridViewDataSource m_DataSource;
        protected GridLayoutParams m_LayoutParams = new GridLayoutParams();

        public bool ListMode
        {
            get { return LayoutParams.ListMode; }
            set { LayoutParams.ListMode = value; }
        }

        public int ItemCount
        {
            get { return m_DataSource.Count; }
        }

        public int ItemsWantedShown
        {
            get { return int.MaxValue; }
        }

        public GridLayoutParams LayoutParams
        {
            get { return m_LayoutParams; }
        }

        public GridView Owner
        {
            set { m_Owner = value; }
        }

        public GridViewDataSource DataSource
        {
            get { return m_DataSource; }
        }

        protected GridLayout(GridViewDataSource dataSource)
        {
            m_DataSource = dataSource;
        }

        public virtual bool DoCharacterOffsetSelection()
        {
            return false;
        }

        private int FirstVisibleRow(Vector2 scrollPos)
        {
            if (scrollPos.y > 0.0)
            {
                float itemHeightSpan = LayoutParams.ItemSize.y + LayoutParams.VerticalSpacing;
                return Mathf.Max(0, Mathf.FloorToInt(scrollPos.y / itemHeightSpan));
            }
            return 0;
        }

        /// <summary>
        /// 判断当前的滚动视图是否是在所有Items所占的显示区域内
        /// </summary>
        private bool IsInView(Vector2 scrollPos, float scrollViewHeight)
        {
            return scrollPos.y + scrollViewHeight >= 0 && LayoutParams.Height >= scrollPos.y;
        }

        public void Draw(float yOffset, Vector2 scrollPos)
        {
            if (!IsInView(scrollPos, m_Owner.VisibleRect.height))
                return;

            int firstVisibleItemIndex = FirstVisibleRow(scrollPos) * LayoutParams.Columns;
            if (firstVisibleItemIndex >= 0 && firstVisibleItemIndex < ItemCount)
            {
                int val1 = Math.Min(ItemCount, LayoutParams.Rows * LayoutParams.Columns);
                int num2 = (int)Math.Ceiling(m_Owner.VisibleRect.height /
                                             (double)(LayoutParams.ItemSize.y + LayoutParams.VerticalSpacing));
                int endItem = Math.Min(val1, firstVisibleItemIndex + num2 * LayoutParams.Columns + LayoutParams.Columns);
                DrawInternal(firstVisibleItemIndex, endItem);
            }

            HandleUnusedDragEvents(yOffset);
        }

        public virtual bool ItemIdAtIndex(int index, out int itemID)
        {
            itemID = 0;
            if (index >= LayoutParams.Rows * LayoutParams.Columns)
                return false;

            if (index >= 0 && index < ItemCount)
            {
                itemID = m_DataSource.GetItemIdByIndex(index);
            }

            return index < LayoutParams.Rows * LayoutParams.Columns;
        }

        public virtual List<int> GetNewSelection(int clickedItemId, bool beginOfDrag, bool useShiftAsActionKey)
        {
            List<int> itemIdList = m_DataSource.GetAllItemId();
            List<int> selectedItemIdList = m_Owner.ViewConfig.SelectedItemIdList;
            bool allowMultiSelect = m_Owner.ViewConfig.AllowMultiSelect;
            return InternalEditorUtility.GetNewSelection(clickedItemId, itemIdList, selectedItemIdList,
                m_Owner.ViewConfig.LastClickedItemId, beginOfDrag, useShiftAsActionKey, allowMultiSelect);
        }

        protected int GetControlIdFromInstanceId(GridItem item)
        {
            int controlId = GUIUtility.GetControlID(item.GetHashCode() + m_Owner.GetHashCode(), FocusType.Passive);
            return controlId;
        }

        protected bool IsRenaming(int itemId)
        {
            RenameOverlay renameOverlay = m_Owner.GetRenameOverlay();
            if (renameOverlay.IsRenaming() && renameOverlay.userData == itemId)
                return !renameOverlay.isWaitingForDelay;
            return false;
        }

        protected abstract void DrawInternal(int itemIdx, int endItem);


        protected virtual void HandleUnusedDragEvents(float yOffset)
        {

        }

        /// <summary>
        /// 绘制列表模式时的图标和文字
        /// </summary>
        public static void DrawIconAndLabel(Rect rect, string label, Texture2D icon, bool selected, bool focus)
        {
            GridView.s_Styles.resultsLabel.padding.left = (int)(16.0 + 2.0);
            GridView.s_Styles.resultsLabel.Draw(rect, label, false, false, selected, focus);
            Rect position = rect;
            position.width = 16f;
            if (icon != null)
                GUI.DrawTexture(position, icon);
        }

        protected static Rect GetImageDrawPosition(Rect position, float imageWidth, float imageHeight)
        {
            if (imageWidth <= position.width && imageHeight <= position.height)
                return new Rect(position.x + Mathf.Round((float)((position.width - imageWidth) / 2.0)), position.y + Mathf.Round((float)((position.height - imageHeight) / 2.0)), imageWidth, imageHeight);

            Rect outScreenRect = new Rect();
            Rect outSourceRect = new Rect();
            float imageAspect = imageWidth / imageHeight;
            GUIWrap.CalculateScaledTextureRects(position, ScaleMode.ScaleToFit, imageAspect, ref outScreenRect, ref outSourceRect);
            return outScreenRect;
        }
    }
}