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
            var configSource = Resources.Load<AssetConfigSource>(configAssetPath);

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

            var path = Path.Combine(Application.dataPath, "Editor/Resources/Config/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            configSource = CreateInstance(type) as AssetConfigSource;
            configSource.m_LazyMode = lazyMode;
            AssetDatabase.CreateAsset(configSource, "Assets/Editor/Resources/" + configAssetPath + ".asset");
            AssetDatabase.Refresh();
            return configSource;
        }
    }
}