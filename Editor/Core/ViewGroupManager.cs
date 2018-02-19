using System.Collections.Generic;
using UnityEngine;

namespace EUTK
{
    public class ViewGroupManager
    {
        protected List<ViewGroup> m_ViewGroupList = new List<ViewGroup>();

        public ViewGroupEditorWindow WindowOwner
        {
            get; protected set;
        }

        public ViewGroupManager(ViewGroupEditorWindow owner)
        {
            WindowOwner = owner;
        }

        public virtual void AddViewGroup(ViewGroup group)
        {
            m_ViewGroupList.Add(group);
        }

        public virtual bool RemoveViewGroup(ViewGroup viewGroup)
        {
            return m_ViewGroupList.Remove(viewGroup);
        }

        public virtual bool RemoveViewGroup(int index)
        {
            if (index >= 0 && index < m_ViewGroupList.Count)
            {
                m_ViewGroupList.RemoveAt(index);
                return true;
            }
            return false;
        }

        public virtual ViewGroup GetViewGroup(int index)
        {
            if (index >= 0 && index < m_ViewGroupList.Count)
            {
                return m_ViewGroupList[index];
            }
            return null;
        }

        public virtual void Awake()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.Awake();
            }
        }

        public virtual void OnGUI(Rect rect)
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnGUI(rect);
            }
        }

        public virtual void OnHierarchyChange()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnHierarchyChange();
            }
        }

        public virtual void OnInspectorUpdate()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnInspectorUpdate();
            }
        }

        public virtual void OnProjectChange()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnProjectChange();
            }
        }

        public virtual void Update()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.Update();
            }
        }

        public virtual void OnFocus()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnFocus();
            }
        }

        public virtual void OnLostFocus()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnLostFocus();
            }
        }

        public virtual void OnSelectionChange()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnSelectionChange();
            }
        }

        public virtual void OnDestroy()
        {
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnDestroy();
            }
        }
    }
}