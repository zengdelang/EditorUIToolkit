using JsonFx.U3DEditor;

namespace EUTK
{
    public class GraphEditorWindowSetting : FileConfigSource
    {
        [JsonMember] protected Graph m_Graph;

        public Graph Graph
        {
            get { return m_Graph; }
            set { m_Graph = value; }
        }
    }
}