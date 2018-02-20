using System;
using System.Collections.Generic;
using System.Linq;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public abstract class Graph : ScriptableObject
    {
        [SerializeField] protected Vector2 m_Translation = new Vector2(-5000, -5000);

        [SerializeField] protected float m_ZoomFactor = 1f;

        [SerializeField] protected List<Node> m_Nodes = new List<Node>();

        [SerializeField] protected GraphConfig m_GraphConfig = new GraphConfig();

        [SerializeField] protected int m_AutoId;

        [SerializeField] public int m_Count;

        [SerializeField] public List<Connection> connectionList = new List<Connection>();

        [SerializeField] protected List<NodeGroup> m_NodeGroups;

        [NonSerialized] public int keyboardControl;
        [NonSerialized] public Node[] CopiedNodes;
        [NonSerialized] public EditorWindowConfigSource windowConfig;
        [NonSerialized] public bool needRebuildGraph = true;

        [NonSerialized] public bool isMultiSelecting;
        [NonSerialized] public bool isDraggingPort;
        [NonSerialized] public int dragDropMisses;

        [JsonIgnore] [SerializeField] protected List<int> m_MultiSelection = new List<int>();

        protected virtual List<int> multiSelection
        {
            get { return m_MultiSelection; }
            set { m_MultiSelection = (value != null) ? value : new List<int>(); }
        }

        public Node.GUIPort clickedPort
        {
            get; set;
        }

        public List<NodeGroup> nodeGroups
        {
            get { return m_NodeGroups; }
            set { m_NodeGroups = value; }
        }

        public virtual Vector2 translation
        {
            get { return m_Translation; }
            set { m_Translation = value; }
        }

        public virtual float zoomFactor
        {
            get { return m_ZoomFactor; }
            set { m_ZoomFactor = value; }
        }

        public virtual List<Node> allNodes
        {
            get { return m_Nodes; }
            protected set { m_Nodes = value; }
        }

        public GraphConfig graphConfig
        {
            get { return m_GraphConfig; }
        }

        public virtual Action PostGUI { get; set; }

        public abstract Type baseNodeType { get; }

        public virtual int GetAutoId()
        {
            return ++m_AutoId;
        }

        public virtual T AddNode<T>() where T : Node
        {
            return (T) AddNode(typeof(T));
        }

        public virtual T AddNode<T>(Vector2 pos) where T : Node
        {
            return (T) AddNode(typeof(T), pos);
        }

        public virtual Node AddNode(Type nodeType)
        {
            return AddNode(nodeType, new Vector2(50, 50));
        }

        public virtual Node AddNode(Type nodeType, Vector2 pos)
        {
            if (!nodeType.IsSubclassOf(baseNodeType))
            {
                Debug.LogWarning(nodeType + " can't be added to " + GetType().FriendlyName() + " graph");
                return null;
            }

            var newNode = Node.Create(this, nodeType, pos);
            Undo.RecordObject(this, "New Node");

            newNode.id = GetAutoId();
            allNodes.Add(newNode);
            SetConfigDirty();
            return newNode;
        }

        public virtual void RemoveNode(int nodeId, bool recordUndo = true)
        {
            var node = GetNode(nodeId);
            if (node != null)
            {
                RemoveNode(node, recordUndo);
            }
        }

        public virtual void RemoveNode(Node node, bool recordUndo = true)
        {
            if (!allNodes.Contains(node))
            {
                Debug.LogWarning("Node is not part of this graph");
                return;
            }

            multiSelection.Remove(node.id);
            node.OnDestroy();

            foreach (var inConnection in node.inConnections.ToArray())
            {
                RemoveConnection(inConnection);
            }

            foreach (var outConnection in node.outConnections.ToArray())
            {
                RemoveConnection(outConnection);
            }

            if (recordUndo)
            {
                Undo.RecordObject(this, "Delete Node");
            }

            allNodes.Remove(node);
            ShowSelectionInspectorGUI();
            SetConfigDirty();
        }

        public virtual void RemoveConnection(Connection connection, bool recordUndo = true)
        {
            if (recordUndo)
            {
                Undo.RecordObject(this, "Delete Connection");
            }

            connection.OnDestroy();
            connection.sourceNode.OnOutputConnectionDisconnected(
                connection.sourceNode.outConnections.IndexOf(connection));
            connection.targetNode.OnInputConnectionDisconnected(connection.targetNode.inConnections
                .IndexOf(connection));

            connection.sourceNode.outConnections.Remove(connection);
            connection.targetNode.inConnections.Remove(connection);

            multiSelection.Remove(connection.id);
            connectionList.Remove(connection);
            ShowSelectionInspectorGUI();
            SetConfigDirty();
        }

        public virtual void ShowNodesGUI(Event e, Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos,
            float zoomFactor)
        {
            RebuildGraph();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            for (var i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].ShowNodeGUI(drawCanvas, fullDrawPass, canvasMousePos, zoomFactor);
            }
        }

        public virtual void ShowGraphControls(Event e, Vector2 canvasMousePos, bool canHandleEvent)
        {
            if(canHandleEvent)
                HandleEvents(e, canvasMousePos);

            if (PostGUI != null)
            {
                PostGUI();
                PostGUI = null;
            }

            if (GUI.changed)
            {
                SetConfigDirty();
            }
        }

        protected virtual void RebuildGraph()
        {
            if (!needRebuildGraph)
                return;

            if (connectionList != null)
            {
                for (int index = connectionList.Count - 1; index >= 0; --index)
                {
                    var conn = connectionList[index];
                    var sourceNode = GetNode(conn.sourceNodeId);
                    if (sourceNode != null)
                    {
                        conn.sourceNode = sourceNode;
                    }
                    else
                    {
                        connectionList.RemoveAt(index);
                        continue;
                    }

                    var targetNode = GetNode(conn.targetNodeId);
                    if (targetNode != null)
                    {
                        conn.targetNode = targetNode;
                    }
                    else
                    {
                        connectionList.RemoveAt(index);
                    }
                }
            }

            if (allNodes != null)
            {
                foreach (var node in allNodes)
                {
                    node.graph = this;

                    var inConnections = new List<Connection>();
                    foreach (var conn in node.inConnections)
                    {
                        foreach (var c in connectionList)
                        {
                            if (c.id == conn.id)
                            {
                                inConnections.Add(c);
                                break;
                            }
                        }
                    }
                    node.inConnections.Clear();
                    node.inConnections.AddRange(inConnections);

                    var outConnections = new List<Connection>();
                    foreach (var conn in node.outConnections)
                    {
                        foreach (var c in connectionList)
                        {
                            if (c.id == conn.id)
                            {
                                outConnections.Add(c);
                                break;
                            }
                        }
                    }
                    node.outConnections.Clear();
                    node.outConnections.AddRange(outConnections);
                }
            }
        }

        protected virtual Node selectedNode
        {
            get { return GetNode(multiSelection.Count > 0 ? multiSelection[0] : -1); }
        }

        protected virtual Connection selectedConnection
        {
            get { return GetConnection(multiSelection.Count > 0 ? multiSelection[0] : -1); }
        }

        protected virtual Node GetNode(int id)
        {
            foreach (var node in allNodes)
            {
                if (node.id == id)
                {
                    return node;
                    ;
                }
            }
            return null;
        }

        public virtual List<Node> GetAllSelectedNodes()
        {
            List<Node> nodeList = new List<Node>();
            if (multiSelection != null)
            {
                foreach (var id in multiSelection)
                {
                    var node = GetNode(id);
                    if (node != null)
                    {
                        nodeList.Add(node);
                    }
                }
            }
            return nodeList;
        }

        protected virtual Connection GetConnection(int id)
        {
            foreach (var node in allNodes)
            {
                foreach (var conn in node.inConnections)
                {
                    if (conn.id == id)
                    {
                        return conn;
                    }
                }

                foreach (var conn in node.outConnections)
                {
                    if (conn.id == id)
                    {
                        return conn;
                    }
                }
            }

            return null;
        }

        protected virtual void HandleEvents(Event e, Vector2 canvasMousePos)
        {
            if (e.type == EventType.KeyUp && GUIUtility.keyboardControl == keyboardControl)
            {
                if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
                {
                    if (multiSelection != null && multiSelection.Count > 0)
                    {
                        foreach (var id in multiSelection.ToArray())
                        {
                            var node = GetNode(id);
                            if (node != null)
                            {
                                RemoveNode(node);
                            }

                            var conn = GetConnection(id);
                            if (conn != null)
                            {
                                RemoveConnection(conn);
                            }
                        }
                        multiSelection = null;
                    }

                    if (selectedNode != null)
                    {
                        RemoveNode(selectedNode);
                    }

                    if (selectedConnection != null)
                    {
                        RemoveConnection(selectedConnection);
                    }
                    e.Use();
                }

                if (e.keyCode == KeyCode.D && e.control)
                {
                    if (multiSelection != null && multiSelection.Count > 1)
                    {
                        var idList = new List<int>();
                        CopyNodesToGraph(GetAllSelectedNodes(), idList);
                        multiSelection = idList;
                        ShowSelectionInspectorGUI();
                        return;
                    }

                    if (selectedNode != null)
                    {
                        var node = selectedNode.Duplicate(this);
                        multiSelection.Clear();        
                        multiSelection.Add(node.id);
                        ShowSelectionInspectorGUI();
                    }

                    SetConfigDirty();
                    e.Use();
                }

                if (e.keyCode == KeyCode.C && e.control)
                {
                    if (multiSelection != null && multiSelection.Count > 1)
                        CopiedNodes = GetAllSelectedNodes().ToArray();
                    else
                    {
                        var node = selectedNode;
                        if (selectedNode != null)
                            CopiedNodes = new Node[] {node};
                    }
                    e.Use();
                }

                if (e.keyCode == KeyCode.A && e.control)
                {
                    foreach (var node in allNodes)
                    {
                        if (!multiSelection.Contains(node.id))
                            multiSelection.Add(node.id);
                    }
                    ShowSelectionInspectorGUI();
                    e.Use();
                }

                if (e.keyCode == KeyCode.V && e.control && CopiedNodes != null && CopiedNodes.Length > 0 &&
                    CopiedNodes[0].GetType().IsSubclassOf(baseNodeType))
                {
                    if (CopiedNodes.Length == 1)
                    {
                        var newNode = CopiedNodes[0].Duplicate(this);
                        newNode.nodePosition = canvasMousePos;
                        multiSelection.Clear();
                        multiSelection.Add(newNode.id);
                        ShowSelectionInspectorGUI();
                    }
                    else if (CopiedNodes.Length > 1)
                    {
                        var idList = new List<int>();
                        var newNodes = CopyNodesToGraph(CopiedNodes.ToList(), idList);
                        var diff = newNodes[0].nodeRect.center - canvasMousePos;
                        newNodes[0].nodePosition = canvasMousePos;
                        for (var i = 1; i < newNodes.Count; i++)
                        {
                            newNodes[i].nodePosition -= diff;
                        }
                        multiSelection = idList;
                        ShowSelectionInspectorGUI();
                    }
                    SetConfigDirty();
                    e.Use();
                }
            }

            if (e.type == EventType.ContextClick)
            {
                var menu = GetAddNodeMenu(canvasMousePos);
                if (CopiedNodes != null && CopiedNodes.Length > 0 &&
                    CopiedNodes[0].GetType().IsSubclassOf(baseNodeType))
                {
                    menu.AddSeparator("/");
                    if (CopiedNodes.Length == 1)
                    {
                        menu.AddItem(new GUIContent(string.Format("Paste Node ({0})", CopiedNodes[0].GetType().Name)),
                            false, () =>
                            {
                                var newNode = CopiedNodes[0].Duplicate(this);
                                newNode.nodePosition = canvasMousePos;
                                multiSelection.Clear();
                                multiSelection.Add(newNode.id);
                                ShowSelectionInspectorGUI();
                                SetConfigDirty();
                            });
                    }
                    else if (CopiedNodes.Length > 1)
                    {
                        menu.AddItem(new GUIContent(string.Format("Paste Nodes ({0})", CopiedNodes.Length)), false,
                            () =>
                            {
                                var idList = new List<int>();
                                var newNodes = CopyNodesToGraph(CopiedNodes.ToList(), idList);
                                var diff = newNodes[0].nodeRect.center - canvasMousePos;
                                newNodes[0].nodePosition = canvasMousePos;
                                for (var i = 1; i < newNodes.Count; i++)
                                {
                                    newNodes[i].nodePosition -= diff;
                                }
                                multiSelection = idList;
                                ShowSelectionInspectorGUI();
                                SetConfigDirty();
                            });
                    }
                }

                menu.ShowAsContext();
                e.Use();
            }
        }

        public virtual List<Node> CopyNodesToGraph(List<Node> nodes, List<int> idList)
        {
            var newNodes = new List<Node>();
            var linkInfo = new Dictionary<Connection, KeyValuePair<int, int>>();

            foreach (var node in nodes)
            {
                var newNode = node.Duplicate(this);
                newNodes.Add(newNode);
                idList.Add(newNode.id);

                foreach (var c in node.outConnections)
                {
                    linkInfo[c] = new KeyValuePair<int, int>(nodes.IndexOf(c.sourceNode), nodes.IndexOf(c.targetNode));
                }
            }

            foreach (var linkPair in linkInfo)
            {
                if (linkPair.Value.Value != -1)
                {
                    var newSource = newNodes[linkPair.Value.Key];
                    var newTarget = newNodes[linkPair.Value.Value];
                    linkPair.Key.Duplicate(newSource, newTarget);
                }
            }

            SetConfigDirty();
            return newNodes;
        }

        public virtual bool IsSelected(int id)
        {
            return multiSelection.Contains(id);
        }

        public virtual void AddSelectedId(int id, bool clear = false)
        {
            Undo.RecordObject(this, "add selection");
            if (clear)
                multiSelection.Clear();
            multiSelection.Add(id);
        }

        public virtual void ClearSelection()
        {
            Undo.RecordObject(this, "clear selection");
            multiSelection.Clear();
        }

        public virtual bool SelectionContains(int id)
        {
            return multiSelection.Contains(id);
        }

        public virtual int SelectionCount()
        {
            return multiSelection.Count;
        }

        public virtual void RemoveSelectedId(int id)
        {
            Undo.RecordObject(this, "remove selection");
            multiSelection.Remove(id);
        }

        public virtual void SetSelection(List<int> selection)
        {
            Undo.RecordObject(this, "set selection");
            multiSelection = selection;
        }

        public virtual int[] GetSelectionArray()
        {
            return multiSelection.ToArray();
        }

        public virtual void ShowSelectionInspectorGUI()
        {
            if (!(Selection.activeObject is GraphInspectorGUI))
            {
                SetSelectionActiveObj();
            }
            else
            {
                var graphInspectorGUI = Selection.activeObject as GraphInspectorGUI;
                var id = -1;
                if (graphInspectorGUI.showNode && graphInspectorGUI.node != null)
                {
                    id = graphInspectorGUI.node.id;
                }

                if (!graphInspectorGUI.showNode && graphInspectorGUI.conn != null)
                {
                    id = graphInspectorGUI.conn.id;
                }

                if (!multiSelection.Contains(id))
                {
                    Selection.activeObject = null;
                }

                if (multiSelection.Count > 0 && multiSelection[0] != id)
                {
                    Selection.activeObject = null;
                    SetSelectionActiveObj();
                }
            }
        }

        protected virtual void SetSelectionActiveObj()
        {
            var node = selectedNode;
            if (node != null)
            {
                var go = CreateInstance<GraphInspectorGUI>();
                go.node = node;
                go.showNode = true;
                go.isShowingValidInfo = true;
                Selection.activeObject = go;
                return;
            }

            var conn = selectedConnection;
            if (conn != null)
            {
                var go = CreateInstance<GraphInspectorGUI>();
                go.conn = conn;
                go.showNode = false;
                go.isShowingValidInfo = true;
                Selection.activeObject = go;
            }

        }

        protected virtual GenericMenu GetAddNodeMenu(Vector2 canvasMousePos)
        {
            Action<Type> selected = (type) =>
            {
                multiSelection.Clear();
                multiSelection.Add(AddNode(type, canvasMousePos).id);
                ShowSelectionInspectorGUI();
            };

            var menu = EditorHelper.GetTypeSelectionMenu(baseNodeType, selected);
            menu = OnCanvasContextMenu(menu, canvasMousePos);
            return menu;
        }

        protected virtual GenericMenu OnCanvasContextMenu(GenericMenu menu, Vector2 canvasMousePos)
        {
            return menu;
        }

        public virtual Connection ConnectNodes(Node sourceNode, Node targetNode)
        {
            return ConnectNodes(sourceNode, targetNode, sourceNode.outConnections.Count);
        }

        public virtual Connection ConnectNodes(Node sourceNode, Node targetNode, int indexToInsert)
        {
            if (targetNode.IsNewConnectionAllowed(sourceNode) == false)
            {
                return null;
            }

            Undo.RecordObject(this, "New Connection");

            var newConnection = Connection.Create(sourceNode, targetNode, indexToInsert);
            newConnection.id = ++m_AutoId;
            sourceNode.OnOutputConnectionConnected(indexToInsert);
            targetNode.OnInputConnectionConnected(targetNode.inConnections.IndexOf(newConnection));
            SetConfigDirty();
            return newConnection;
        }

        public virtual void SetConfigDirty()
        {
            if (windowConfig != null)
            {
                windowConfig.SetConfigDirty();
            }
        }
    }
}