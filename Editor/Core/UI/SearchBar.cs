using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class SearchBar : View
    {
        private int m_HashForSearchField;

        private bool m_FocusSearchField;
        private string m_SearchFieldText = string.Empty;

        public EditorWindowConfigSource m_WindowConfigSource;
        protected string m_SearchTextConfigName;

        protected float m_RightMargin;
        protected bool m_ShowSearchField = true;
        protected bool m_EnableShortcutKeyCtrlF = true;

        public float RightMargin
        {
            get { return m_RightMargin; }
            set { m_RightMargin = value; }
        }

        public bool ShowSearchField
        {
            get { return m_ShowSearchField; }
            set { m_ShowSearchField = value; }
        }

        public string SearchTextConfigName
        {
            get { return m_SearchTextConfigName; }
            set { m_SearchTextConfigName = value; }
        }

        public bool EnableShortcutKeyCtrlF
        {
            get { return m_EnableShortcutKeyCtrlF; }
            set { m_EnableShortcutKeyCtrlF = value; }
        }

        public Action<string> OnTextChangedAction
        {
            get; set;
        }

        public Action UpOrDownArrowPressedAction
        {
            get; set;
        }

        public string SearchText
        {
            get { return m_SearchFieldText; }
            protected set
            {
                if (string.IsNullOrEmpty(value))
                    value = string.Empty;

                if (m_SearchFieldText != value)
                {
                    m_SearchFieldText = value;
                    if (m_WindowConfigSource != null && !string.IsNullOrEmpty(m_SearchTextConfigName))
                    {
                        m_WindowConfigSource.SetValue(m_SearchTextConfigName, m_SearchFieldText);
                        m_WindowConfigSource.SetConfigDirty();
                    }

                    if (OnTextChangedAction != null)
                    {
                        OnTextChangedAction(m_SearchFieldText);
                    }
                }
            }
        }

        public SearchBar(ViewGroupManager owner) : base(owner)
        {
            m_HashForSearchField = GUID.Generate().ToString().GetHashCode();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            GUILayout.BeginArea(new Rect(0.0f, 0.0f, rect.width, EditorStyles.toolbar.fixedHeight));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.FlexibleSpace();
            if (m_ShowSearchField)
            {
                SearchField();
                GUILayout.Space(m_RightMargin);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            HandleCommandEvents();
        }

        private void SearchField()
        {
            float maxWidth = (EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5.0f) * 1.5f;
            GUILayoutOption[] guiLayoutOptionArray = new GUILayoutOption[2];
            guiLayoutOptionArray[0] = GUILayout.MinWidth(65f);
            guiLayoutOptionArray[1] = GUILayout.MaxWidth(300f);
            Rect rect = GUILayoutUtility.GetRect(0, maxWidth, 16, 16, EditorStylesWrap.toolbarSearchField, guiLayoutOptionArray);
            int controlId = GUIUtility.GetControlID(m_HashForSearchField, FocusType.Passive, rect);
            if (m_FocusSearchField)
            {
                GUIUtility.keyboardControl = controlId;
                EditorGUIUtility.editingTextField = true;
                if (Event.current.type == EventType.Repaint)
                    m_FocusSearchField = false;
            }

            Event current = Event.current;
            if (current.type == EventType.KeyDown &&
                (current.keyCode == KeyCode.DownArrow || current.keyCode == KeyCode.UpArrow) &&
                GUIUtility.keyboardControl == controlId)
            {
                if (UpOrDownArrowPressedAction != null)
                    UpOrDownArrowPressedAction();
                current.Use();
            }

            string str = EditorGUIWrap.ToolbarSearchField(controlId, rect, m_SearchFieldText, false);
            if (str == m_SearchFieldText && !m_FocusSearchField)
                return;

            SearchText = str;
            Repaint();
        }

        private void HandleCommandEvents()
        {
            EventType type = Event.current.type;
            switch (type)
            {
                case EventType.ExecuteCommand:
                case EventType.ValidateCommand:
                    bool flag = type == EventType.ExecuteCommand;
                    if (m_EnableShortcutKeyCtrlF && Event.current.commandName == "Find")   //Ctrl + F
                    {
                        if (flag)
                            m_FocusSearchField = true;
                        Event.current.Use();
                    }
                    break;
            }
        }

        public void ClearSearchText()
        {
            SearchText = string.Empty;
        }

        public void LoadConfig(string configName, EditorWindowConfigSource configSource)
        {
            m_SearchTextConfigName = configName;
            m_WindowConfigSource = configSource;

            if (m_WindowConfigSource != null && !string.IsNullOrEmpty(m_SearchTextConfigName))
            {
                var text = m_WindowConfigSource.GetValue<string>(m_SearchTextConfigName);
                if (!string.IsNullOrEmpty(text))
                {
                    SearchText = text;
                }
            }
        }
    }
}

