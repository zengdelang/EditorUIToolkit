using System;
using System.Reflection;
using UnityEngine;

namespace EUTK
{
    public class GUIClipWrap
    {
        public delegate Vector2 UnclipVector2Delegate(Vector2 pos);
        protected static UnclipVector2Delegate s_UnclipRectDelegate;

        public delegate Rect VisibleRectAction();
        protected static VisibleRectAction s_VisibleRectAction;

        public delegate void PushDelegate(Rect screenRect, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset);

        protected static PushDelegate m_PushDelegate;

        protected static Action<Rect, Vector2, Vector2, bool> s_PushAction;
        protected static Action s_PopAction;

        public static Vector2 Unclip(Vector2 pos)
        {
            if (s_UnclipRectDelegate != null)
            {
                return s_UnclipRectDelegate(pos);
            }

            Type type = typeof(GUIUtility);
            type = type.Assembly.GetType("UnityEngine.GUIClip");
            var mf = type.GetMethod("Unclip",
                BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Vector2) }, null);

            var action = (UnclipVector2Delegate)Delegate.CreateDelegate(typeof(UnclipVector2Delegate), mf);
            if (action == null)
                throw new NullReferenceException("action");
            s_UnclipRectDelegate = action;
            return s_UnclipRectDelegate(pos);
        }

        public static Rect visibleRect
        {
            get
            {
                if (s_VisibleRectAction != null)
                {
                    return s_VisibleRectAction();
                }
                Type type = typeof(GUIUtility);
                type = type.Assembly.GetType("UnityEngine.GUIClip");
                var mf = type.GetProperty("visibleRect", BindingFlags.Public | BindingFlags.Static);
                var action = (VisibleRectAction)Delegate.CreateDelegate(typeof(VisibleRectAction), mf.GetGetMethod());
                if (action == null)
                    throw new NullReferenceException("action");
                s_VisibleRectAction = action;
                return s_VisibleRectAction();
            }
        }

        public static void Push(Rect screenRect, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset)
        {
            if (s_PushAction != null)
            {
                s_PushAction(screenRect, scrollOffset, renderOffset, resetOffset);
                return;
            }

            Type type = typeof(GUIUtility);
            type = type.Assembly.GetType("UnityEngine.GUIClip");
            var mf = type.GetMethod("Push",
                BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Rect), typeof(Vector2), typeof(Vector2), typeof(bool) }, null);
            mf.Invoke(null, new object[] { screenRect, scrollOffset, renderOffset, resetOffset });

            var action = (Action<Rect, Vector2, Vector2, bool>)Delegate.CreateDelegate(typeof(Action<Rect, Vector2, Vector2, bool>), mf);
            if (action == null)
                throw new NullReferenceException("action");
            s_PushAction = action;
            s_PushAction(screenRect, scrollOffset, renderOffset, resetOffset);
        }

        public static void Pop()
        {
            if (s_PopAction != null)
            {
                s_PopAction();
                return;
            }

            Type type = typeof(GUIUtility);
            type = type.Assembly.GetType("UnityEngine.GUIClip");
            var mf = type.GetMethod("Pop",
                BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { }, null);
            mf.Invoke(null, new object[] { });

            var action = (Action)Delegate.CreateDelegate(typeof(Action), mf);
            if (action == null)
                throw new NullReferenceException("action");
            s_PopAction = action;
            s_PopAction();
        }
    }
}
