using System;
using System.IO;
using System.Text;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonOptIn]
    public class FileConfigSource : EditorWindowConfigSource
    {
        [JsonIgnore]
        protected string m_FilePath;

        protected override void Save()
        {
            var content = JsonWriter.Serialize(this, new JsonWriterSettings() { MaxDepth = Int32.MaxValue });
            var filePath = Path.GetFullPath(m_FilePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            File.WriteAllBytes(filePath, Compress(content));
        }

        public static EditorWindowConfigSource CreateFileConfigSource(string configFilePath, bool lazyMode, Type type)
        {
            FileConfigSource fileConfigSource;
            var filePath = Path.GetFullPath(configFilePath);
            if (File.Exists(filePath))
            {
                var data = File.ReadAllBytes(filePath);
                fileConfigSource = JsonReader.Deserialize(Encoding.UTF8.GetString(Decompress(data)), type, true) as FileConfigSource;
            }
            else
            {
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    fileConfigSource = CreateInstance(type) as FileConfigSource;
                }
                else
                {
                    fileConfigSource = Activator.CreateInstance(type) as FileConfigSource;
                }               
            }

            fileConfigSource.m_FilePath = configFilePath;
            fileConfigSource.m_LazyMode = lazyMode;
            return fileConfigSource;
        }
    }

}