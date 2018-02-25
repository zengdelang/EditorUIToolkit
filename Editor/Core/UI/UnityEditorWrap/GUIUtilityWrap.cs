using System;
using System.Reflection;
using UnityEngine;

namespace EUTK
{
    public class GUIUtilityWrap
    {
        public delegate GUISkin GetDefaultSkinDelegate(int skinMode);

        private static GetDefaultSkinDelegate getDefaultSkinDelegate;

        public static Rect GUIToScreenRect(Rect guiRect)
        {
            Vector2 vector2 = GUIUtility.GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
            guiRect.x = vector2.x;
            guiRect.y = vector2.y;
            return guiRect;
        }
    }
}