using UnityEngine;

namespace EUTK
{
    public class PingData
    {
        public float m_TimeStart = -1f;
        public float m_ZoomTime = 0.2f;
        public float m_WaitTime = 2.5f;
        public float m_FadeOutTime = 1.5f;
        public float m_PeakScale = 1.75f;
        public float m_AvailableWidth = 100f;
        public System.Action<Rect> m_ContentDraw;
        public Rect m_ContentRect;
        public GUIStyle m_PingStyle;

        public bool isPinging
        {
            get
            {
                return m_TimeStart > -1f;
            }
        }

        public void HandlePing()
        {
            if (!isPinging)
            {
                return;
            }

            float num1 = m_ZoomTime + m_WaitTime + m_FadeOutTime;
            float num2 = Time.realtimeSinceStartup - m_TimeStart;
            if (num2 >= 0f && num2 < num1)
            {
                Color color = GUI.color;
                Matrix4x4 matrix1 = GUI.matrix;
                if (num2 < m_ZoomTime)
                {
                    float num3 = m_ZoomTime / 2f;
                    float num4 = (float)((m_PeakScale - 1.0) * ((m_ZoomTime - Mathf.Abs(num3 - num2)) / num3 - 1.0) + 1.0);
                    Matrix4x4 matrix2 = GUI.matrix;
                    Vector2 vector2 = GUIClipWrap.Unclip(m_ContentRect.xMax >= m_AvailableWidth ? new Vector2(m_AvailableWidth, m_ContentRect.center.y) : m_ContentRect.center);
                    GUI.matrix = Matrix4x4.TRS(vector2, Quaternion.identity, new Vector3(num4, num4, 1f)) * Matrix4x4.TRS(-vector2, Quaternion.identity, Vector3.one) * matrix2;
                }
                else if (num2 > m_ZoomTime + m_WaitTime)
                {
                    float num3 = (num1 - num2) / m_FadeOutTime;
                    GUI.color = new Color(color.r, color.g, color.b, color.a * num3);
                }
                if (m_ContentDraw != null && Event.current.type == EventType.Repaint)
                {
                    Rect position = m_ContentRect;
                    position.x -= m_PingStyle.padding.left;
                    position.y -= m_PingStyle.padding.top;
                    m_PingStyle.Draw(position, GUIContent.none, false, false, false, false);
                    m_ContentDraw(m_ContentRect);
                }
                GUI.matrix = matrix1;
                GUI.color = color;
            }
            else
                m_TimeStart = -1f;
        }
    }
}
