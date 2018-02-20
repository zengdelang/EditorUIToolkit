using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class HorizontalSplitLine
    {
        public static readonly string ConfigKey = "splitLineX";

        protected static Vector2 s_MouseDeltaReaderLastPos;

        protected EditorWindowConfigSource m_ConfigSource;

        protected float m_MinXPos;
        protected float m_PositionX;
        protected float m_MinRightDelta = 124;

        protected float m_LastPositionX;

        public Action PositionChangedAction { get; set; }

        public float PositionX
        {
            get { return m_PositionX; }
            protected set
            {
                m_PositionX = value;
                if (m_ConfigSource != null)
                {
                    if (m_ConfigSource.FindProperty(ConfigKey) != null)
                        m_ConfigSource.SetValue(ConfigKey, m_PositionX);
                    m_ConfigSource.SetConfigDirty();
                }
            }
        }

        public EditorWindowConfigSource ConfigSource
        {
            set
            {
                m_ConfigSource = value;
                if (m_ConfigSource != null)
                {
                    if (m_ConfigSource.FindProperty(ConfigKey) != null)
                    {
                        var x = m_ConfigSource.GetValue<float>(ConfigKey);
                        if (x >= m_MinXPos)
                            m_PositionX = x;
                    }
                }
            }
        }

        public HorizontalSplitLine(float posX, float minXPos)
        {
            PositionX = posX;
            m_MinXPos = minXPos;
        }

        public void OnGUI(float offsetX, float marginY, float height)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color color = GUI.color;

                GUI.color *= !EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f, 1.333f) : new Color(0.12f, 0.12f, 0.12f, 1.333f);
                GUI.DrawTexture(new Rect(PositionX - offsetX, marginY, 1f, height), EditorGUIUtility.whiteTexture);
                GUI.color = color;
            }
        }

        public void ResizeHandling(float marginTop, float width, float height, float minOwnerWidth = 0, float lineWidth = 5f)
        {
            Rect position = new Rect(PositionX, marginTop, lineWidth, height);
            if (Event.current.type == EventType.Repaint)
                EditorGUIUtility.AddCursorRect(position, MouseCursor.SplitResizeLeftRight);

            float newPos = 0.0f;
            float mouseDelta = MouseDeltaReader(position, true).x;
            if (!Mathf.Approximately(mouseDelta, 0f))
            {
                PositionX += mouseDelta;
                newPos = Mathf.Clamp(PositionX, m_MinXPos, width - m_MinXPos);
            }

            if (width - m_MinRightDelta < PositionX)
                newPos = width - m_MinRightDelta;

            if (newPos > 0.0)
                PositionX = newPos;

            if (PositionX < m_MinXPos)
                PositionX = m_MinXPos;

            if (Mathf.Approximately(PositionX, m_LastPositionX))
            {
                if (PositionChangedAction != null)
                    PositionChangedAction();
            }
            m_LastPositionX = PositionX;
        }

        internal static Vector2 MouseDeltaReader(Rect position, bool activated)
        {
            int controlId = GUIUtility.GetControlID("MouseDeltaReader".GetHashCode(), FocusType.Passive, position);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (activated && GUIUtility.hotControl == 0 && (position.Contains(current.mousePosition) && current.button == 0))
                    {
                        GUIUtility.hotControl = controlId;
                        GUIUtility.keyboardControl = 0;
                        s_MouseDeltaReaderLastPos = GUIClipWrap.Unclip(current.mousePosition);
                        current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId && current.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        Vector2 vector2_1 = GUIClipWrap.Unclip(current.mousePosition);
                        Vector2 vector2_2 = vector2_1 - s_MouseDeltaReaderLastPos;
                        s_MouseDeltaReaderLastPos = vector2_1;
                        current.Use();
                        return vector2_2;
                    }
                    break;
            }
            return Vector2.zero;
        }
    }

}