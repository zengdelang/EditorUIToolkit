using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EUTK
{
    public class RenameOverlay
    {
        private static GUIStyle s_DefaultTextFieldStyle;
        private static int s_TextFieldHash = "RenameFieldTextField".GetHashCode();
        private static int s_IndentLevel;
        private string k_RenameOverlayFocusName = "RenameOverlayField";

        [SerializeField] private EventType m_OriginalEventType = EventType.Ignore;
        [SerializeField] private bool m_UserAcceptedRename;
        [SerializeField] private string m_Name;
        [SerializeField] private string m_OriginalName;
        [SerializeField] private Rect m_EditFieldRect;
        [SerializeField] private int m_UserData;
        [SerializeField] private bool m_IsWaitingForDelay;
        [SerializeField] private bool m_IsRenaming;
        [SerializeField] private bool m_IsRenamingFilename;


        private object m_ClientGUIView;

        [NonSerialized] private Rect m_LastScreenPosition;
        [NonSerialized] private bool m_UndoRedoWasPerformed;
        [NonSerialized] private DelayedCallback m_DelayedCallback;

        private int m_TextFieldControlID;

        public string name
        {
            get { return m_Name; }
        }

        public string originalName
        {
            get { return m_OriginalName; }
        }

        public bool userAcceptedRename
        {
            get { return m_UserAcceptedRename; }
        }

        public int userData
        {
            get { return m_UserData; }
        }

        public bool isWaitingForDelay
        {
            get { return m_IsWaitingForDelay; }
        }

        public Rect editFieldRect
        {
            get { return m_EditFieldRect; }
            set { m_EditFieldRect = value; }
        }

        public bool isRenamingFilename
        {
            get { return m_IsRenamingFilename; }
            set { m_IsRenamingFilename = value; }
        }

        /// <summary>
        /// 
        /// <para>
        /// The indent level of the field labels.
        /// </para>
        /// 
        /// </summary>
        public static int indentLevel
        {
            get
            {
                return s_IndentLevel;
            }
            set
            {
                s_IndentLevel = value;
            }
        }

        internal static float indent
        {
            get
            {
                return indentLevel * 15f;
            }
        }

        public bool BeginRename(string name, int userData, float delay)
        {
            if (m_IsRenaming)
            {
                Debug.LogError("BeginRename fail: already renaming");
                return false;
            }

            m_Name = name;
            m_OriginalName = name;
            m_UserData = userData;
            m_UserAcceptedRename = false;
            m_IsWaitingForDelay = delay > 0.0;
            m_IsRenaming = true;
            m_EditFieldRect = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
            m_ClientGUIView = GUIViewWrap.current;
            if (delay > 0.0)
                m_DelayedCallback = new DelayedCallback(BeginRenameInternalCallback, delay);
            else
                BeginRenameInternalCallback();
            return true;
        }

        private void BeginRenameInternalCallback()
        {
            EditorGUIWrap.RecycledEditor.text = m_Name;
            EditorGUIWrap.RecycledEditor.SelectAll();
            RepaintClientView();
            m_IsWaitingForDelay = false;
            Undo.undoRedoPerformed -= UndoRedoWasPerformed;
            Undo.undoRedoPerformed += UndoRedoWasPerformed;
        }

        public void EndRename(bool acceptChanges)
        {
            if (!m_IsRenaming)
                return;

            Undo.undoRedoPerformed -= UndoRedoWasPerformed;

            if (m_DelayedCallback != null)
                m_DelayedCallback.Clear();

            RemoveMessage();
            if (isRenamingFilename)
                m_Name = InternalEditorUtility.RemoveInvalidCharsFromFileName(m_Name, true);
            m_IsRenaming = false;
            m_IsWaitingForDelay = false;
            m_UserAcceptedRename = acceptChanges;
            RepaintClientView();
        }

        private void RepaintClientView()
        {
            if (m_ClientGUIView == null)
                return;
            GUIViewWrap.Repaint(m_ClientGUIView);
        }

        public void Clear()
        {
            m_IsRenaming = false;
            m_UserAcceptedRename = false;
            m_Name = string.Empty;
            m_OriginalName = string.Empty;
            m_EditFieldRect = new Rect();
            m_UserData = 0;
            m_IsWaitingForDelay = false;
            m_OriginalEventType = EventType.Ignore;
            Undo.undoRedoPerformed -= UndoRedoWasPerformed;
        }

        private void UndoRedoWasPerformed()
        {
            m_UndoRedoWasPerformed = true;
        }

        public bool HasKeyboardFocus()
        {
            return GUI.GetNameOfFocusedControl() == k_RenameOverlayFocusName;
        }

        public bool IsRenaming()
        {
            return m_IsRenaming;
        }

        public bool OnEvent()
        {
            if (!m_IsRenaming)
                return true;

            if (!m_IsWaitingForDelay)
            {

                GUI.SetNextControlName(k_RenameOverlayFocusName);
                EditorGUI.FocusTextInControl(k_RenameOverlayFocusName);
                m_TextFieldControlID = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, m_EditFieldRect);
            }

            m_OriginalEventType = Event.current.type;
            if (!m_IsWaitingForDelay ||
                m_OriginalEventType != EventType.MouseDown && m_OriginalEventType != EventType.KeyDown)
                return true;

            EndRename(false);
            return false;
        }

        public bool OnGUI()
        {
            return OnGUI(null);
        }

        public bool OnGUI(GUIStyle textFieldStyle)
        {
            if (m_IsWaitingForDelay)
                return true;

            if (!m_IsRenaming)
                return false;

            if (m_UndoRedoWasPerformed)
            {
                m_UndoRedoWasPerformed = false;
                EndRename(false);
                return false;
            }


            if (m_EditFieldRect.width <= 0.0 || m_EditFieldRect.height <= 0.0 || m_TextFieldControlID == 0)
            {
                HandleUtility.Repaint();
                return true;
            }

            Event current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Escape)
                {
                    current.Use();
                    EndRename(false);
                    return false;
                }
                if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    current.Use();
                    EndRename(true);
                    return false;
                }
            }

            if (m_OriginalEventType == EventType.MouseDown && !m_EditFieldRect.Contains(Event.current.mousePosition))
            {
                EndRename(true);
                return false;
            }

            m_Name = DoTextField(m_Name, textFieldStyle);
            if (current.type == EventType.ScrollWheel)
                current.Use();
            return true;
        }

        private string DoTextField(string text, GUIStyle textFieldStyle)
        {
            if (m_TextFieldControlID == 0)
                Debug.LogError(
                    "RenameOverlay: Ensure to call OnEvent() as early as possible in the OnGUI of the current EditorWindow!");

            if (s_DefaultTextFieldStyle == null)
                s_DefaultTextFieldStyle = "PR TextField";

            if (isRenamingFilename)
                EatInvalidChars();

            GUI.changed = false;
            if (GUIUtility.keyboardControl != m_TextFieldControlID)
                GUIUtility.keyboardControl = m_TextFieldControlID;

            bool changed;
            return EditorGUIWrap.DoTextField(EditorGUIWrap.RecycledEditor, m_TextFieldControlID,
                IndentedRect(m_EditFieldRect), text, textFieldStyle ?? s_DefaultTextFieldStyle, null, out changed,
                false, false, false);
        }

        public static Rect IndentedRect(Rect source)
        {
            float indent = RenameOverlay.indent;
            return new Rect(source.x + indent, source.y, source.width - indent, source.height);
        }

        private void EatInvalidChars()
        {
            if (!isRenamingFilename)
                return;

            Event current = Event.current;
            if (GUIUtility.keyboardControl == m_TextFieldControlID &&
                current.GetTypeForControl(m_TextFieldControlID) == EventType.KeyDown)
            {
                string msg = string.Empty;
                string invalidFilenameChars = EditorUtilityWrap.GetInvalidFilenameChars();
                if (invalidFilenameChars.IndexOf(current.character) > -1)
                    msg = "A file name can't contain any of the following characters:\t" + invalidFilenameChars;
                if (msg != string.Empty)
                {
                    current.Use();
                    ShowMessage(msg);
                }
                else
                    RemoveMessage();
            }

            if (current.type != EventType.Repaint)
                return;

            Rect screenRect = GetScreenRect();
            if (!Mathf.Approximately(m_LastScreenPosition.x, screenRect.x) ||
                !Mathf.Approximately(m_LastScreenPosition.y, screenRect.y))
                RemoveMessage();
            m_LastScreenPosition = screenRect;
        }

        private Rect GetScreenRect()
        {
            return GUIUtilityWrap.GUIToScreenRect(m_EditFieldRect);
        }

        private void ShowMessage(string msg)
        {
            TooltipViewWrap.Show(msg, GetScreenRect());
        }

        private void RemoveMessage()
        {
            TooltipViewWrap.Close();
        }
    }
}