using System;
using System.Collections.Generic;
using System.Reflection;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    public class ScriptableObjectWrap : ScriptableObject
    {
        public delegate T GetMethodDelegate<T>();

        [JsonIgnore]
        protected Dictionary<string, PropertyInfo> m_PropsMap;
        [JsonIgnore]
        protected Dictionary<string, object> m_SetActionMap;
        [JsonIgnore]
        protected Dictionary<string, object> m_GetActionMap;

        public PropertyInfo FindProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            if (m_PropsMap == null)
                m_PropsMap = new Dictionary<string, PropertyInfo>();

            if (m_PropsMap.ContainsKey(propertyName))
                return m_PropsMap[propertyName];

            var property = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            m_PropsMap[propertyName] = property;
            return property;
        }

        public T GetValue<T>(string propertyName)
        {
            if (m_GetActionMap == null)
                m_GetActionMap = new Dictionary<string, object>();

            if (m_GetActionMap.ContainsKey(propertyName))
            {
                var action = m_GetActionMap[propertyName] as GetMethodDelegate<T>;
                if (action == null)
                    throw new NullReferenceException("action");
                return action();
            }

            var action1 = GetGetAction<T>(propertyName);
            m_GetActionMap[propertyName] = action1;
            return action1();
        }

        public void SetValue<T>(string propertyName, T value)
        {
            if (m_SetActionMap == null)
                m_SetActionMap = new Dictionary<string, object>();

            if (m_SetActionMap.ContainsKey(propertyName))
            {
                var action = m_SetActionMap[propertyName] as Action<T>;
                if (action == null)
                    throw new NullReferenceException("action");
                action(value);
            }
            else
            {
                var action = GetSetAction<T>(propertyName);
                m_SetActionMap[propertyName] = action;
                action(value);
            }
        }

        protected GetMethodDelegate<T> GetGetAction<T>(string propertyName)
        {
            var property = FindProperty(propertyName);
            MethodInfo method = property.GetGetMethod(true);
            var action = (GetMethodDelegate<T>)Delegate.CreateDelegate(typeof(GetMethodDelegate<T>), this, method, false);
            if (action == null)
                throw new NullReferenceException("action");
            return action;
        }

        protected Action<T> GetSetAction<T>(string propertyName)
        {
            var property = FindProperty(propertyName);
            if (property.CanWrite == false)
                throw new NotSupportedException("属性不支持写操作。");

            MethodInfo method = property.GetSetMethod(true);
            if (method.GetParameters().Length > 1)
                throw new NotSupportedException("不支持构造索引器属性的委托。");

            var action = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), this, method, false);
            if (action == null)
                throw new NullReferenceException("action");
            return action;
        }
    }
}
