using System.IO;
using System.Text;
using JsonFx.U3DEditor;
using SharpZipLib.U3DEditor.GZip;
using UnityEngine;

namespace EUTK
{
    public abstract class EditorWindowConfigSource : ScriptableObjectWrap
    {
        [HideInInspector]
        [JsonIgnore]
        [SerializeField]
        protected byte[] m_Data;

        [JsonIgnore]
        protected bool m_LazyMode;
        [JsonIgnore]
        protected bool m_IsDirty;

        protected EditorWindowConfigSource()
        {

        }

        public virtual void SetConfigDirty()
        {
            if (!m_LazyMode)
            {
                m_IsDirty = false;
                Save();
            }
            else
            {
                m_IsDirty = true;
            }
        }

        public virtual void SaveConfigLazily()
        {
            if (m_IsDirty || !m_LazyMode)
            {
                m_IsDirty = false;
                Save();
            }
        }

        protected abstract void Save();

        public static byte[] Compress(string content)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (GZipOutputStream outStream = new GZipOutputStream(memoryStream))
            {
                var data = Encoding.UTF8.GetBytes(content);
                outStream.IsStreamOwner = false;
                outStream.SetLevel(4);
                outStream.Write(data, 0, data.Length);
                outStream.Flush();
                outStream.Finish();
            }
            return memoryStream.GetBuffer();
        }

        public static byte[] Decompress(byte[] bytesToDecompress)
        {
            byte[] writeData = new byte[4096];
            GZipInputStream s2 = new GZipInputStream(new MemoryStream(bytesToDecompress));
            MemoryStream outStream = new MemoryStream();
            while (true)
            {
                int size = s2.Read(writeData, 0, writeData.Length);
                if (size > 0)
                {
                    outStream.Write(writeData, 0, size);
                }
                else
                {
                    break;
                }
            }
            s2.Close();
            byte[] outArr = outStream.ToArray();
            outStream.Close();
            return outArr;
        }
    }
}
