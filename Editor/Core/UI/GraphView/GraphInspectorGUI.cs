using System;
using UnityEngine;

namespace EUTK
{
    public class GraphInspectorGUI : ScriptableObject
    {
        [SerializeField] [HideInInspector] public Node node;
        [SerializeField] [HideInInspector] public Connection conn;
        [SerializeField] [HideInInspector] public bool showNode;  //是否显示的是结点，false为connection

        [NonSerialized]
        public bool isShowingValidInfo; //是否显示的是有效信息，true代表是，如果unity编译后，变为false
    }
}

