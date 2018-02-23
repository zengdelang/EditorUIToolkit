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
        [JsonIgnore] [NonSerialized] private EditorWindowConfigSource m_ConfigSource;

        [JsonMember] [SerializeField] private List<int> m_SelectedIDs = new List<int>();
        [JsonMember] [SerializeField] private List<int> m_ExpandedIDs = new List<int>();

        [JsonIgnore] [NonSerialized] private RenameOverlay m_RenameOverlay = new RenameOverlay();

        [JsonMember] [SerializeField] private Vector2 m_ScrollPos;
        [JsonMember] [SerializeField] private int m_LastClickedID = Int32.MinValue;
        [JsonMember] [SerializeField] private string m_SearchString;
        [JsonMember] [SerializeField] private float[] m_ColumnWidths;

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
                if (m_ScrollPos != value)
                {
                    m_ScrollPos = value;
                    SetDirty();
                }               
            }
        }

        public float scrollPosY
        {
            get { return m_ScrollPos.y; }
            set
            {
                if (m_ScrollPos.y != value)
                {
                    m_ScrollPos.y = value;
                    SetDirty();
                }              
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