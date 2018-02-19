using System.Collections.Generic;

namespace EUTK
{
    public class GridViewDataSource
    {
        protected List<GridItem> m_ItemList = new List<GridItem>();

        public int Count
        {
            get { return m_ItemList.Count; }
        }

        public List<GridItem> ItemList
        {
            get { return m_ItemList; }
        }

        public int GetItemIdByIndex(int index)
        {
            return m_ItemList[index].Id;
        }

        public GridItem GetItemByIndex(int index)
        {
            return m_ItemList[index];
        }

        public int GetItemIndexByItemId(int itemId)
        {
            for (int i = 0, count = Count; i < count; ++i)
            {
                if (m_ItemList[i].Id == itemId)
                    return i;
            }
            return -1;
        }

        public List<int> GetAllItemId()
        {
            List<int> list = new List<int>();
            for (int i = 0, count = Count; i < count; ++i)
            {
                list.Add(m_ItemList[i].Id);
            }
            return list;
        }
    }
}