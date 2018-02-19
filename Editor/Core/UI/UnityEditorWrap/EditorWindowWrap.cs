using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class EditorWindowWrap
    {
        public delegate bool HasFocusDelegate();
        protected static Dictionary<EditorWindow, HasFocusDelegate> s_ActionMap = new Dictionary<EditorWindow, HasFocusDelegate>();

        public static bool HasFocus(EditorWindow window)
        {
            if (s_ActionMap.ContainsKey(window))
            {
                return s_ActionMap[window]();
            }

            var fi = typeof(EditorWindow).GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
            var obj = fi.GetValue(window);
            var p = obj.GetType().GetProperty("hasFocus", BindingFlags.Public | BindingFlags.Instance);

            var action = (HasFocusDelegate)Delegate.CreateDelegate(typeof(HasFocusDelegate), obj, p.GetGetMethod());
            if (action == null)
                throw new NullReferenceException("action");

            s_ActionMap.Add(window, action);
            return action();
        }

        /// <summary>
        /// 无需优化调用次数不多
        /// </summary>
        public static void ShowAsDropDown(EditorWindow window, Rect buttonRect, Vector2 windowSize)
        {
            var type = typeof(UnityEditor.EditorWindow);

            var popupLocationArrayType = type.Assembly.GetType("UnityEditor.PopupLocationHelper+PopupLocation[]");
            var showModeType = type.Assembly.GetType("UnityEditor.ShowMode");

            var mf = type.GetMethod("ShowAsDropDown", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
            {
                typeof(Rect), typeof(Vector2), popupLocationArrayType, showModeType
            }, null);
            mf.Invoke(window, new object[] { buttonRect, windowSize, null, Enum.Parse(type.Assembly.GetType("UnityEditor.ShowMode"), "PopupMenuWithKeyboardFocus") });
        }

        /// <summary>
        /// 无需优化调用次数不多
        /// </summary>
        public static void AddToAuxWindowList(EditorWindow window)
        {
            var fi = typeof(EditorWindow).GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
            var obj = fi.GetValue(window);
            var p = obj.GetType().GetMethod("AddToAuxWindowList", BindingFlags.NonPublic | BindingFlags.Instance);
            p.Invoke(obj, null);
        }
    }
}
