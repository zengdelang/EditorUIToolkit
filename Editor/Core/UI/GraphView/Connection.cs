using JsonFx.U3DEditor;
using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class Connection
    {
        protected enum TipConnectionStyle
        {
            None,
            Circle,
            Arrow
        }

        protected const float RELINK_DISTANCE_SNAP = 20f;

        [SerializeField] protected int m_SourceNodeId;
        [SerializeField] protected int m_TargetNodeId;
        [SerializeField] protected int m_Id;
        [SerializeField] protected bool m_InfoCollapsed;

        [NonSerialized] protected Node m_SourceNode;
        [NonSerialized] protected Node m_TargetNode;
        [NonSerialized] protected Rect m_AreaRect = new Rect(0, 0, 50, 10);
        [NonSerialized] protected Color m_ConnectionColor = new Color(0.7f, 0.7f, 1f, 0.8f);
        [NonSerialized] protected float m_LineSize = 3;

        [NonSerialized] protected Vector3 m_LineFromTangent = Vector3.zero;
        [NonSerialized] protected Vector3 m_LineToTangent = Vector3.zero;
        [NonSerialized] protected bool m_IsRelinking;
        [NonSerialized] protected Vector3 m_RelinkClickPos;

        [NonSerialized] protected Rect m_StartPortRect;
        [NonSerialized] protected Rect m_EndPortRect;

        protected virtual Color defaultColor
        {
            get { return new Color(0.7f, 0.7f, 1f, 0.8f); }
        }

        protected virtual float defaultSize
        {
            get { return 3f; }
        }

        protected virtual TipConnectionStyle tipConnectionStyle
        {
            get { return TipConnectionStyle.Circle; }
        }

        protected virtual bool canRelink
        {
            get { return true; }
        }

        protected virtual bool infoExpanded
        {
            get { return !m_InfoCollapsed; }
            set { m_InfoCollapsed = !value; }
        }

        public virtual int id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        public virtual Node sourceNode
        {
            get { return m_SourceNode; }
            set
            {
                m_SourceNode = value;
                m_SourceNodeId = m_SourceNode.id;
            }
        }

        public virtual int sourceNodeId
        {
            get { return m_SourceNodeId; }
        }

        public virtual Node targetNode
        {
            get { return m_TargetNode; }
            set
            {
                m_TargetNode = value;
                m_TargetNodeId = m_TargetNode.id;
            }
        }

        public virtual int targetNodeId
        {
            get { return m_TargetNodeId; }
        }

        protected virtual Graph graph
        {
            get { return sourceNode.graph; }
        }

        public static Connection Create(Node source, Node target, int sourceIndex)
        {
            if (source == null || target == null)
            {
                Debug.LogError("Can't Create a Connection without providing Source and Target Nodes");
                return null;
            }

            var newConnection = Activator.CreateInstance(source.outConnectionType) as Connection;
            Undo.RecordObject(source.graph, "Create Connection");
            newConnection.sourceNode = source;
            newConnection.targetNode = target;

            source.outConnections.Insert(sourceIndex, newConnection);
            target.inConnections.Add(newConnection);
            source.graph.connectionList.Add(newConnection);
            source.graph.SetConfigDirty();
            return newConnection;
        }

        public Connection Duplicate(Node newSource, Node newTarget)
        {
            if (newSource == null || newTarget == null)
            {
                Debug.LogError("Can't Duplicate a Connection without providing NewSource and NewTarget Nodes");
                return null;
            }

            Undo.RecordObject(newSource.graph, "Duplicate Connection");

            var newConnection =
                JsonReader.Deserialize(JsonWriter.Serialize(this, new JsonWriterSettings() {MaxDepth = Int32.MaxValue}),
                    true) as Connection;
            newConnection.id = newSource.graph.GetAutoId();
            newConnection.SetSource(newSource, false);
            newConnection.SetTarget(newTarget, false);

            newSource.graph.connectionList.Add(newConnection);
            newSource.graph.SetConfigDirty();
            return newConnection;
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void SetSource(Node newSource, bool isRelink = true)
        {
            Undo.RecordObject(graph, "Set Source");

            if (isRelink)
            {
                var i = sourceNode.outConnections.IndexOf(this);
                sourceNode.OnOutputConnectionDisconnected(i);
                newSource.OnOutputConnectionConnected(i);

                sourceNode.outConnections.Remove(this);
            }

            newSource.outConnections.Add(this);
            sourceNode = newSource;
            graph.SetConfigDirty();
        }

        public virtual void SetTarget(Node newTarget, bool isRelink = true)
        {
            Undo.RecordObject(graph, "Set Target");

            if (isRelink)
            {
                var i = targetNode.inConnections.IndexOf(this);
                targetNode.OnInputConnectionDisconnected(i);
                newTarget.OnInputConnectionConnected(i);

                targetNode.inConnections.Remove(this);
            }

            newTarget.inConnections.Add(this);
            targetNode = newTarget;
            if (sourceNode != null)
                graph.SetConfigDirty();
        }

        public virtual void DrawConnectionGUI(Vector3 lineFrom, Vector3 lineTo)
        {
            var mlt = 0.8f;
            var tangentX = Mathf.Abs(lineFrom.x - lineTo.x) * mlt;
            var tangentY = Mathf.Abs(lineFrom.y - lineTo.y) * mlt;

            GUI.color = defaultColor;

            m_StartPortRect = new Rect(0, 0, 12, 12);
            m_StartPortRect.center = lineFrom;

            m_EndPortRect = new Rect(0, 0, 15, 15);
            m_EndPortRect.center = lineTo;

            if (lineFrom.x <= sourceNode.nodeRect.x)
            {
                m_LineFromTangent = new Vector3(-tangentX, 0, 0);
            }

            if (lineFrom.x >= sourceNode.nodeRect.xMax)
            {
                m_LineFromTangent = new Vector3(tangentX, 0, 0);
            }

            if (lineFrom.y <= sourceNode.nodeRect.y)
                m_LineFromTangent = new Vector3(0, -tangentY, 0);

            if (lineFrom.y >= sourceNode.nodeRect.yMax)
                m_LineFromTangent = new Vector3(0, tangentY, 0);

            if (!m_IsRelinking || Vector3.Distance(m_RelinkClickPos, Event.current.mousePosition) <
                RELINK_DISTANCE_SNAP)
            {
                if (lineTo.x <= targetNode.nodeRect.x)
                {
                    m_LineToTangent = new Vector3(-tangentX, 0, 0);
                    if (tipConnectionStyle == TipConnectionStyle.Circle)
                    {
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.circle);
                    }
                    else if (tipConnectionStyle == TipConnectionStyle.Arrow)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.arrowRight);
                }

                if (lineTo.x >= targetNode.nodeRect.xMax)
                {
                    m_LineToTangent = new Vector3(tangentX, 0, 0);
                    if (tipConnectionStyle == TipConnectionStyle.Circle)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.circle);
                    else if (tipConnectionStyle == TipConnectionStyle.Arrow)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.arrowLeft);
                }

                if (lineTo.y <= targetNode.nodeRect.y)
                {
                    m_LineToTangent = new Vector3(0, -tangentY, 0);
                    if (tipConnectionStyle == TipConnectionStyle.Circle)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.circle);
                    else if (tipConnectionStyle == TipConnectionStyle.Arrow)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.arrowBottom);
                }

                if (lineTo.y >= targetNode.nodeRect.yMax)
                {
                    m_LineToTangent = new Vector3(0, tangentY, 0);
                    if (tipConnectionStyle == TipConnectionStyle.Circle)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.circle);
                    else if (tipConnectionStyle == TipConnectionStyle.Arrow)
                        GUI.Box(m_EndPortRect, string.Empty, GraphStyles.arrowTop);
                }
            }

            GUI.color = Color.white;

            HandleEvents();
            if (!m_IsRelinking || Vector3.Distance(m_RelinkClickPos, Event.current.mousePosition) <
                RELINK_DISTANCE_SNAP)
            {
                DrawConnection(lineFrom, lineTo);
                DrawInfoRect(lineFrom, lineTo);
            }

            if (GUI.changed)
                graph.SetConfigDirty();
        }

        protected virtual void DrawConnection(Vector3 lineFrom, Vector3 lineTo)
        {
            m_ConnectionColor = defaultColor;
            var highlight = graph.IsSelected(id) || graph.IsSelected(sourceNode.id) || graph.IsSelected(targetNode.id);
            m_ConnectionColor.a = highlight ? 1 : m_ConnectionColor.a;
            m_LineSize = highlight ? defaultSize + 2 : defaultSize;

            Handles.color = m_ConnectionColor;
            var shadow = new Vector3(3.5f, 3.5f, 0);
            Handles.DrawBezier(lineFrom, lineTo + shadow, lineFrom + shadow + m_LineFromTangent + shadow,
                lineTo + shadow + m_LineToTangent, new Color(0, 0, 0, 0.1f), null, m_LineSize + 10f);
            Handles.DrawBezier(lineFrom, lineTo, lineFrom + m_LineFromTangent, lineTo + m_LineToTangent,
                m_ConnectionColor, null, m_LineSize);
            Handles.color = Color.white;
        }

        /// <summary>
        /// 绘制中部显示信息
        /// </summary>
        protected virtual void DrawInfoRect(Vector3 lineFrom, Vector3 lineTo)
        {
            var t = 0.5f;
            float u = 1.0f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            Vector3 result = uuu * lineFrom;
            result += 3 * uu * t * (lineFrom + m_LineFromTangent);
            result += 3 * u * tt * (lineTo + m_LineToTangent);
            result += ttt * lineTo;
            var midPosition = (Vector2) result;
            m_AreaRect.center = midPosition;
            var alpha = (infoExpanded || graph.IsSelected(id) || graph.IsSelected(sourceNode.id)) ? 0.8f : 0.1f;
            var info = GetConnectionInfo(infoExpanded);
            var extraInfo = sourceNode.GetConnectionInfo(sourceNode.outConnections.IndexOf(this));
            if (!string.IsNullOrEmpty(info) || !string.IsNullOrEmpty(extraInfo))
            {
                if (!string.IsNullOrEmpty(extraInfo) && !string.IsNullOrEmpty(info))
                {
                    extraInfo = "\n" + extraInfo;
                }

                var textToShow = string.Format("<m_Size=9>{0}{1}</m_Size>", info, extraInfo);
                if (!infoExpanded)
                {
                    textToShow = "<m_Size=9>-||-</m_Size>";
                }
                var finalSize = GUI.skin.GetStyle("Box").CalcSize(new GUIContent(textToShow));

                m_AreaRect.width = finalSize.x;
                m_AreaRect.height = finalSize.y;

                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Box(m_AreaRect, textToShow);
                GUI.color = Color.white;
            }
            else
            {
                m_AreaRect.width = 0;
                m_AreaRect.height = 0;
            }
        }

        public virtual void ShowConnectionInspectorGUI()
        {
            var e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == 0 ||
                e.type == EventType.DragPerform ||
                e.type == EventType.KeyUp)
            {
                Undo.RegisterCompleteObjectUndo(graph, "Connection Inspector");
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
            {
                graph.PostGUI += delegate { graph.RemoveConnection(this); };
                return;
            }
            GUILayout.EndHorizontal();

            OnConnectionInspectorGUI();
            sourceNode.OnConnectionInspectorGUI(sourceNode.outConnections.IndexOf(this));

            if (GUI.changed)
            {
                graph.SetConfigDirty();
            }
        }

        protected virtual void OnConnectionInspectorGUI()
        {
        }

        /// <summary>
        /// 中部显示的信息
        /// </summary>
        protected virtual string GetConnectionInfo(bool isExpanded)
        {
            return null;
        }

        protected virtual void HandleEvents()
        {
            var e = Event.current;
            if ((graph.graphConfig.allowClick && e.type == EventType.MouseDown && e.button == 0) &&
                (m_AreaRect.Contains(e.mousePosition) || m_StartPortRect.Contains(e.mousePosition) ||
                 m_EndPortRect.Contains(e.mousePosition)))
            {
                if (canRelink)
                {
                    m_IsRelinking = true;
                    graph.isDraggingPort = true;
                    m_RelinkClickPos = e.mousePosition;
                }
                graph.AddSelectedId(id, true);
                graph.ShowSelectionInspectorGUI();
                e.Use();
                return;
            }

            if (canRelink && m_IsRelinking)
            {
                if (Vector3.Distance(m_RelinkClickPos, Event.current.mousePosition) > RELINK_DISTANCE_SNAP)
                {
                    Handles.DrawBezier(m_StartPortRect.center, e.mousePosition, m_StartPortRect.center, e.mousePosition,
                        defaultColor, null, defaultSize);
                    if (e.type == EventType.MouseUp)
                    {
                        foreach (var node in graph.allNodes)
                        {
                            if (node != targetNode && node != sourceNode && node.nodeRect.Contains(e.mousePosition) &&
                                node.IsNewConnectionAllowed())
                            {
                                SetTarget(node);
                                break;
                            }
                        }
                        m_IsRelinking = false;
                        graph.isDraggingPort = false;
                        e.Use();
                    }
                }
                else
                {
                    if (e.type == EventType.MouseUp)
                    {
                        m_IsRelinking = false;
                        graph.isDraggingPort = false;
                    }
                }
            }

            if (graph.graphConfig.allowClick && e.type == EventType.MouseDown && e.button == 1 &&
                                         m_AreaRect.Contains(e.mousePosition))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(infoExpanded ? "Collapse Info" : "Expand Info"), false,
                    () => { infoExpanded = !infoExpanded; });
                menu.AddSeparator("/");
                menu.AddItem(new GUIContent("Delete"), false, () => { graph.RemoveConnection(this); });

                graph.PostGUI += () => { menu.ShowAsContext(); };
                e.Use();
            }
        }
    }
}