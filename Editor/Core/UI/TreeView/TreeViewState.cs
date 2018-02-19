using System;
using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    public class TreeViewState
    {
        [JsonIgnore]
        private EditorWindowConfigSource m_ConfigSource;

        [SerializeField]
        private List<int> m_SelectedIDs = new List<int>();
        [SerializeField]
        private List<int> m_ExpandedIDs = new List<int>();

        [JsonIgnore]
        private RenameOverlay m_RenameOverlay = new RenameOverlay();

        [SerializeField]
        private Vector2 m_ScrollPos;

        [SerializeField]
        private int m_LastClickedID = Int32.MinValue;
        [SerializeField]
        private string m_SearchString;
        [SerializeField]
        private float[] m_ColumnWidths;

        public void SetConfigSource(EditorWindowConfigSource config)
        {
            m_ConfigSource = config;
        }

        public List<int> selectedIDs
        {
            get
            {
                return m_SelectedIDs;
            }
            set
            {
                m_SelectedIDs = value;
                SetDirty();
            }
        }

        public Vector2 scrollPos
        {
            get { return m_ScrollPos; }
            set
            {
                m_ScrollPos = value;
                SetDirty();
            }
        }

        public float scrollPosY
        {
            get { return m_ScrollPos.y; }
            set
            {
                m_ScrollPos.y = value;
                SetDirty();
            }
        }

        public int lastClickedID
        {
            get
            {
                return m_LastClickedID;
            }
            set
            {
                m_LastClickedID = value;
                SetDirty();
            }
        }

        public List<int> expandedIDs
        {
            get
            {
                return m_ExpandedIDs;
            }
            set
            {
                m_ExpandedIDs = value;
                SetDirty();
            }
        }

        public RenameOverlay renameOverlay
        {
            get
            {
                return m_RenameOverlay;
            }
            set
            {
                m_RenameOverlay = value;
                SetDirty();
            }
        }

        public float[] columnWidths
        {
            get
            {
                return m_ColumnWidths;
            }
            set
            {
                m_ColumnWidths = value;
                SetDirty();
            }
        }

        public string searchString
        {
            get
            {
                return m_SearchString;
            }
            set
            {
                m_SearchString = value;
                SetDirty();
            }
        }

        public void OnAwake()
        {
            m_RenameOverlay.Clear();
        }

        protected virtual void SetDirty()
        {
            if (m_ConfigSource != null)
                m_ConfigSource.SetConfigDirty();
        }
    }
}