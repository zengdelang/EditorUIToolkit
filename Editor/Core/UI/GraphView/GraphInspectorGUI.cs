using UnityEngine;

namespace EUTK
{
    public class GraphInspectorGUI : ScriptableObject
    {
        [HideInInspector] public Node node;
        [HideInInspector] public Connection conn;
        [HideInInspector] public bool showNode;  //是否显示的是结点，false为connection
    }
}

