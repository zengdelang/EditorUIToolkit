using System;
using System.Collections.Generic;
using UnityEditor;

namespace EUTK
{
    public class GridViewHandler
    {
        protected GridViewDataSource m_DataSource;

        public bool SearchMode { get; set; }

        public string GenericDragId { get; set; }

        public GridViewHandler(GridViewDataSource dataSource)
        {
            m_DataSource = dataSource;
        }

        public virtual bool AcceptRename(GridItem item)
        {
            return true;
        }

        public virtual bool HasChildren(GridItem item)
        {
            return true;
        }

        public virtual void StartDrag(int draggedItemId, List<int> selectedItemIdList)
        {
            if (GenericDragId == null)
                throw new NullReferenceException("You must specify a unique string for GenericDragId");

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(GenericDragId, GetItemList(selectedItemIdList));
            DragAndDrop.objectReferences = new UnityEngine.Object[0];
            DragAndDrop.StartDrag(selectedItemIdList.Count + " item" + (selectedItemIdList.Count <= 1 ? string.Empty : "s"));
        }

        protected List<GridItem> GetItemList(List<int> selectedItemIdList)
        {
            List<GridItem> itemList = new List<GridItem>();
            foreach (var id in selectedItemIdList)
            {
                var item = m_DataSource.GetItemByIndex(m_DataSource.GetItemIndexByItemId(id));
                itemList.Add(item);
            }
            return itemList;
        }

        /// <summary>
        /// 执行拖拽，拖拽到哪个item身上，perform是否要执行拖拽
        /// if perform is true, Event.current.type == DragUpdated, otherwise Event.current.type == DragPerform
        /// </summary>
        public virtual DragAndDropVisualMode DoDrag(int dragToItemId, bool perform)
        {
            if (dragToItemId == int.MinValue) //代表拖拽到Layout身上，不是item身上
            {
                if (perform)
                {

                }
                return DragAndDropVisualMode.None;
            }

            if (perform)
            {

            }
            return DragAndDropVisualMode.None;
        }
    }
}

