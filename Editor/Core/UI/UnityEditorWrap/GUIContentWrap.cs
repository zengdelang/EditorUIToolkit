using System;
using UnityEngine;
using System.Reflection;

namespace EUTK
{
    public class GUIContentWrap
    {
        public delegate GUIContent TempDelegate(string displayName);

        private static TempDelegate tempDelegate;

        public static GUIContent Temp(string displayName)
        {
            if (tempDelegate != null)
            {
                return tempDelegate(displayName);
            }

            Type type = typeof(GUIContent);
            var mf = type.GetMethod("Temp",
                BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);

            var action = (TempDelegate)Delegate.CreateDelegate(typeof(TempDelegate), mf);
            if (action == null)
                throw new NullReferenceException("action");
            tempDelegate = action;
            return (GUIContent)mf.Invoke(null, new object[] { displayName });
        }
    }
}
