using System.Collections.Generic;
using UnityEngine;

namespace EUTK
{
    public class ViewGroup : AbstractView
    {
        protected List<View> m_ViewList = new List<View>();
        protected List<ViewGroup> m_ViewGroupList = new List<ViewGroup>();

        public ViewGroup(ViewGroupManager owner) : base(owner)
        {

        }

        public virtual void AddViewGroup(ViewGroup viewGroup)
        {
            m_ViewGroupList.Add(viewGroup);
        }

        public virtual void AddView(View view)
        {
            m_ViewList.Add(view);
        }

        public virtual bool RemoveViewGroup(ViewGroup viewGroup)
        {
            return m_ViewGroupList.Remove(viewGroup);
        }

        public virtual bool RemoveView(View view)
        {
            return m_ViewList.Remove(view);
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

        public virtual bool RemoveView(int index)
        {
            if (index >= 0 && index < m_ViewList.Count)
            {
                m_ViewList.RemoveAt(index);
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

        public virtual View GetView(int index)
        {
            if (index >= 0 && index < m_ViewList.Count)
            {
                return m_ViewList[index];
            }
            return null;
        }

        public override void Awake()
        {
            base.Awake();

            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.Awake();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.Awake();
            }
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnGUI(rect);
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnGUI(rect);
            }
        }

        public override void OnHierarchyChange()
        {
            base.OnHierarchyChange();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnHierarchyChange();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnHierarchyChange();
            }
        }

        public override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnInspectorUpdate();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnInspectorUpdate();
            }
        }

        public override void OnProjectChange()
        {
            base.OnProjectChange();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnProjectChange();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnProjectChange();
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.Update();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.Update();
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnFocus();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnFocus();
            }
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnLostFocus();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnLostFocus();
            }
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnSelectionChange();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnSelectionChange();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var group in m_ViewGroupList)
            {
                if (group.Active)
                    group.OnDestroy();
            }

            foreach (var layout in m_ViewList)
            {
                if (layout.Active)
                    layout.OnDestroy();
            }
        }
    }
}