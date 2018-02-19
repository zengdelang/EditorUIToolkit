using System;
using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    public class TreeViewItem : IComparable<TreeViewItem>
    {
        [JsonIgnore] protected EditorWindowConfigSource m_ConfigSource;

        [SerializeField] protected int m_ID;
        [SerializeField] protected int m_Depth;
        [SerializeField] protected string m_DisplayName;
        [SerializeField] protected List<TreeViewItem> m_Children;

        [JsonIgnore] protected TreeViewItem m_Parent;
        [JsonIgnore] protected Texture2D m_Icon;
        [JsonIgnore] protected object m_UserData;

        public TreeViewItem()
        {

        }

        public virtual int id
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
                SetDirty();
            }
        }

        public virtual string displayName
        {
            get
            {
                return m_DisplayName;
            }
            set
            {
                m_DisplayName = value;
                SetDirty();
            }
        }

        public virtual int depth
        {
            get
            {
                return m_Depth;
            }
            set
            {
                m_Depth = value;
                SetDirty();
            }
        }

        public virtual bool hasChildren
        {
            get
            {
                if (m_Children != null)
                    return m_Children.Count > 0;
                return false;
            }
        }

        public virtual List<TreeViewItem> children
        {
            get
            {
                return m_Children;
            }
            set
            {
                m_Children = value;
                SetDirty();
            }
        }

        public virtual TreeViewItem parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                m_Parent = value;
                SetDirty();
            }
        }

        public virtual Texture2D icon
        {
            get
            {
                return m_Icon;
            }
            set
            {
                m_Icon = value;
            }
        }

        public virtual object userData
        {
            get
            {
                return m_UserData;
            }
            set
            {
                m_UserData = value;
            }
        }

        public TreeViewItem(int id, int depth, TreeViewItem parent, string displayName)
        {
            m_Depth = depth;
            m_Parent = parent;
            m_ID = id;
            m_DisplayName = displayName;
        }

        public void AddChild(TreeViewItem child)
        {
            if (m_Children == null)
                m_Children = new List<TreeViewItem>();
            m_Children.Add(child);
            if (child == null)
                return;
            child.depth = this.depth + 1;
            ChangeChildrenDepth(child);
            child.parent = this;
        }

        public void AddChildAtIndex(TreeViewItem child, int index)
        {
            if (m_Children == null)
                m_Children = new List<TreeViewItem>();
            m_Children.Insert(index, child);
            if (child == null)
                return;
            child.depth = this.depth + 1;
            ChangeChildrenDepth(child);
            child.parent = this;
        }

        private void ChangeChildrenDepth(TreeViewItem item)
        {
            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    child.depth = item.depth + 1;
                    ChangeChildrenDepth(child);
                }
            }
        }

        public void SetConfigSource(EditorWindowConfigSource source)
        {
            m_ConfigSource = source;
        }

        protected void SetDirty()
        {
            if (m_ConfigSource != null)
                m_ConfigSource.SetConfigDirty();
        }

        public virtual int CompareTo(TreeViewItem other)
        {
            return displayName.CompareTo(other.displayName);
        }

        public override string ToString()
        {
            return string.Format("Item: '{0}' ({1}), has {2} children, depth {3}, parent id {4}", displayName, id, !hasChildren ? 0 : children.Count, depth, parent == null ? -1 : parent.id);
        }
    }

}