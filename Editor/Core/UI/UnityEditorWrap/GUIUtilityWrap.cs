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

        public static GUISkin GetDefaultSkin(int skinMode)
        {
            if (getDefaultSkinDelegate != null)
            {
                return getDefaultSkinDelegate(skinMode);
            }

            Type type = typeof(GUIUtility);
            var mf = type.GetMethod("GetDefaultSkin",
                BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);

            var action = (GetDefaultSkinDelegate)Delegate.CreateDelegate(typeof(GetDefaultSkinDelegate), mf);
            if (action == null)
                throw new NullReferenceException("action");
            getDefaultSkinDelegate = action;
            return mf.Invoke(null, new object[]{skinMode}) as GUISkin;
        }
    }
}