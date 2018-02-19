using System;
using System.Linq;

namespace EUTK
{
    public static class TypeUtility
    {
        public static bool IsSubclassOf(this Type type, Type other)
        {
#if NETFX_CORE
			return type.GetTypeInfo().IsSubclassOf(other);
#else
            return type.IsSubclassOf(other);
#endif
        }

        public static T GetAttribute<T>(this Type type, bool inherited) where T : Attribute
        {
#if NETFX_CORE
			return (T)type.GetTypeInfo().GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
#else
            return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
#endif
        }

        public static bool IsGenericParameter(this Type type)
        {
#if NETFX_CORE
			return type.GetTypeInfo().IsGenericParameter;
#else
            return type.IsGenericParameter;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETFX_CORE
			return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Type[] GetGenericArguments(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static string FriendlyName(this Type t, bool trueSignature = false)
        {
            if (t == null)
            {
                return null;
            }

            if (!trueSignature && t == typeof(UnityEngine.Object))
            {
                return "UnityObject";
            }

            var s = trueSignature ? t.FullName : t.Name;
            if (!trueSignature)
            {
                if (s == "Single") { s = "Float"; }
                if (s == "Int32") { s = "Integer"; }
            }

            if (t.IsGenericParameter())
            {
                s = "T";
            }

            if (t.IsGenericType())
            {
                s = trueSignature ? t.FullName : t.Name;
                var args = t.GetGenericArguments();
                if (args.Length != 0)
                {
                    s = s.Replace("`" + args.Length.ToString(), "");
                    s += "<";
                    for (var i = 0; i < args.Length; i++)
                    {
                        s += (i == 0 ? "" : ", ") + args[i].FriendlyName(trueSignature);
                    }
                    s += ">";
                }
            }
            return s;
        }
    }
}