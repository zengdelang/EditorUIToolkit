using System;
using System.Collections.Generic;
using System.Reflection;
using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    /// <summary>
    /// 不能使用abstract关键字，否则undo无效
    /// </summary>
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class Node
    {
        public class GUIPort
        {
            public readonly int portIndex;
            public readonly Node parent;
            public readonly Vector2 pos;

            public GUIPort(int portIndex, Node parent, Vector2 pos)
            {
                this.portIndex = portIndex;
                this.parent = parent;
                this.pos = pos;
            }
        }

        [JsonMember] [SerializeField] protected Vector2 m_Position = Vector2.zero;
        [JsonMember] [SerializeField] protected string m_CustomName;
        [JsonMember] [SerializeField] protected string m_Comment;
        [JsonMember] [SerializeField] protected int m_Id;

        [JsonMember] [SerializeField] protected List<Connection> m_InConnections = new List<Connection>();
        [JsonMember] [SerializeField] protected List<Connection> m_OutConnections = new List<Connection>();

        [JsonMember] [SerializeField] protected Color m_NodeNameColor;
        [JsonMember] [SerializeField] protected Color m_NodeColor = new Color(0.8f, 0.8f, 0.8f);

        [JsonIgnore] [NonSerialized] protected Vector2 m_Size = new Vector2(100, 20);
        [JsonIgnore] [NonSerialized] protected bool m_IsDragging;
        [JsonIgnore] [NonSerialized] protected GUIStyle m_CenterLabel;
        [JsonIgnore] [NonSerialized] protected Vector2 m_MinSize = new Vector2(100, 20);
        [JsonIgnore] [NonSerialized] protected string m_NodeName;

        public Graph graph
        {
            get; set;
        }

        public virtual List<Connection> inConnections
        {
            get { return m_InConnections; }
            protected set { m_InConnections = value; }
        }

        public virtual List<Connection> outConnections
        {
            get { return m_OutConnections; }
            protected set { m_OutConnections = value; }
        }

        public virtual Vector2 nodePosition
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public virtual int id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        public virtual string nodeComment
        {
            get { return m_Comment; }
            set { m_Comment = value; }
        }

        public virtual bool showCommentsBottom
        {
            get { return true; }
        }

        /// <summary>
        /// 最大的输入连接数量, -1代表负无穷
        /// </summary>
        public virtual int maxInConnections
        {
            get { return -1; }
        }

        public virtual int maxOutConnections
        {
            get { return -1; }
        }

        public virtual Type outConnectionType
        {
            get { return typeof(Connection); }
        }

        protected virtual string customName
        {
            get { return m_CustomName; }
            set { m_CustomName = value; }
        }

        protected virtual bool doubleClickOpenScript
        {
            get; set;
        }

        protected virtual bool showOpenScriptBtnInInspector
        {
            get; set;
        }

        protected string hexColor
        {
            get; set;
        }

        public virtual string nodeName
        {
            get
            {
                if (!string.IsNullOrEmpty(customName))
                {
                    return customName;
                }

                if (string.IsNullOrEmpty(m_NodeName))
                {
                    var nameAtt = GetType().GetAttribute<NameAttribute>(false);
                    m_NodeName = nameAtt != null ? nameAtt.name : GetType().FriendlyName().SplitCamelCase();
                }
                return m_NodeName;
            }

            set { customName = value; }
        }

        //This is to be able to work with rects which is easier in many cases.
        //Size is temporary to the node since it's auto adjusted thus no need to serialize it
        public Rect nodeRect
        {
            get { return new Rect(m_Position.x, m_Position.y, m_Size.x, m_Size.y); }
            set
            {
                m_Position = new Vector2(value.x, value.y);
                m_Size = new Vector2(Mathf.Max(value.width, m_MinSize.x), Mathf.Max(value.height, m_MinSize.y));
            }
        }

        public bool isSelected
        {
            get { return graph.SelectionContains(id); }
        }

        private GUIStyle centerLabel
        {
            get
            {
                if (m_CenterLabel == null)
                {
                    m_CenterLabel = new GUIStyle("label");
                    m_CenterLabel.alignment = TextAnchor.UpperCenter;
                    m_CenterLabel.richText = true;
                }
                return m_CenterLabel;
            }
        }

        protected Color NodeNameColor
        {
            get { return m_NodeNameColor; }
            set
            {
                if (m_NodeNameColor != value)
                {
                    m_NodeNameColor = value;
                    var temp = (Color32) value;
                    hexColor = (temp.r.ToString("X2") + temp.g.ToString("X2") + temp.b.ToString("X2")).ToLower();
                }
            }
        }

        public static Node Create(Graph targetGraph, Type nodeType, Vector2 pos)
        {
            if (targetGraph == null)
            {
                Debug.LogError("Can not Create a Node without providing a Target Graph");
                return null;
            }

            var newNode = Activator.CreateInstance(nodeType) as Node;
            Undo.RecordObject(targetGraph, "Create Node");
            newNode.graph = targetGraph;
            newNode.nodePosition = pos;
            targetGraph.SetConfigDirty();
            return newNode;
        }

        /// <summary>
        /// 设置connection的中部显示信息
        /// </summary>
        /// <param name="index">connection的索引</param>
        /// <returns></returns>
        public virtual string GetConnectionInfo(int index)
        {
            return null;
        }

        public virtual void OnConnectionInspectorGUI(int index)
        {
        }

        public virtual void ShowNodeGUI(Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos, float zoomFactor)
        {
            if (fullDrawPass || drawCanvas.Overlaps(nodeRect))
            {
                DrawNodeWindow(canvasMousePos, zoomFactor);
                DrawNodeComments();
            }

            DrawNodeConnections(drawCanvas, fullDrawPass, canvasMousePos, zoomFactor);

            if (GUI.changed)
            {
                graph.SetConfigDirty();
            }
        }

        public virtual void OnDestroy()
        {
        }

        protected virtual void DrawNodeWindow(Vector2 canvasMousePos, float zoomFactor)
        {
            GUI.color = m_NodeColor;
            var oldPos = nodePosition;
            nodeRect = GUILayout.Window(id, nodeRect, NodeWindowGUI, string.Empty, GraphStyles.window);

            GUI.color = Color.white;
            if (isSelected)
            {
                GUI.Box(nodeRect, string.Empty, GraphStyles.windowHighlight);
            }

            if (graph.graphConfig.allowClick)
            {
                EditorGUIUtility.AddCursorRect(
                    new Rect(nodeRect.x * zoomFactor, nodeRect.y * zoomFactor, nodeRect.width * zoomFactor,
                        nodeRect.height * zoomFactor), MouseCursor.Link);
            }

            if (oldPos != nodePosition)
                graph.SetConfigDirty();
        }

        protected virtual void NodeWindowGUI(int ID)
        {
            var e = Event.current;
            ShowHeader();
            HandleEvents(e);
            ShowNodeContents();
            HandleContextMenu(e);
            HandleNodePosition(e);

            if (GUI.changed)
            {
                nodeRect = new Rect(nodePosition.x, nodePosition.y, m_MinSize.x, m_MinSize.y);
                graph.SetConfigDirty();
            }
        }

        protected virtual void ShowHeader()
        {
            if (nodeName != null)
            {
                GUILayout.Label(string.Format("<b><size=12><color=#{0}>{1}</color></size></b>", hexColor, nodeName),centerLabel);
            }
        }

        protected virtual void HandleEvents(Event e)
        {
            if (e.type == EventType.MouseDown && graph.graphConfig.allowClick && e.button != 2)
            {
                if (graph.isMultiSelecting)
                    return;

                Undo.RegisterCompleteObjectUndo(graph, "Move Node");

                if (!e.control)
                {
                    if (!graph.SelectionContains(id))
                    {
                        graph.AddSelectedId(id, true);
                    }
                }

                if (e.control)
                {
                    if (isSelected)
                    {
                        graph.RemoveSelectedId(id);
                    }
                    else
                    {
                        graph.AddSelectedId(id);
                    }
                }

                graph.ShowSelectionInspectorGUI();


                if (e.button == 0 && e.clickCount == 2 && doubleClickOpenScript)
                {
                    EditorHelper.OpenScriptOfType(GetType());
                    e.Use();
                }

                OnNodePicked();
            }

            if (e.type == EventType.MouseUp)
            {
                if (!e.control && !m_IsDragging)
                {
                    graph.AddSelectedId(id, true);
                    graph.ShowSelectionInspectorGUI();
                }
                m_IsDragging = false;
                OnNodeReleased();
            }
        }

        protected virtual void ShowNodeContents()
        {
            GUI.color = Color.white;
            GUI.skin = null;
            GUI.skin.label.richText = true;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            OnNodeGUI();
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// 右键弹出菜单处理
        /// </summary>
        /// <param name="e"></param>
        protected virtual void HandleContextMenu(Event e)
        {
            var isContextClick = e.type == EventType.MouseUp && e.button == 1;
            if (graph.graphConfig.allowClick && isContextClick)
            {
                if (graph.SelectionCount() > 1)
                {
                    //多个结点被选中的处理
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Duplicate Selected Nodes"), false, () =>
                    {
                        var idList = new List<int>();
                        graph.CopyNodesToGraph(graph.GetAllSelectedNodes(), idList);
                        graph.SetSelection(idList);
                        graph.ShowSelectionInspectorGUI();
                    });
                    menu.AddItem(new GUIContent("Copy Selected Nodes"), false,
                        () => { graph.CopiedNodes = graph.GetAllSelectedNodes().ToArray(); });
                    menu.AddSeparator("/");
                    menu.AddItem(new GUIContent("Delete Selected Nodes"), false, () =>
                    {
                        foreach (var nodeId in graph.GetSelectionArray())
                            graph.RemoveNode(nodeId);
                    });
                    graph.PostGUI += () => { menu.ShowAsContext(); };
                    e.Use();
                }
                else
                {
                    //单个结点被选中的处理
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Duplicate (CTRL+D)"), false, () =>
                    {
                        graph.AddSelectedId(Duplicate(graph).id, true);
                        graph.ShowSelectionInspectorGUI();
                    });
                    menu.AddItem(new GUIContent("Copy Node"), false,
                        () => { graph.CopiedNodes = new Node[] {this}; });

                    menu = OnContextMenu(menu);
                    if (menu != null)
                    {
                        menu.AddSeparator("/");
                        menu.AddItem(new GUIContent("Delete (DEL)"), false, () => { graph.RemoveNode(this); });
                        graph.PostGUI += () => { menu.ShowAsContext(); };
                    }
                    e.Use();
                }
            }
        }

        protected virtual void HandleNodePosition(Event e)
        {
            if (graph.graphConfig.allowClick && e.button != 2)
            {
                if (e.type == EventType.MouseDrag)
                {
                    if (graph.isDraggingPort || graph.isMultiSelecting)
                    {
                        return;
                    }

                    if (!graph.SelectionContains(id))
                    {
                        graph.AddSelectedId(id);
                    }

                    if (graph.SelectionCount() > 1)
                    {
                        var nodeList = graph.GetAllSelectedNodes();
                        foreach (var node in nodeList)
                        {
                            node.nodePosition += e.delta;
                        }
                        m_IsDragging = true;
                        graph.SetConfigDirty();
                        return;
                    }
                }
                GUI.DragWindow();
            }
        }

        protected virtual void DrawNodeComments()
        {
            if (!string.IsNullOrEmpty(nodeComment))
            {
                var commentsRect = new Rect();
                var size = GraphStyles.textArea.CalcSize(new GUIContent(nodeComment));

                if (showCommentsBottom)
                {
                    size.y = GraphStyles.textArea.CalcHeight(new GUIContent(nodeComment), nodeRect.width);
                    commentsRect = new Rect(nodeRect.x, nodeRect.yMax + 5, nodeRect.width, size.y);
                }
                else
                {
                    commentsRect = new Rect(nodeRect.xMax + 5, nodeRect.yMin, Mathf.Min(size.x, nodeRect.width * 2),
                        nodeRect.height);
                }

                GUI.color = new Color(1, 1, 1, 0.6f);
                GUI.backgroundColor = new Color(1f, 1f, 1f, 0.2f);
                GUI.Box(commentsRect, nodeComment, GraphStyles.textArea);
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
            }
        }

        public Node Duplicate(Graph targetGraph)
        {
            if (targetGraph == null)
            {
                Debug.LogError("Can't duplicate a Node without providing a Target Graph");
                return null;
            }

            Undo.RecordObject(targetGraph, "Duplicate");

            var newNode =
                JsonReader.Deserialize(JsonWriter.Serialize(this, new JsonWriterSettings() {MaxDepth = Int32.MaxValue}),
                    true) as Node;
            newNode.id = targetGraph.GetAutoId();
            targetGraph.allNodes.Add(newNode);
            newNode.inConnections.Clear();
            newNode.outConnections.Clear();
            newNode.nodePosition += new Vector2(50, 50);
            newNode.graph = targetGraph;
            targetGraph.SetConfigDirty();
            return newNode;
        }

        public virtual void ShowNodeInspectorGUI()
        {
            var e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == 0 ||
                e.type == EventType.DragPerform ||
                e.type == EventType.KeyUp)
            {
                Undo.RegisterCompleteObjectUndo(graph, "Node Inspector");
            }

            var descAtt = GetType().GetAttribute<DescriptionAttribute>(true);
            var description = descAtt != null ? descAtt.description : null;
            if (string.IsNullOrEmpty(description))
            {
                EditorGUILayout.HelpBox(GetType().FriendlyName(), MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("{0} : {1}", GetType().FriendlyName(), description), MessageType.Info);
            }

            GUILayout.BeginHorizontal();
            {
                nodeName = EditorGUILayout.TextField("Node Name", nodeName);
                NodeNameColor = EditorGUILayout.ColorField(NodeNameColor, GUILayout.Width(30));
            }
            GUILayout.EndHorizontal();
            m_NodeColor = EditorGUILayout.ColorField("Node Color", m_NodeColor);
            OnNodeInspectorGUI();

            if (showOpenScriptBtnInInspector)
            {
                if (GUILayout.Button("Open Node Script"))
                {
                    EditorHelper.OpenScriptOfType(GetType());
                    e.Use();
                }
            }

            nodeComment = EditorGUILayout.TextArea(nodeComment);
            EditorInspectorGUIUtility.TextFieldComment(nodeComment, "Comments...");

            if (GUI.changed)
            {
                nodeRect = new Rect(nodePosition.x, nodePosition.y, m_MinSize.x, m_MinSize.y);
                graph.SetConfigDirty();
            }
        }

        protected void DrawDefaultInspector()
        {
            foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                field.SetValue(this, EditorInspectorGUIUtility.GenericField(field.Name, field.GetValue(this), field.FieldType, field, this));
                GUI.backgroundColor = Color.white;
            }
        }

        protected virtual void OnNodePicked()
        {
        }

        protected virtual void OnNodeReleased()
        {
        }

        protected virtual void OnNodeGUI()
        {
        }

        protected virtual void OnNodeInspectorGUI()
        {
            DrawDefaultInspector();
        }

        protected virtual GenericMenu OnContextMenu(GenericMenu menu)
        {
            return menu;
        }

        protected virtual void DrawNodeConnections(Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos,
            float zoomFactor)
        {
            var e = Event.current;
            if (graph.clickedPort != null && e.type == EventType.MouseUp && e.button == 0)
            {
                if (nodeRect.Contains(e.mousePosition))
                {
                    graph.ConnectNodes(graph.clickedPort.parent, this, graph.clickedPort.portIndex);
                    graph.clickedPort = null;
                    graph.isDraggingPort = false;
                    e.Use();
                }
                else
                {
                    graph.dragDropMisses++;
                    if (graph.dragDropMisses == graph.allNodes.Count && graph.clickedPort != null)
                    {
                        var source = graph.clickedPort.parent;
                        var index = graph.clickedPort.portIndex;
                        var pos = e.mousePosition;
                        graph.clickedPort = null;
                        graph.isDraggingPort = false;

                        Action<Type> Selected = (type) =>
                        {
                            var newNode = graph.AddNode(type, pos);
                            graph.ConnectNodes(source, newNode, index);
                            graph.AddSelectedId(newNode.id, true);
                            graph.ShowSelectionInspectorGUI();
                        };

                        var menu = EditorHelper.GetTypeSelectionMenu(graph.baseNodeType, Selected);
                        graph.PostGUI += () => { menu.ShowAsContext(); };
                        e.Use();
                    }
                }
            }

            if (maxOutConnections == 0)
            {
                return;
            }

            if (fullDrawPass || drawCanvas.Overlaps(nodeRect))
            {
                if (outConnections.Count < maxOutConnections || maxOutConnections == -1)
                {
                    for (var i = 0; i < outConnections.Count + 1; i++)
                    {
                        var portRect = new Rect(0, 0, 10, 10);
                        portRect.center =
                            new Vector2(((nodeRect.width / (outConnections.Count + 1)) * (i + 0.5f)) + nodeRect.xMin,
                                nodeRect.yMax + 3);
                        var old = GUI.color;

                        if (portRect.Contains(e.mousePosition))
                        {
                            GUI.color = new Color(1, 0.4f, 0);
                        }

                        GUI.DrawTexture(portRect, GraphStyles.ConnectionPoint);
                        GUI.color = old;
                        if (graph.graphConfig.allowClick)
                        {
                            EditorGUIUtility.AddCursorRect(portRect, MouseCursor.ArrowPlus);

                            if (e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition))
                            {
                                graph.dragDropMisses = 0;
                                graph.clickedPort = new GUIPort(i, this, portRect.center);
                                graph.isDraggingPort = true;
                                e.Use();
                            }
                        }
                    }
                }
            }

            if (graph.clickedPort != null && graph.clickedPort.parent == this)
            {
                var yDiff = (graph.clickedPort.pos.y - e.mousePosition.y) * 0.5f;
                yDiff = e.mousePosition.y > graph.clickedPort.pos.y ? -yDiff : yDiff;
                var tangA = new Vector2(0, yDiff);
                var tangB = tangA * -1;
                Handles.DrawBezier(graph.clickedPort.pos, e.mousePosition, graph.clickedPort.pos + tangA,
                    e.mousePosition + tangB,
                    new Color(0.5f, 0.5f, 0.8f, 0.8f), null, 3);
                m_IsDragging = false;
            }

            for (var i = 0; i < outConnections.Count; i++)
            {
                var connection = outConnections[i];
                if (connection != null)
                {
                    var sourcePos =
                        new Vector2(((nodeRect.width / (outConnections.Count + 1)) * (i + 1)) + nodeRect.xMin,
                            nodeRect.yMax + 3);
                    var targetPos = new Vector2(connection.targetNode.nodeRect.center.x,
                        connection.targetNode.nodeRect.y);

                    var sourcePortRect = new Rect(0, 0, 10, 10);
                    sourcePortRect.center = sourcePos;

                    var targetPortRect = new Rect(0, 0, 15, 15);
                    targetPortRect.center = targetPos;

                    var boundRect = RectUtility.GetBoundRect(sourcePortRect, targetPortRect);
                    if (fullDrawPass || drawCanvas.Overlaps(boundRect))
                    {
                        var oldColor = GUI.color;
                        GUI.color = new Color(0.7f, 0.7f, 1f, 0.8f);
                        GUI.DrawTexture(sourcePortRect, GraphStyles.ConnectionPoint);
                        GUI.color = oldColor;
                        connection.DrawConnectionGUI(sourcePos, targetPos);

                        if (graph.graphConfig.allowClick)
                        {
                            if (e.type == EventType.ContextClick && sourcePortRect.Contains(e.mousePosition))
                            {
                                graph.RemoveConnection(connection);
                                e.Use();
                                return;
                            }

                            if (e.type == EventType.ContextClick && targetPortRect.Contains(e.mousePosition))
                            {
                                graph.RemoveConnection(connection);
                                e.Use();
                                return;
                            }
                        }
                    }
                }
            }
        }

        public virtual void OnInputConnectionConnected(int connectionIndex)
        {
        }

        public virtual void OnInputConnectionDisconnected(int connectionIndex)
        {
        }

        public virtual void OnOutputConnectionConnected(int connectionIndex)
        {
        }

        public virtual void OnOutputConnectionDisconnected(int connectionIndex)
        {
        }

        public virtual bool IsNewConnectionAllowed()
        {
            return IsNewConnectionAllowed(null);
        }

        public virtual bool IsNewConnectionAllowed(Node sourceNode)
        {
            if (sourceNode != null)
            {
                if (this == sourceNode)
                {
                    Debug.LogWarning("Node can't connect to itself");
                    return false;
                }

                if (sourceNode.outConnections.Count >= sourceNode.maxOutConnections &&
                    sourceNode.maxOutConnections != -1)
                {
                    Debug.LogWarning("Source node can have no more out connections.");
                    return false;
                }
            }

            if (maxInConnections == 1)
            {
                Debug.LogWarning("Target node can have no more connections");
                return false;
            }

            if (maxInConnections <= inConnections.Count && maxInConnections != -1)
            {
                Debug.LogWarning("Target node can have no more connections");
                return false;
            }
            return true;
        }
    }
}