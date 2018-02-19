using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class BottomBar : View
    {
        protected int m_Value;
        protected int m_MinValue;
        protected int m_MaxValue;

        private List<GUIContent> m_SelectedPathSplitted = new List<GUIContent>();

        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public int MinValue
        {
            get { return m_MinValue; }
            set { m_MinValue = value; }
        }

        public int MaxValue
        {
            get { return m_MaxValue; }
            set { m_MaxValue = value; }
        }

        public List<GUIContent> SelectedPathSplitted
        {
            get { return m_SelectedPathSplitted; }
        }

        public Action<int> OnValueChangedAction
        {
            get; set;
        }

        public BottomBar(ViewGroupManager owner) : base(owner)
        {

        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            Rect position = rect;
            GUI.Label(position, GUIContent.none, "ProjectBrowserBottomBarBg");

            var sliderRect = new Rect(position.x + position.width - 71f, position.y + position.height - 17f, 55f, 17f);
            EditorGUI.BeginChangeCheck();
            int value = (int)GUI.HorizontalSlider(sliderRect, m_Value, m_MinValue, m_MaxValue);

            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            rect.width -= 4f;
            rect.x += 2f;
            rect.height = 17f;
            for (int index = m_SelectedPathSplitted.Count - 1; index >= 0; --index)
            {
                if (index == 0)
                    rect.width = rect.width - 55.0f - 14.0f;
                GUI.Label(rect, m_SelectedPathSplitted[index], "Label");
                rect.y += 17f;
            }
            EditorGUIUtility.SetIconSize(new Vector2(0.0f, 0.0f));

            if (!EditorGUI.EndChangeCheck())
                return;

            if (value != m_Value)
            {
                m_Value = value;
                if (OnValueChangedAction != null)
                {
                    OnValueChangedAction(value);
                }
            }
        }
    }
}
