using System;
using UnityEngine;

namespace EUTK
{
    public class NodeGroupInspectorGUI : ScriptableObject
    {
        [HideInInspector] public NodeGroup nodeGroup;
        public Action SetDirtyAction;
    }
}