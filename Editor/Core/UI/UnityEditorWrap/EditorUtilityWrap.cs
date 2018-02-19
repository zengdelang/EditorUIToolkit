using System;
using System.Reflection;
using UnityEditor;

namespace EUTK
{
    public class EditorUtilityWrap
    {
        public delegate string GetInvalidFilenameCharsDelegate();
        protected static GetInvalidFilenameCharsDelegate s_GetInvalidFilenameCharsDelegate;

        public static string GetInvalidFilenameChars()
        {
            if (s_GetInvalidFilenameCharsDelegate != null)
                return s_GetInvalidFilenameCharsDelegate();

            Type editorUtilityType = typeof(EditorUtility);
            var mi = editorUtilityType.GetMethod("GetInvalidFilenameChars",
                BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { }, null);

            var action = (GetInvalidFilenameCharsDelegate)Delegate.CreateDelegate(typeof(GetInvalidFilenameCharsDelegate), mi);
            if (action == null)
                throw new NullReferenceException("action");

            s_GetInvalidFilenameCharsDelegate = action;
            return s_GetInvalidFilenameCharsDelegate();
        }
    }
}