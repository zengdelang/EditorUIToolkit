using System;
using System.Reflection;
using UnityEngine;

namespace EUTK
{
    public class SpriteUtilityWrap
    {
        public delegate Texture2D CreateTemporaryDuplicateDelegate(Texture2D original, int width, int height);
        protected static CreateTemporaryDuplicateDelegate m_CreateTemporaryDuplicateDelegate;

        public static Texture2D CreateTemporaryDuplicate(Texture2D original, int width, int height)
        {
            if (m_CreateTemporaryDuplicateDelegate != null)
            {
                return m_CreateTemporaryDuplicateDelegate(original, width, height);
            }

            Type type = typeof(UnityEditor.AssetDatabase);
            type = type.Assembly.GetType("UnityEditor.SpriteUtility");
            var mf = type.GetMethod("CreateTemporaryDuplicate",
                BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture2D), typeof(int), typeof(int) }, null);

            var action = (CreateTemporaryDuplicateDelegate)Delegate.CreateDelegate(typeof(CreateTemporaryDuplicateDelegate), mf);
            if (action == null)
                throw new NullReferenceException("action");
            m_CreateTemporaryDuplicateDelegate = action;
            return m_CreateTemporaryDuplicateDelegate(original, width, height);
        }
    }
}
