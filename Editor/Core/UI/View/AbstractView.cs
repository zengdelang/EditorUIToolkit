using UnityEngine;

namespace EUTK
{
    public abstract class AbstractView
    {
        protected bool m_IsActive = true;

        public bool Active
        {
            get { return m_IsActive; }
            set
            {
                if (value != m_IsActive)
                {
                    m_IsActive = value;
                    OnActive(m_IsActive);
                }
            }
        }

        public ViewGroupManager Owner { get; protected set; }

        public AbstractView(ViewGroupManager owner)
        {
            Owner = owner;
        }

        public virtual void Awake()
        {

        }

        public virtual void OnGUI(Rect rect)
        {

        }

        public virtual void OnHierarchyChange()
        {

        }

        public virtual void OnInspectorUpdate()
        {

        }

        public virtual void OnProjectChange()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void OnFocus()
        {

        }

        public virtual void OnLostFocus()
        {

        }

        public virtual void OnSelectionChange()
        {

        }

        public virtual void OnDestroy()
        {

        }

        protected virtual void OnActive(bool active)
        {

        }

        public void Repaint()
        {
            Owner.WindowOwner.Repaint();
        }
    }
}