using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class TooltipViewWrap
    {
        protected static Action<string, Rect> s_ShowAction;
        protected static Action s_CloseAction;

        public static void Show(string tooltip, Rect rect)
        {
            if (s_ShowAction != null)
            {
                s_ShowAction(tooltip, rect);
                return;
            }

            Type type = typeof(Editor);
            type = type.Assembly.GetType("UnityEditor.TooltipView");
            var mf = type.GetMethod("Show", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(Rect) }, null);

            var action = (Action<string, Rect>)Delegate.CreateDelegate(typeof(Action<string, Rect>), mf);
            if (action == null)
                throw new NullReferenceException("action");
            s_ShowAction = action;
            s_ShowAction(tooltip, rect);
        }

        public static void Close()
        {
            if (s_CloseAction != null)
            {
                s_CloseAction();
                return;
            }

            Type type = typeof(Editor);
            type = type.Assembly.GetType("UnityEditor.TooltipView");
            var mf = type.GetMethod("Close", BindingFlags.Public | BindingFlags.Static, null, new Type[] { }, null);
            var action = (Action)Delegate.CreateDelegate(typeof(Action), mf);
            if (action == null)
                throw new NullReferenceException("action");
            s_CloseAction = action;
            s_CloseAction();
        }
    }
}