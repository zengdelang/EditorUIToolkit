using System;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace EUTK
{
    public class EditorGUIWrap
    {
        public delegate string ToolbarSearchFieldDelegate(int id, Rect position, string text, bool showWithPopupArrow);
        protected static ToolbarSearchFieldDelegate s_ToolbarSearchFieldDelegate;

        public delegate string SearchFieldDelegate(Rect position, string text);
        protected static SearchFieldDelegate s_SearchFieldDelegate;

        private static MethodInfo s_DoTextFieldMF;
        private static TextEditor s_RecycledEditor;

        public static TextEditor RecycledEditor
        {
            get
            {
                if (s_RecycledEditor != null)
                    return s_RecycledEditor;

                Type type = typeof(EditorGUI);
                var fi = type.GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);
                s_RecycledEditor = fi.GetValue(null) as TextEditor;
                return s_RecycledEditor;
            }
        }

        public static string DoTextField(TextEditor textEditor, int id, Rect position, string text, GUIStyle style,
            string allowedletters, out bool changed, bool reset, bool multiline, bool passwordField)
        {
            changed = false;
            if (s_DoTextFieldMF != null)
            {
                var param1 = new object[] { textEditor, id, position, text, style, allowedletters, changed, reset, multiline, passwordField };
                var result1 = s_DoTextFieldMF.Invoke(null, param1) as string;
                changed = true;
                return result1;
            }

            Type type = typeof(EditorGUI);
            var mf = type.GetMethod("DoTextField", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[]
            {
                type.Assembly.GetType("UnityEditor.EditorGUI+RecycledTextEditor") ,
                typeof(int),typeof(Rect),typeof(string),typeof(GUIStyle),typeof(string),typeof(bool).MakeByRefType(),typeof(bool),typeof(bool),typeof(bool)
            }, null);

            s_DoTextFieldMF = mf;
            var param = new object[] { textEditor, id, position, text, style, allowedletters, changed, reset, multiline, passwordField };
            var result = s_DoTextFieldMF.Invoke(null, param) as string;
            changed = true;
            return result;
        }

        public static string ToolbarSearchField(int id, Rect position, string text, bool showWithPopupArrow)
        {
            if (s_ToolbarSearchFieldDelegate != null)
                return s_ToolbarSearchFieldDelegate(id, position, text, showWithPopupArrow);

            Type type = typeof(EditorGUI);
            var mf = type.GetMethod("ToolbarSearchField", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[]
            {
                typeof(int),typeof(Rect),typeof(string),typeof(bool)
            }, null);

            var action = (ToolbarSearchFieldDelegate)Delegate.CreateDelegate(typeof(ToolbarSearchFieldDelegate), mf);
            if (action == null)
                throw new NullReferenceException("action");

            s_ToolbarSearchFieldDelegate = action;
            return s_ToolbarSearchFieldDelegate(id, position, text, showWithPopupArrow);
        }

        public static string SearchField(Rect position, string text)
        {
            if (s_SearchFieldDelegate != null)
                return s_SearchFieldDelegate(position, text);

            Type type = typeof(EditorGUI);
            var mf = type.GetMethod("SearchField", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[]
            {
                typeof(Rect),typeof(string)
            }, null);

            var action = (SearchFieldDelegate)Delegate.CreateDelegate(typeof(SearchFieldDelegate), mf);
            if (action == null)
                throw new NullReferenceException("action");

            s_SearchFieldDelegate = action;
            return s_SearchFieldDelegate(position, text);
        }
    }

}