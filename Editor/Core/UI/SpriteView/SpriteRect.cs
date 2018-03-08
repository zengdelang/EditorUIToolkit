using System;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class SpriteRect : TreeViewItem
    {
        [SerializeField]
        [JsonMember]
        protected string m_Name;

        [SerializeField]
        [JsonMember]
        protected string m_OriginalName;

        [SerializeField]
        [JsonMember]
        protected Vector2 m_Pivot;

        [SerializeField]
        [JsonMember]
        protected SpriteAlignment m_Alignment;

        [SerializeField]
        [JsonMember]
        protected Vector4 m_Border;

        [SerializeField]
        [JsonMember]
        protected Rect m_Rect;

        public string name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        public string originalName
        {
            get
            {
                if (m_OriginalName == null)
                    m_OriginalName = name;
                return m_OriginalName;
            }
            set
            {
                m_OriginalName = value;
            }
        }

        public Vector2 pivot
        {
            get
            {
                return m_Pivot;
            }
            set
            {
                m_Pivot = value;
            }
        }

        public SpriteAlignment alignment
        {
            get
            {
                return m_Alignment;
            }
            set
            {
                m_Alignment = value;
            }
        }

        public Vector4 border
        {
            get
            {
                return m_Border;
            }
            set
            {
                m_Border = value;
            }
        }

        public Rect rect
        {
            get
            {
                return m_Rect;
            }
            set
            {
                m_Rect = value;
            }
        }
    }
}