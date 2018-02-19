using UnityEditor;

namespace EUTK
{
    public class FolderGridViewHandler : GridViewHandler
    {
        public FolderTreeViewDragging TreeViewDragging;

        public FolderGridViewHandler(GridViewDataSource dataSource) : base(dataSource)
        {

        }

        /// <summary>
        /// 执行拖拽，拖拽到哪个item身上，perform是否要执行拖拽
        /// if perform is true, Event.current.type == DragUpdated, otherwise Event.current.type == DragPerform
        /// </summary>
        public override DragAndDropVisualMode DoDrag(int dragToItemId, bool perform)
        {
            if (dragToItemId == int.MinValue) //代表拖拽到Layout身上，不是item身上
            {
                return DragAndDropVisualMode.None;
            }

            var parentItem = m_DataSource.GetItemByIndex(m_DataSource.GetItemIndexByItemId(dragToItemId)) as FolderGridItem;
            if (parentItem.IsFolder)
            {
                if (TreeViewDragging != null)
                    return TreeViewDragging.DoDragForGridView(TreeViewDragging.GetItem(parentItem.Id), null, perform);
            }
            return DragAndDropVisualMode.None;
        }
    }
}

