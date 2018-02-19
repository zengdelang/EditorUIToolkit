using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{ 
    public class Graph1 : Graph
    {
        public override Type baseNodeType
        {
            get { return typeof(Node1); }
        }
    }

    public abstract class Node1 : Node
    {
        public override Type outConnectionType { get { return typeof(Connection1); } }
    }

    [Category("Composites")]
    [Description("Works like a normal Selector, but when a child node returns Success, that child will be moved to the end.\nAs a result, previously Failed children will always be checked first and recently Successful children last")]
    public class Node11 : Node1
    {
        [SerializeField]
        protected string haha;

        [SerializeField]
        public Dictionary<string, string> ss;

        [SerializeField] public List<string> dd;

        public string dsew;


        public int xxx;

        sealed public override int maxOutConnections { get { return -1; } }



        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Node12 : Node1
    {
        [SerializeField]
        protected string haha11;


        sealed public override int maxOutConnections { get { return 5; } }



        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Node13 : Node1
    {
        [SerializeField]
        protected string haha11;


        sealed public override int maxOutConnections { get { return 5; } }



        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Node14 : Node1
    {
        [SerializeField]
        protected string haha11;


        sealed public override int maxOutConnections { get { return 5; } }



        public override int maxInConnections
        {
            get { return -1; }
        }
    }

    public class Connection1 : Connection
    {

    }

    public class GraphEditorWindow : ViewGroupEditorWindow
    {
        [MenuItem("Tools/Eaxamples/GraphEditorWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            GetWindow<GraphEditorWindow>();
        }

        protected override void InitData()
        {
            m_WindowConfigSource = FileConfigSource.CreateFileConfigSource("ViewConfig/TestWindow/config6.txt", true, typeof(GraphEditorWindowSetting));

            ViewGroup viewGroup = new ViewGroup(m_LayoutGroupMgr);
            var graphView = new GraphView(m_LayoutGroupMgr, m_WindowConfigSource);

            graphView.currentGraph = m_WindowConfigSource.GetValue<Graph>("Graph");
            if (graphView.currentGraph == null)
            {
                graphView.currentGraph = CreateInstance<Graph1>();
                m_WindowConfigSource.SetValue("Graph", graphView.currentGraph);
                m_WindowConfigSource.SetConfigDirty();
            }
            graphView.currentGraph.windowConfig = m_WindowConfigSource;

            var searchBar = new SearchBar(m_LayoutGroupMgr);
            viewGroup.AddView(searchBar);
            viewGroup.AddView(graphView);
            m_LayoutGroupMgr.AddViewGroup(viewGroup);
        }
    }
}