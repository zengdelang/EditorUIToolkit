using System;
using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    public abstract class GridItem
    {
        [NonSerialized]
        protected EditorWindowConfigSource m_EditorWindowConfigSource;

        [SerializeField]
        protected int m_Id;

        [SerializeField]
        protected string m_DisplayName;

        [SerializeField]
        protected bool m_IsChildItem;

        [SerializeField]
        protected List<GridItem> m_ChildItems;

        public virtual int Id
        {
            get { return m_Id; }
            set
            {
                if (m_Id != value)
                {
                    m_Id = value;
                    SetDirty();
                }
            }
        }

        public virtual string DisplayName
        {
            get { return m_DisplayName; }
            set
            {
                if (m_DisplayName != value)
                {
                    m_DisplayName = value;
                    SetDirty();
                }
            }
        }

        public abstract Texture Texture { get; }

        public virtual bool IsChildItem
        {
            get { return m_IsChildItem; }
            set
            {
                if (m_IsChildItem != value)
                {
                    m_IsChildItem = value;
                    SetDirty();
                }
            }
        }

        public virtual bool HasChildren
        {
            get
            {
                return ChildItemCount > 0;
            }
        }

        public virtual int ChildItemCount
        {
            get
            {
                if (m_ChildItems == null)
                    return 0;
                return m_ChildItems.Count;
            }
        }

        protected virtual void SetDirty()
        {
            if (m_EditorWindowConfigSource != null)
                m_EditorWindowConfigSource.SetConfigDirty();
        }

        public virtual void SetConfigSource(EditorWindowConfigSource configSource)
        {
            m_EditorWindowConfigSource = configSource;
        }

        public virtual void AddChildItem(GridItem item)
        {
            if (m_ChildItems == null)
            {
                m_ChildItems = new List<GridItem>();
            }

            item.IsChildItem = true;
            item.SetConfigSource(m_EditorWindowConfigSource);
            m_ChildItems.Add(item);
            SetDirty();
        }

        public virtual void AddChildItemRange(List<GridItem> itemList)
        {
            if (m_ChildItems == null)
            {
                m_ChildItems = new List<GridItem>();
            }

            foreach (var item in itemList)
            {
                item.IsChildItem = true;
                item.SetConfigSource(m_EditorWindowConfigSource);
            }

            m_ChildItems.AddRange(itemList);
            SetDirty();
        }

        public virtual bool RemoveChildItem(GridItem item)
        {
            if (m_ChildItems != null)
            {
                m_ChildItems.Remove(item);
                SetDirty();
                return true;
            }
            return false;
        }

        public virtual bool RemoveChildItemRange(int index, int count)
        {
            if (m_ChildItems != null)
            {
                m_ChildItems.RemoveRange(index, count);
                SetDirty();
                return true;
            }
            return false;
        }

        public virtual GridItem GetChildItem(int index)
        {
            if (m_ChildItems != null)
            {
                return m_ChildItems[index];
            }
            return null;
        }

        public virtual List<GridItem> GetChildItemRange(int index, int count)
        {
            if (m_ChildItems != null)
            {
                return m_ChildItems.GetRange(index, count);
            }
            return null;
        }

        public virtual List<GridItem> GetAllChildItem()
        {
            if (m_ChildItems != null)
            {
                return m_ChildItems.GetRange(0, ChildItemCount);
            }
            return null;
        }

        public void ClearAllChildItem()
        {
            if (m_ChildItems != null)
            {
                m_ChildItems.Clear();
                SetDirty();
            }
        }
    }

}