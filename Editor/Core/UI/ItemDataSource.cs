using System;
using System.Collections.Generic;
using UnityEngine;

namespace EUTK
{
    [Serializable]
    public class ItemDataSource
    {
        protected EditorWindowConfigSource m_ConfigSource;
        [SerializeField]
        protected List<GridItem> m_ItemList = new List<GridItem>();
        [SerializeField]
        protected int m_MaxItemId;

        public int Count
        {
            get { return m_ItemList.Count; }
        }

        public void SetConfigSource(EditorWindowConfigSource configSource)
        {
            m_ConfigSource = configSource;
            if (m_ItemList == null)
            {
                m_ItemList = new List<GridItem>();
            }
            else
            {
                foreach (var item in m_ItemList)
                {
                    item.SetConfigSource(configSource);
                }
            }
        }

        protected void SetDirty()
        {
            if (m_ConfigSource != null)
            {
                m_ConfigSource.SetConfigDirty();
            }
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

        public virtual void AddItem(GridItem item)
        {
            ++m_MaxItemId;
            item.Id = m_MaxItemId;
            item.SetConfigSource(m_ConfigSource);
            m_ItemList.Add(item);
            SetDirty();
        }

        public virtual void AddItemRange(List<GridItem> itemList)
        {
            foreach (var item in itemList)
            {
                ++m_MaxItemId;
                item.Id = m_MaxItemId;
                item.SetConfigSource(m_ConfigSource);
            }

            m_ItemList.AddRange(itemList);
            SetDirty();
        }

        public virtual void AddItemRange(int startIndex, List<GridItem> itemList, bool autoId = true)
        {
            foreach (var item in itemList)
            {
                ++m_MaxItemId;
                item.Id = m_MaxItemId;
                item.SetConfigSource(m_ConfigSource);
            }

            m_ItemList.InsertRange(startIndex, itemList);
            SetDirty();
        }

        public virtual bool RemoveItem(GridItem item)
        {
            if (m_ItemList != null)
            {
                m_ItemList.Remove(item);
                SetDirty();
                return true;
            }
            return false;
        }

        public virtual bool RemoveItemRange(int index, int count)
        {
            if (m_ItemList != null)
            {
                m_ItemList.RemoveRange(index, count);
                SetDirty();
                return true;
            }
            return false;
        }

        public virtual GridItem GetItem(int index)
        {
            if (m_ItemList != null)
            {
                return m_ItemList[index];
            }
            return null;
        }

        public virtual List<GridItem> GetItemRange(int index, int count)
        {
            if (m_ItemList != null)
            {
                return m_ItemList.GetRange(index, count);
            }
            return null;
        }

        public void ClearAllItem()
        {
            m_MaxItemId = 0;
            m_ItemList.Clear();
            SetDirty();
        }

        public void AddChildItem(GridItem parent, GridItem child)
        {
            ++m_MaxItemId;
            child.Id = m_MaxItemId;
            child.SetConfigSource(m_ConfigSource);
            parent.AddChildItem(child);
            SetDirty();
        }
    }
}