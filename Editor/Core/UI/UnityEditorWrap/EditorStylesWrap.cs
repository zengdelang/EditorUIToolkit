using System;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace EUTK
{
    public class EditorStylesWrap
    {
        private static GUIStyle s_InspectorBig;
        private static GUIStyle s_ToolbarSeachField;

        public static GUIStyle inspectorBig
        {
            get
            {
                if (s_InspectorBig != null)
                    return s_InspectorBig;

                Type type = typeof(EditorStyles);
                var pi = type.GetProperty("inspectorBig", BindingFlags.Static | BindingFlags.NonPublic);
                s_InspectorBig = (GUIStyle)pi.GetValue(null, null);
                return s_InspectorBig;
            }
        }

        public static GUIStyle toolbarSearchField
        {
            get
            {
                if (s_ToolbarSeachField != null)
                    return s_ToolbarSeachField;

                Type type = typeof(EditorStyles);
                var pi = type.GetProperty("toolbarSearchField", BindingFlags.Static | BindingFlags.NonPublic);
                s_ToolbarSeachField = (GUIStyle)pi.GetValue(null, null);
                return s_ToolbarSeachField;
            }
        }
    }
}