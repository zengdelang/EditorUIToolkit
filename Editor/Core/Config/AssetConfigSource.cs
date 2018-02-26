using System;
using System.IO;
using System.Text;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    [JsonOptIn]
    public class AssetConfigSource : EditorWindowConfigSource
    {
        protected override void Save()
        {
            if (validFlag == null)
            {
                return;
            }
            var info = JsonWriter.Serialize(this, new JsonWriterSettings() { MaxDepth = Int32.MaxValue });
            if (validFlag == null)
            {
                return;
            }
            m_Data = Compress(info);
            EditorUtility.SetDirty(this);
        }

        public static EditorWindowConfigSource CreateAssetConfigSource(string configAssetPath, bool lazyMode, Type type)
        {
            string assetPath = null;
            var configSource = Resources.Load(configAssetPath) as AssetConfigSource;
            if (configSource != null && configSource.GetType() != type)
            {
                assetPath = AssetDatabase.GetAssetPath(configSource);
                configSource = null;
            }

            if (configSource != null)
            {
                var jsonReaderSettings = new JsonReaderSettings();
                jsonReaderSettings.HandleCyclicReferences = true;
                if (configSource.m_Data != null && configSource.m_Data.Length > 0)
                {
                    var jsonReader = new JsonReader(Encoding.UTF8.GetString(Decompress(configSource.m_Data)), jsonReaderSettings);
                    jsonReader.autoType = true;
                    jsonReader.PopulateObject(ref configSource);
                }
                configSource.m_LazyMode = lazyMode;
                return configSource;
            }

            configSource = CreateInstance(type) as AssetConfigSource;
            configSource.m_LazyMode = lazyMode;

            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = "Assets/Editor/Resources/" + configAssetPath + ".asset";
                var path = Path.Combine(Application.dataPath, "Editor/Resources/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            AssetDatabase.CreateAsset(configSource, assetPath);
            AssetDatabase.Refresh();
            return configSource;
        }
    }
}