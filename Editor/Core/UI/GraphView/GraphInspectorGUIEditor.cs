using UnityEditor;

namespace EUTK
{
    [CustomEditor(typeof(GraphInspectorGUI))]
    public class GraphInspectorGUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var graphGUI = target as GraphInspectorGUI;

            if (graphGUI.showNode)
            {
                if (graphGUI.node != null)
                {
                    graphGUI.node.ShowNodeInspectorGUI();
                }
                else
                {
                    Selection.activeObject = null;
                }
            }
            else
            {
                if (graphGUI.conn != null)
                {
                    graphGUI.conn.ShowConnectionInspectorGUI();
                }
                else
                {
                    Selection.activeObject = null;
                }
            }
        }
    }
}
