using UnityEditor;
using UnityEngine;

namespace EUTK
{
    [CustomEditor(typeof(NodeGroupInspectorGUI))]
    public class NodeGroupInspectorGUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var nodeGroupInspectorGUI = target as NodeGroupInspectorGUI;
            if (nodeGroupInspectorGUI.nodeGroup != null)
            {
                nodeGroupInspectorGUI.nodeGroup.ShowNodeGroupInspector();
            }

            if (GUI.changed && nodeGroupInspectorGUI.SetDirtyAction != null)
                nodeGroupInspectorGUI.SetDirtyAction();
        }
    }
}