using System;
using UnityEditor;

namespace EUTK
{
    public class DelayedCallback
    {
        private Action m_Callback;
        private double m_CallbackTime;

        public DelayedCallback(Action function, double timeFromNow)
        {
            m_Callback = function;
            m_CallbackTime = EditorApplication.timeSinceStartup + timeFromNow;
            EditorApplication.update += Update;
        }

        public void Clear()
        {
            EditorApplication.update -= Update;
            m_CallbackTime = 0.0;
            m_Callback = null;
        }

        private void Update()
        {
            if (EditorApplication.timeSinceStartup <= m_CallbackTime)
                return;

            Action action = m_Callback;
            Clear();
            action();
        }
    }
}