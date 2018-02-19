using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public abstract class ViewGroupEditorWindow : EditorWindow
    {
        protected ViewGroupManager m_LayoutGroupMgr;
        protected EditorWindowConfigSource m_WindowConfigSource;

        public virtual EditorWindowConfigSource WindowConfigSource
        {
            get { return m_WindowConfigSource; }
        }

        public virtual Action ConfigSourceChangedAction { get; set; }

        protected virtual void Awake()
        {
            Initialize();
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.Awake();
            }
        }

        protected virtual void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                if (m_WindowConfigSource != null)
                    m_WindowConfigSource.SaveConfigLazily();
            }

            Initialize();
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnGUI(new Rect(0, 0, position.width, position.height));
            }
        }

        protected virtual void OnHierarchyChange()
        {
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnHierarchyChange();
            }
        }

        protected virtual void OnInspectorUpdate()
        {
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnInspectorUpdate();
            }
        }

        protected virtual void OnProjectChange()
        {
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnProjectChange();
            }
        }

        protected virtual void Update()
        {
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.Update();
            }
        }

        protected virtual void OnFocus()
        {
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnFocus();
            }
        }

        protected virtual void OnLostFocus()
        {
            if (m_WindowConfigSource != null)
                m_WindowConfigSource.SaveConfigLazily();

            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnLostFocus();
            }
        }

        protected virtual void OnSelectionChange()
        {
            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnSelectionChange();
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_WindowConfigSource != null)
                m_WindowConfigSource.SaveConfigLazily();

            if (m_LayoutGroupMgr != null)
            {
                m_LayoutGroupMgr.OnDestroy();
            }
        }

        protected virtual void Initialize()
        {
            if (m_LayoutGroupMgr == null)
            {
                m_LayoutGroupMgr = new ViewGroupManager(this);
                InitData();
            }
        }

        protected abstract void InitData();

        public virtual void ChangeWindowConfigSource(EditorWindowConfigSource configSource)
        {
            if (configSource != m_WindowConfigSource)
            {
                if (m_WindowConfigSource != null)
                {
                    m_WindowConfigSource.SaveConfigLazily();
                }

                m_WindowConfigSource = configSource;
                if (ConfigSourceChangedAction != null)
                    ConfigSourceChangedAction();
            }
        }
    }
}