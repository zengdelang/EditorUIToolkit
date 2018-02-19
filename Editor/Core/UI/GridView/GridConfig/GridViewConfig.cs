using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonOptIn]
    public class GridViewConfig
    {
        [JsonIgnore]
        private EditorWindowConfigSource m_ConfigSource;

        [SerializeField]
        private bool m_AllowDragging = true;

        [SerializeField]
        private bool m_AllowRenaming = true;

        [SerializeField]
        private bool m_AllowMultiSelect = true;

        [SerializeField]
        private bool m_AllowDeselection = true;

        [SerializeField]
        private bool m_AllowFocusRendering = true;

        [SerializeField]
        private bool m_AllowFindNextShortcut = true;

        [SerializeField]
        private List<int> m_SelectedItemIdList = new List<int>();

        [SerializeField]
        public List<int> m_ExpandedItemIdList = new List<int>();

        [SerializeField]
        private int m_LastClickedItemId;

        [SerializeField]
        public Vector2 m_ScrollPosition;

        [SerializeField]
        private int m_GridSize = 64;

        public int GridSize
        {
            get { return m_GridSize; }
            set
            {
                if (m_GridSize != value)
                {
                    m_GridSize = value;
                    SetDirty();
                }
            }
        }

        public bool AllowDragging
        {
            get { return m_AllowDragging; }
            set
            {
                if (m_AllowDragging != value)
                {
                    m_AllowDragging = value;
                    SetDirty();
                }
            }
        }

        public bool AllowRenaming
        {
            get { return m_AllowRenaming; }
            set
            {
                if (m_AllowRenaming != value)
                {
                    m_AllowRenaming = value;
                    SetDirty();
                }
            }
        }

        public bool AllowMultiSelect
        {
            get { return m_AllowMultiSelect; }
            set
            {
                if (m_AllowMultiSelect != value)
                {
                    m_AllowMultiSelect = value;
                    SetDirty();
                }
            }
        }

        public bool AllowDeselection
        {
            get { return m_AllowDeselection; }
            set
            {
                if (m_AllowDeselection != value)
                {
                    m_AllowDeselection = value;
                    SetDirty();
                }
            }
        }

        public bool AllowFocusRendering
        {
            get { return m_AllowFocusRendering; }
            set
            {
                if (m_AllowFocusRendering != value)
                {
                    m_AllowFocusRendering = value;
                    SetDirty();
                }
            }
        }

        public bool AllowFindNextShortcut
        {
            get { return m_AllowFindNextShortcut; }
            set
            {
                if (m_AllowFindNextShortcut != value)
                {
                    m_AllowFindNextShortcut = value;
                    SetDirty();
                }
            }
        }


        public Vector2 ScrollPosition
        {
            get { return m_ScrollPosition; }
            set
            {
                if (m_ScrollPosition != value)
                {
                    m_ScrollPosition = value;
                    SetDirty();
                }
            }
        }

        public float ScrollPositionY
        {
            get { return m_ScrollPosition.y; }
            set
            {
                if (m_ScrollPosition.y != value)
                {
                    m_ScrollPosition.y = value;
                    SetDirty();
                }
            }
        }

        public List<int> SelectedItemIdList
        {
            get { return m_SelectedItemIdList; }
            set
            {
                m_SelectedItemIdList = value;
                SetDirty();
            }
        }

        public List<int> ExpandedItemIdList
        {
            get { return m_ExpandedItemIdList; }
            set
            {
                m_ExpandedItemIdList = value;
                SetDirty();
            }
        }

        public int LastClickedItemId
        {
            get { return m_LastClickedItemId; }
            set
            {
                if (m_LastClickedItemId != value)
                {
                    m_LastClickedItemId = value;
                    SetDirty();
                }
            }
        }

        public void SetConfigSource(EditorWindowConfigSource configSource)
        {
            m_ConfigSource = configSource;
        }

        public void SetDirty()
        {
            if (m_ConfigSource != null)
            {
                m_ConfigSource.SetConfigDirty();
            }
        }
    }
}