using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    [Serializable]
    public class NodeGroup
    {
        [SerializeField] public string name;
        [SerializeField] [HideInInspector] public Rect rect;
        [SerializeField] public Color color;

        [NonSerialized] public bool isDragging;
        [NonSerialized] public bool isRescaling;
        [NonSerialized] public bool isRenaming;

      

        public NodeGroup()
        {

        }

        public NodeGroup(Rect rect, string name)
        {
            this.rect = rect;
            this.name = name;
        }

        public void ShowNodeGroupInspector()
        {
            name = EditorGUILayout.TextField("Node Group Name", name);
            color = EditorGUILayout.ColorField("Node Group Color", color);
        }
    }

    public class NodeGroupInspectorGUI : ScriptableObject
    {
        [HideInInspector] public NodeGroup nodeGroup;
        public Action SetDirtyAction;
    }
}