using System;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class NodeGroup
    {
        [JsonMember] [SerializeField] public string name;
        [JsonMember] [SerializeField] [HideInInspector] public Rect rect;
        [JsonMember] [SerializeField] public Color color;

        [JsonIgnore] [NonSerialized] public bool isDragging;
        [JsonIgnore] [NonSerialized] public bool isRescaling;
        [JsonIgnore] [NonSerialized] public bool isRenaming;

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
}