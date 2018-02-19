using System;
using System.Collections.Generic;
using System.Reflection;

namespace EUTK
{
    public class GUIViewWrap
    {
        public delegate void RepaintDelegate();
        protected static Dictionary<object, RepaintDelegate> s_ActionMap = new Dictionary<object, RepaintDelegate>();

        public delegate object CurrentDelegate();

        protected static CurrentDelegate s_CurrentAction;

        public static object current
        {
            get
            {
                if (s_CurrentAction != null)
                {
                    return s_CurrentAction();
                }

                Type type = typeof(UnityEditor.Editor);
                type = type.Assembly.GetType("UnityEditor.GUIView");

                var mf = type.GetProperty("current",
                    BindingFlags.Static | BindingFlags.Public);

                var action = (CurrentDelegate)Delegate.CreateDelegate(typeof(CurrentDelegate), mf.GetGetMethod());
                if (action == null)
                    throw new NullReferenceException("action");
                s_CurrentAction = action;
                return s_CurrentAction();
            }
        }

        public static void Repaint(object obj)
        {
            if (s_ActionMap.ContainsKey(obj))
            {
                s_ActionMap[obj]();
                return;
            }

            Type type = typeof(UnityEditor.Editor);
            type = type.Assembly.GetType("UnityEditor.GUIView");
            var mf = type.GetMethod("Repaint",
                BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null);

            var action = (RepaintDelegate)Delegate.CreateDelegate(typeof(RepaintDelegate), obj, mf);
            if (action == null)
                throw new NullReferenceException("action");

            s_ActionMap.Add(obj, action);
            action();
        }
    }
}
