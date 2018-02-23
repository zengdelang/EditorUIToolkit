using System;
using System.IO;
using System.Text;
using JsonFx.U3DEditor;
using SharpZipLib.U3DEditor.GZip;
using UnityEngine;

namespace EUTK
{
    public abstract class EditorWindowConfigSource : ScriptableObjectWrap
    {
        public class ValidFlag
        {
            
        }

        [HideInInspector] [JsonIgnore] [SerializeField] protected byte[] m_Data;

        [JsonIgnore] [NonSerialized] protected bool m_LazyMode;
        [JsonIgnore] [NonSerialized] protected bool m_IsDirty;

        /// <summary>
        /// Unity再Play模式下，如果正在编译，有概率会导致配置对象数据被重置，如果validFlag在保存数据的时候为空
        /// 说明当前数据可能损坏，不应该保存当前数据
        /// </summary>
        [JsonIgnore]
        [SerializeField]
        public ValidFlag validFlag;

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
