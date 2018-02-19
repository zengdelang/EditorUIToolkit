using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public static class EditorHelper
    {
        public class ScriptInfo
        {
            public Type type;
            public string name;
            public string category;
            public string description;

            public ScriptInfo(Type type, string name, string category)
            {
                this.type = type;
                this.name = name;
                this.category = category;
                if (type != null)
                {
                    var descAtt = type.GetAttribute<DescriptionAttribute>(true);
                    description = descAtt != null ? descAtt.description : description;
                }
            }
        }

        public static GenericMenu GetTypeSelectionMenu(Type baseType, Action<Type> callback, GenericMenu menu = null, string subCategory = null)
        {
            if (menu == null)
                menu = new GenericMenu();

            if (subCategory != null)
                subCategory = subCategory + "/";

            GenericMenu.MenuFunction2 Selected = delegate (object selectedType) {
                callback((Type)selectedType);
            };

            var scriptInfos = GetScriptInfosOfType(baseType);
            foreach (var info in scriptInfos.Where(info => string.IsNullOrEmpty(info.category)))
            {
                menu.AddItem(new GUIContent(subCategory + info.name, info.description), false, info.type != null ? Selected : null, info.type);
            }

            foreach (var info in scriptInfos.Where(info => !string.IsNullOrEmpty(info.category)))
            {
                menu.AddItem(new GUIContent(subCategory + info.category + "/" + info.name, info.description), false, info.type != null ? Selected : null, info.type);
            }

            return menu;
        }


        private static Dictionary<Type, List<ScriptInfo>> cachedInfos = new Dictionary<Type, List<ScriptInfo>>();
        public static List<ScriptInfo> GetScriptInfosOfType(Type baseType, Type extraGenericType = null)
        {
            List<ScriptInfo> infos;
            if (cachedInfos.TryGetValue(baseType, out infos))
            {
                return infos;
            }

            infos = new List<ScriptInfo>();

            var subTypes = GetAssemblyTypes(baseType);
            if (baseType.IsGenericTypeDefinition)
            {
                subTypes = new List<Type> {baseType};
            }

            foreach (var subType in subTypes)
            {
                if (subType.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() == null)
                {
                    if (subType.IsAbstract)
                    {
                        continue;
                    }

                    var scriptName = subType.FriendlyName().SplitCamelCase();
                    var scriptCategory = string.Empty;

                    var nameAttribute = subType.GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault() as NameAttribute;
                    if (nameAttribute != null)
                        scriptName = nameAttribute.name;

                    var categoryAttribute = subType.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
                    if (categoryAttribute != null)
                        scriptCategory = categoryAttribute.category;

                    infos.Add(new ScriptInfo(subType, scriptName, scriptCategory));
                }
            }

            infos = infos.OrderBy(script => script.name).ToList();
            infos = infos.OrderBy(script => script.category).ToList();
            return cachedInfos[baseType] = infos;
        }

        private static Dictionary<Type, List<Type>> cachedSubTypes = new Dictionary<Type, List<Type>>();
        public static List<Type> GetAssemblyTypes(Type baseType)
        {
            List<Type> subTypes;
            if (cachedSubTypes.TryGetValue(baseType, out subTypes))
            {
                return subTypes;
            }

            subTypes = new List<Type>();
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetExportedTypes().Where(t => t.IsSubclassOf(baseType)))
                    {
                        subTypes.Add(t);
                    }
                }
                catch
                {
                    Debug.Log(asm.FullName + " will be excluded");
                }
            }

            subTypes = subTypes.OrderBy(t => t.FriendlyName()).ToList();
            subTypes = subTypes.OrderBy(t => t.Namespace).ToList();
            return cachedSubTypes[baseType] = subTypes;
        }

        public static bool OpenScriptOfType(Type type)
        {
            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.EndsWith(type.Name + ".cs") || path.EndsWith(type.Name + ".js"))
                {
                    var script = (MonoScript) AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
                    if (type == script.GetClass())
                    {
                        AssetDatabase.OpenAsset(script);
                        return true;
                    }
                }
            }

            Debug.Log(string.Format("Can't open script of type '{0}', cause a script with the same name does not exist", type.FriendlyName()));
            return false;
        }
    }
}