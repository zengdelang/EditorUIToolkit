using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class GraphView : View
    {
        public static readonly float   unityTabHeight = 22;
        public static readonly int     gridSize = 15;
        public static readonly Vector2 virtualCenterOffset = new Vector2(-5000, -5000);

        protected EditorWindowConfigSource m_WindowConfig;
        protected Graph   m_Graph;
        protected Rect    m_ClientAreaRect;
        protected Rect    m_ViewRect;
        protected bool    m_FullDrawPass = true;

        protected Vector2? m_SmoothPan;
        protected float?   m_SmoothZoomFactor;
        protected Vector2  m_PanVelocity = Vector2.one;
        protected float    m_ZoomVelocity = 1;

        protected bool    m_IsMultiSelecting;
        protected Vector2 m_SelectionStartPos;

        protected Node[]  m_TempGroupNodes;
        protected NodeGroup[] m_TempNestedGroups;

        public Graph currentGraph
        {
            get { return m_Graph; }
            set
            {
                m_Graph = value;
                if(m_Graph != null)
                    m_Graph.windowConfig = m_WindowConfig;
            }
        }

        protected Vector2 pan
        {
            get { return currentGraph != null ? Vector2.Min(currentGraph.translation, Vector2.zero) : virtualCenter; }
            set
            {
                if (currentGraph != null)
                {
                    var t = currentGraph.translation;
                    t = Vector2.Min(value, Vector2.zero);
                    if (m_SmoothPan == null)
                    {
                        t.x = Mathf.Round(t.x); //pixel perfect correction
                        t.y = Mathf.Round(t.y); //pixel perfect correction
                    }
                    currentGraph.translation = t;
                    currentGraph.SetConfigDirty();
                }
            }
        }

        protected float zoomFactor
        {
            get { return currentGraph != null ? Mathf.Clamp(currentGraph.zoomFactor, 0.25f, 1f) : 1f; }
            set
            {
                if (currentGraph != null)
                {
                    currentGraph.zoomFactor = Mathf.Clamp(value, 0.25f, 1f);
                    currentGraph.SetConfigDirty();
                }
            }
        }

        protected Vector2 virtualCenter
        {
            get { return -virtualCenterOffset + m_ViewRect.size / 2; }
        }

        protected Vector2 mousePosInCanvas
        {
            get { return ViewSpaceToCanvasSpace(Event.current.mousePosition); }
        }

        public GraphView(ViewGroupManager owner, EditorWindowConfigSource windowConfig) : base(owner)
        {
            m_WindowConfig = windowConfig;
            Undo.undoRedoPerformed += UndoRedoPerformedAction;
        }

        public GraphView(ViewGroupManager owner) : base(owner)
        {
            Undo.undoRedoPerformed += UndoRedoPerformedAction;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();           
            Undo.undoRedoPerformed -= UndoRedoPerformedAction;
        }

        private void UndoRedoPerformedAction()
        {
            m_WindowConfig.SetConfigDirty();
            if (currentGraph != null)
            {
                currentGraph.needRebuildGraph = true;
            }
        }

        protected Vector2 ViewSpaceToCanvasSpace(Vector2 viewPos)
        {
            viewPos -= new Vector2(m_ClientAreaRect.x, m_ClientAreaRect.y);
            return (viewPos - pan) / zoomFactor;
        }

        protected Vector2 CanvasSpaceToViewSpace(Vector2 canvasPos)
        {
            return (canvasPos * zoomFactor) + pan + new Vector2(m_ClientAreaRect.x, m_ClientAreaRect.y); ;
        }

        public override void Update()
        {
            DoSmoothPan();
            DoSmoothZoom();
        }

        protected void DoSmoothPan()
        {
            if (m_SmoothPan == null)
            {
                return;
            }

            var targetPan = (Vector2)m_SmoothPan;
            if ((targetPan - pan).magnitude < 0.1f)
            {
                m_SmoothPan = null;
                return;
            }

            pan = Vector2.SmoothDamp(pan, targetPan, ref m_PanVelocity, 0.05f, Mathf.Infinity, Application.isPlaying ? Time.deltaTime : 1f / 200);
            Repaint();
        }

        protected void DoSmoothZoom()
        {
            if (m_SmoothZoomFactor == null)
            {
                return;
            }

            var targetZoom = (float)m_SmoothZoomFactor;
            if (Mathf.Abs(targetZoom - zoomFactor) < 0.00001f)
            {
                m_SmoothZoomFactor = null;
                return;
            }

            zoomFactor = Mathf.SmoothDamp(zoomFactor, targetZoom, ref m_ZoomVelocity, 0.05f, Mathf.Infinity, Application.isPlaying ? Time.deltaTime : 1f / 200);
            if (zoomFactor > 0.99999f)
            {
                zoomFactor = 1;
            }
            Repaint();
        }

        public override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            Repaint();
        }

        public override void OnGUI(Rect rect)
        {        
            base.OnGUI(rect);

            m_ClientAreaRect = rect;

            if (Event.current.type == EventType.Repaint)
            {
                UnityEditor.Graphs.Styles.graphBackground.Draw(m_ClientAreaRect, false, false, false, false);  
            }
            m_ClientAreaRect.y += unityTabHeight;
          
            var keyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            var canHandleEvent = HandleEvents(keyboardControlID);

            Matrix4x4 oldMatrix;

            m_ClientAreaRect = StartZoomArea(m_ClientAreaRect, out oldMatrix);
            GUI.BeginGroup(m_ClientAreaRect);

            var totalCanvas = m_ClientAreaRect;
            totalCanvas.x = pan.x / zoomFactor;
            totalCanvas.y = pan.y / zoomFactor;
            //totalCanvas.width = canvasRect.width  已经/zoomFactor
            //实际根据pan来增加宽度，向右移动，pan.x是负的
            totalCanvas.width -= pan.x / zoomFactor;
            totalCanvas.height -= pan.y / zoomFactor;

            GUI.BeginGroup(totalCanvas);

            m_ViewRect = totalCanvas;
            m_ViewRect.x = -pan.x / zoomFactor;
            m_ViewRect.y = -pan.y / zoomFactor;
            m_ViewRect.width += pan.x / zoomFactor;
            m_ViewRect.height += pan.y / zoomFactor;

            DrawGrid(m_ViewRect, pan, zoomFactor);

            if (currentGraph != null)
            {
                currentGraph.keyboardControl = keyboardControlID;

                DoNodeGroups(Event.current);

                Owner.WindowOwner.BeginWindows();
                currentGraph.ShowNodesGUI(Event.current, m_ViewRect, m_FullDrawPass, mousePosInCanvas, zoomFactor);
                Owner.WindowOwner.EndWindows();
            }

            DoCanvasRectSelection(m_ViewRect, Event.current);
            GUI.EndGroup();

            GUI.EndGroup();
            EndZoomArea(oldMatrix);

            if (currentGraph != null)
                currentGraph.ShowGraphControls(Event.current, mousePosInCanvas, canHandleEvent);

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }

        protected void DrawGrid(Rect container, Vector2 offset, float zoomFactor)
        {
            Handles.matrix = Matrix4x4.identity;
            var scaledX = (container.width - offset.x) / zoomFactor;
            var scaledY = (container.height - offset.y) / zoomFactor;
            for (var i = 0 - (int)offset.x; i < scaledX; i++)
            {
                if (i % gridSize == 0)
                {
                    Handles.color = new Color(0, 0, 0, i % (gridSize * 5) == 0 ? 0.2f : 0.1f);
                    Handles.DrawLine(new Vector3(i, 0, 0), new Vector3(i, scaledY, 0));
                }
            }

            for (var i = 0 - (int)offset.y; i < scaledY; i++)
            {
                if (i % gridSize == 0)
                {
                    Handles.color = new Color(0, 0, 0, i % (gridSize * 5) == 0 ? 0.2f : 0.1f);
                    Handles.DrawLine(new Vector3(0, i, 0), new Vector3(scaledX, i, 0));
                }
            }

            Handles.color = Color.white;
        }

        protected Rect StartZoomArea(Rect container, out Matrix4x4 oldMatrix)
        {
            GUI.EndGroup();

            container.width /= zoomFactor;
            container.height /= zoomFactor;

            oldMatrix = GUI.matrix;
            var matrix1 = Matrix4x4.TRS(new Vector3(container.x, container.y), Quaternion.identity, Vector3.one);
            var matrix2 = Matrix4x4.Scale(new Vector3(zoomFactor, zoomFactor, zoomFactor));
            GUI.matrix = matrix1 * matrix2 * matrix1.inverse * GUI.matrix;
            return container;
        }

        protected void EndZoomArea(Matrix4x4 oldMatrix)
        {
            GUI.matrix = oldMatrix;
            var zoomRecoveryRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, Screen.height);
            GUI.BeginGroup(zoomRecoveryRect, GUIStyle.none);
        }

        protected bool HandleEvents(int keyboardControlID)
        {
            Event e = Event.current;
            var rect = m_ClientAreaRect;
            rect.y -= unityTabHeight;
            if (!rect.Contains(e.mousePosition))
            {
                return false;
            }

            if (Event.current.type == EventType.MouseDown)
            {
                GUIUtility.keyboardControl = keyboardControlID;
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F && GUIUtility.keyboardControl == keyboardControlID)
            {
                if (currentGraph.allNodes.Count > 0)
                {
                    FocusPosition(GetNodeBounds(currentGraph.allNodes, m_ViewRect).center);
                }
                else
                {
                    FocusPosition(virtualCenter);
                }
            }

            if (e.type == EventType.MouseDown && e.button == 2 && e.clickCount == 2)
            {
                FocusPosition(ViewSpaceToCanvasSpace(e.mousePosition));
            }

            if (e.type == EventType.ScrollWheel)
            {
                var zoomDelta = e.shift ? 0.1f : 0.25f;
                ZoomAt(e.mousePosition, -e.delta.y > 0 ? zoomDelta : -zoomDelta);
            }

            if ((e.button == 2 && e.type == EventType.MouseDrag)
                || (e.type == EventType.MouseDrag) && e.alt && e.isMouse)
            {
                pan += e.delta;
                m_SmoothPan = null;
                m_SmoothZoomFactor = null;
                e.Use();
            }

            return true;
        }
 
        protected void DoCanvasRectSelection(Rect container, Event e)
        {
            if (currentGraph == null)
                return;


            if (currentGraph.graphConfig.allowClick &&  e.type == EventType.MouseDown && e.button == 0 && !e.alt && !e.shift && m_ClientAreaRect.Contains(CanvasSpaceToViewSpace(e.mousePosition)))
            {
                currentGraph.ClearSelection();
                currentGraph.ShowSelectionInspectorGUI();
                m_SelectionStartPos = e.mousePosition;
                m_IsMultiSelecting = true;
                currentGraph.isMultiSelecting = true;
                e.Use();
            }

            if (m_IsMultiSelecting && e.rawType == EventType.MouseUp)
            {
                var rect = GetSelectionRect(m_SelectionStartPos, e.mousePosition);
                var overlapedNodes = currentGraph.allNodes.Where(n => rect.Overlaps(n.nodeRect)).ToList();
                m_IsMultiSelecting = false;
                currentGraph.isMultiSelecting = false;

                if (e.control && rect.width > 50 && rect.height > 50)
                {
                    Undo.RegisterCompleteObjectUndo(currentGraph, "Create Group");
                    if (currentGraph.nodeGroups == null)
                    { 
                        currentGraph.nodeGroups = new List<NodeGroup>();
                    }
                    currentGraph.nodeGroups.Add(new NodeGroup(rect, "New Node Group"));
                    currentGraph.SetConfigDirty();
                }
                else
                {
                    if (overlapedNodes.Count > 0)
                    {
                        List<int> idList = new List<int>();
                        foreach (var item in overlapedNodes.Cast<object>().ToList())
                        {
                            var node = item as Node;
                            if (node != null)
                                idList.Add(node.id);
                        }
                        currentGraph.SetSelection(idList);
                        currentGraph.ShowSelectionInspectorGUI();
                        e.Use();
                    }
                }
            }

            if (m_IsMultiSelecting)
            {
                var rect = GetSelectionRect(m_SelectionStartPos, e.mousePosition);
                if (rect.width > 5 && rect.height > 5)
                {
                    GUI.Box(rect, string.Empty, GraphStyles.selectionRect);
                    GUI.color = new Color(1, 0.4f, 0, 0.7f);
                    foreach (var node in currentGraph.allNodes)
                    {
                        if (rect.Overlaps(node.nodeRect))
                        {
                            var highlightRect = node.nodeRect;
                            GUI.Box(highlightRect, string.Empty, GraphStyles.selection);
                        }
                    }
                    if (rect.width > 50 && rect.height > 50)
                    {
                        GUI.color = new Color(1, 1, 1, e.control ? 0.6f : 0.15f);
                        GUI.Label(new Rect(e.mousePosition.x + 16, e.mousePosition.y, 120, 22), "<i>+ control for group</i>");
                    }
                }
            }

            GUI.color = Color.white;
        }

        protected Rect GetSelectionRect(Vector2 startPos, Vector2 endPos)
        {
            var num1 = (startPos.x < endPos.x) ? startPos.x : endPos.x;
            var num2 = (startPos.x > endPos.x) ? startPos.x : endPos.x;
            var num3 = (startPos.y < endPos.y) ? startPos.y : endPos.y;
            var num4 = (startPos.y > endPos.y) ? startPos.y : endPos.y;
            return new Rect(num1, num3, num2 - num1, num4 - num3);
        }

        protected void FocusPosition(Vector2 targetPos)
        {
            m_SmoothPan = -targetPos;
            m_SmoothPan += new Vector2(m_ViewRect.width / 2, m_ViewRect.height / 2);
            m_SmoothPan *= zoomFactor;
        }

        protected void ZoomAt(Vector2 center, float delta)
        {
            var pinPoint = (center - pan) / zoomFactor;
            var newZ = zoomFactor;
            newZ += delta;
            newZ = Mathf.Clamp(newZ, 0.25f, 1f);
            m_SmoothZoomFactor = newZ;
            var a = (pinPoint * newZ) + pan;
            var b = center;
            var diff = b - a;
            m_SmoothPan = pan + diff;
        }

        protected Rect GetNodeBounds(List<Node> nodes, Rect container, bool expandToContainer = false)
        {
            if (nodes == null)
            {
                return container;
            }

            var minX = float.PositiveInfinity;
            var minY = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var maxY = float.NegativeInfinity;

            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null)
                {
                    minX = Mathf.Min(minX, nodes[i].nodeRect.xMin);
                    minY = Mathf.Min(minY, nodes[i].nodeRect.yMin);
                    maxX = Mathf.Max(maxX, nodes[i].nodeRect.xMax);
                    maxY = Mathf.Max(maxY, nodes[i].nodeRect.yMax);
                }
            }

            minX -= 20;
            minY -= 20;
            maxX += 20;
            maxY += 20;

            if (expandToContainer)
            {
                minX = Mathf.Min(minX, container.xMin + 20);
                minY = Mathf.Min(minY, container.yMin + 20);
                maxX = Mathf.Max(maxX, container.xMax - 20);
                maxY = Mathf.Max(maxY, container.yMax - 20);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        void DoNodeGroups(Event e)
        {
            if (currentGraph.nodeGroups == null)
            {
                return;
            }

            for (var i = 0; i < currentGraph.nodeGroups.Count; i++)
            {
                var group = currentGraph.nodeGroups[i];
                var handleRect = new Rect(group.rect.x, group.rect.y, group.rect.width, 25);
                var scaleRect = new Rect(group.rect.xMax - 20, group.rect.yMax - 20, 20, 20);
                var style = (GUIStyle)"box";
                style.richText = true;

                GUI.color = new Color(1, 1, 1, 0.4f);
                GUI.Box(group.rect, string.Empty, style);

                if (group.color != default(Color))
                {
                    GUI.color = group.color;
                    var r = group.rect;
                    r.x += 1;
                    r.width -= 2;
                    GUI.DrawTexture(r, EditorGUIUtility.whiteTexture);
                }

                GUI.color = Color.white;
                GUI.Box(new Rect(scaleRect.x + 10, scaleRect.y + 10, 6, 6), string.Empty, GraphStyles.scaleArrow);

                var size = 14 / zoomFactor;
                var name = string.Format("<size={0}>{1}</size>", size, group.name);
                GUI.Label(handleRect, name, style);

                EditorGUIUtility.AddCursorRect(handleRect, group.isRenaming ? MouseCursor.Text : MouseCursor.Link);
                EditorGUIUtility.AddCursorRect(scaleRect, MouseCursor.ResizeUpLeft);

                if (group.isRenaming)
                {
                    GUI.SetNextControlName("GroupRename");
                    group.name = EditorGUI.TextField(handleRect, group.name, style);
                    GUI.FocusControl("GroupRename");
                    if (e.keyCode == KeyCode.Return || (e.type == EventType.MouseDown && !handleRect.Contains(e.mousePosition)))
                    {
                        group.isRenaming = false;
                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                    }
                }

                if (e.type == EventType.MouseDown && currentGraph.graphConfig.allowClick)
                {                
                    if (handleRect.Contains(e.mousePosition))
                    {
                        Undo.RegisterCompleteObjectUndo(currentGraph, "Move Node Group");

                        m_TempGroupNodes = currentGraph.allNodes.Where(n => group.rect.Encapsulates(n.nodeRect)).ToArray();
                        m_TempNestedGroups = currentGraph.nodeGroups.Where(c => group.rect.Encapsulates(c.rect)).ToArray();

                        if (e.button == 1)
                        {
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Show In Inspector"), false, () =>
                            {
                                var obj = ScriptableObject.CreateInstance<NodeGroupInspectorGUI>();
                                obj.nodeGroup = group;
                                obj.SetDirtyAction = currentGraph.SetConfigDirty;
                                Selection.activeObject = obj;
                            });
                            menu.AddItem(new GUIContent("Select Nodes"), false, () =>
                            {
                                List<int> selectNodeIdList = new List<int>();
                                foreach (var node in m_TempGroupNodes)
                                {
                                    selectNodeIdList.Add(node.id);
                                }
                                currentGraph.SetSelection(selectNodeIdList);
                            });
                            menu.AddItem(new GUIContent("Delete Group"), false, () =>
                            {
                                currentGraph.nodeGroups.Remove(group);
                                currentGraph.SetConfigDirty();
                            });
                            currentGraph.PostGUI += () => { menu.ShowAsContext(); };
                        }
                        else if (e.button == 0)
                        {
                            group.isDragging = true;
                        }

                        e.Use();
                    }

                    if (e.button == 0 && scaleRect.Contains(e.mousePosition))
                    {
                        Undo.RegisterCompleteObjectUndo(currentGraph, "Scale Node Group");
                        group.isRescaling = true;
                        e.Use();
                    }
                }

                if (e.rawType == EventType.MouseUp)
                {
                    group.isDragging = false;
                    group.isRescaling = false;
                    currentGraph.SetConfigDirty();
                }

                if (e.type == EventType.MouseDrag)
                {
                    if (group.isDragging)
                    {
                        group.rect.x += e.delta.x;
                        group.rect.y += e.delta.y;

                        if (!e.shift)
                        {
                            if (m_TempGroupNodes != null)
                            {
                                for (var j = 0; j < m_TempGroupNodes.Length; j++)
                                {
                                    m_TempGroupNodes[j].nodePosition += e.delta;
                                }
                            }

                            if (m_TempNestedGroups != null)
                            {
                                for (var j = 0; j < m_TempNestedGroups.Length; j++)
                                {
                                    m_TempNestedGroups[j].rect.x += e.delta.x;
                                    m_TempNestedGroups[j].rect.y += e.delta.y;
                                }
                            }                      
                        }
                    }

                    if (group.isRescaling)
                    {
                        group.rect.xMax = Mathf.Max(e.mousePosition.x + 5, group.rect.x + 100);
                        group.rect.yMax = Mathf.Max(e.mousePosition.y + 5, group.rect.y + 100);
                    }
                }
            }
        }
    }
}