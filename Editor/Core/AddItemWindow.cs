using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EUTK
{
    /*  使用方法一:
    GUIContent label = new GUIContent("设置");
    Rect rect = GUILayoutUtility.GetRect(label, "Button"); 
    if (GUI.Button(rect, label))
    {
        //var deltaX = (rect.width - 230) / 2;
        //rect.x += deltaX;
        //rect.width = 230;
        if (AddItemWindow.Show(rect, "ssss"))
        {
            AddItemWindow.s_AddItemWindow.Init(rect,
                ItemPathGenerator.GetItemInfo("D:\\GitHub\\test\\Assets", ".txt").pathArray, null, null, null);
        }
    }
*/
    /*  使用方法二:
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIContent addComponentLabel = new GUIContent("Add Component");
        Rect rect = GUILayoutUtility.GetRect(addComponentLabel, "AC Button");
        if (EditorGUI.DropdownButton(rect, addComponentLabel, FocusType.Passive, "AC Button") && AddItemWindow.Show(rect, "ssss"))
        {
            //初始化不能放在Show里面，GetItemInfo的操作可能超过50ms，这样再次点击就会又立刻显示出来
            AddItemWindow.s_AddItemWindow.Init(rect,
                ItemPathGenerator.GetItemInfo("D:\\GitHub\\test\\Assets", ".txt").pathArray, null, null, null);
            GUIUtility.ExitGUI();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
     */
    public class AddItemWindow : EditorWindow
    {
        private List<GroupElement> m_Stack = new List<GroupElement>();

        private float m_Anim = 1f;
        private int m_AnimTarget = 1;
        private long m_LastTime;
        private bool m_ScrollToSelected;
        private string m_DelayedSearch;
        private string m_Search = "";

        private bool m_DirtyList;

        private const int kHeaderHeight = 30;
        private const int kWindowHeight = 320;

        private static string s_ItemSearchKey = "";

        private static Styles s_Styles;
        private static long s_LastClosedTime;
        public static AddItemWindow s_AddItemWindow;

        private string[] m_ItemPathArray;
        private Texture2D[] m_ItemTexture2DArray;
        private object[] m_ItemInfoArray;
        private Action<string, object> m_ClickedAction;

        private Element[] m_Tree;
        private Element[] m_SearchResultTree;

        private const string kSearchHeader = "Search";

        private bool hasSearch
        {
            get { return !string.IsNullOrEmpty(m_Search); }
        }

        private GroupElement activeParent
        {
            get { return m_Stack[m_Stack.Count - 2 + m_AnimTarget]; }
        }

        private Element[] activeTree
        {
            get { return !hasSearch ? m_Tree : m_SearchResultTree; }
        }

        private Element activeElement
        {
            get
            {
                if (activeTree == null)
                    return null;
                List<Element> children = GetChildren(activeTree, activeParent);
                if (children.Count == 0)
                    return null;
                return children[activeParent.selectedIndex];
            }
        }

        private bool isAnimating
        {
            get { return m_Anim != m_AnimTarget; }
        }

        private void OnEnable()
        {
            m_DirtyList = true;
            m_Search = EditorPrefs.GetString(s_ItemSearchKey, "");
        }

        private void OnDisable()
        {
            s_LastClosedTime = DateTime.Now.Ticks / 10000L;
            s_AddItemWindow = null;
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="searchStringKey">保存搜索值的键</param>
        public static bool Show(Rect rect, string searchStringKey)
        {
            Object[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(AddItemWindow));
            if (objectsOfTypeAll.Length > 0)
            {
                ((EditorWindow)objectsOfTypeAll[0]).Close();
                return false;
            }

            if (DateTime.Now.Ticks / 10000L < s_LastClosedTime + 50L)
                return false;

            if (Event.current != null)
                Event.current.Use();

            s_ItemSearchKey = searchStringKey;

            if (s_AddItemWindow == null)
                s_AddItemWindow = CreateInstance<AddItemWindow>();

            return true;
        }

        /// <param name="itemPathArray">item的路径数据，路径格式： xxx/yyy/zzz</param>
        /// <param name="itemTexture2DArray">item的图片</param>
        /// <param name="itemInfoArray">item的额外数据，传入点击回调</param>
        /// <param name="clickedAction">点击item的回调</param>
        public void Init(Rect buttonRect, string[] itemPathArray, Texture2D[] itemTexture2DArray, object[] itemInfoArray, Action<string, object> clickedAction)
        {
            s_AddItemWindow.m_ItemPathArray = itemPathArray;
            s_AddItemWindow.m_ItemTexture2DArray = itemTexture2DArray;
            s_AddItemWindow.m_ItemInfoArray = itemInfoArray;
            s_AddItemWindow.m_ClickedAction = clickedAction;

            buttonRect = GUIUtilityWrap.GUIToScreenRect(buttonRect);
            CreateComponentTree();
            EditorWindowWrap.ShowAsDropDown(this, buttonRect, new Vector2(buttonRect.width, kWindowHeight));
            Focus();
            EditorWindowWrap.AddToAuxWindowList(this);
            wantsMouseMove = true;
        }

        private void CreateComponentTree()
        {
            List<string> list = new List<string>();
            List<Element> list2 = new List<Element>();
            for (int i = 0; i < m_ItemPathArray.Length; i++)
            {
                string menuPath = m_ItemPathArray[i];
                string[] strArray3 = menuPath.Split('/');
                while ((strArray3.Length - 1) < list.Count)
                {
                    list.RemoveAt(list.Count - 1);
                }
                while ((list.Count > 0) && (strArray3[list.Count - 1] != list[list.Count - 1]))
                {
                    list.RemoveAt(list.Count - 1);
                }
                while ((strArray3.Length - 1) > list.Count)
                {
                    list2.Add(new GroupElement(list.Count, strArray3[list.Count]));
                    list.Add(strArray3[list.Count]);
                }

                Texture2D texture = null;
                if (m_ItemTexture2DArray != null && i < m_ItemTexture2DArray.Length)
                {
                    texture = m_ItemTexture2DArray[i];
                }

                object info = null;
                if (m_ItemInfoArray != null && i < m_ItemInfoArray.Length)
                {
                    info = m_ItemInfoArray[i];
                }

                list2.Add(new ItemElement(list.Count, strArray3[strArray3.Length - 1], menuPath, texture, info));
            }

            m_Tree = list2.ToArray();
            if (m_Stack.Count == 0)
            {
                m_Stack.Add(m_Tree[0] as GroupElement);
            }

            m_DirtyList = false;
            RebuildSearch();
        }

        void OnGUI()
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            GUI.Label(new Rect(0.0f, 0.0f, position.width, position.height), GUIContent.none, s_Styles.background);
            if (m_DirtyList)
                CreateComponentTree();
            HandleKeyboard();
            GUILayout.Space(7f);


            EditorGUI.FocusTextInControl(s_ItemSearchKey);
            Rect rect = GUILayoutUtility.GetRect(10f, 20f);
            rect.x += 8f;
            rect.width -= 16f;
            GUI.SetNextControlName(s_ItemSearchKey);

            string str = EditorGUIWrap.SearchField(rect, m_DelayedSearch ?? m_Search);
            if (str != m_Search || m_DelayedSearch != null)
            {
                if (!isAnimating)
                {
                    m_Search = m_DelayedSearch ?? str;
                    EditorPrefs.SetString(s_ItemSearchKey, m_Search);
                    RebuildSearch();
                    m_DelayedSearch = null;
                }
                else
                    m_DelayedSearch = str;
            }

            ListGUI(activeTree, m_Anim, GetElementRelative(0), GetElementRelative(-1));
            if (m_Anim < 1.0)
                ListGUI(activeTree, m_Anim + 1f, GetElementRelative(-1), GetElementRelative(-2));

            if (!isAnimating || Event.current.type != EventType.Repaint)
                return;

            long ticks = DateTime.Now.Ticks;
            float num = (ticks - m_LastTime) / 1E+07f;
            m_LastTime = ticks;
            m_Anim = Mathf.MoveTowards(m_Anim, m_AnimTarget, num * 4f);
            if (m_AnimTarget == 0 && m_Anim == 0.0)
            {
                m_Anim = 1f;
                m_AnimTarget = 1;
                m_Stack.RemoveAt(m_Stack.Count - 1);
            }
            Repaint();
        }

        private void HandleKeyboard()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.DownArrow)
            {
                ++activeParent.selectedIndex;
                activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex,
                    GetChildren(activeTree, activeParent).Count - 1);
                m_ScrollToSelected = true;
                current.Use();
            }

            if (current.keyCode == KeyCode.UpArrow)
            {
                --activeParent.selectedIndex;
                activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
                m_ScrollToSelected = true;
                current.Use();
            }

            if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
            {
                GoToChild(activeElement, true);
                current.Use();
            }

            if (!hasSearch)
            {
                if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace)
                {
                    GoToParent();
                    current.Use();
                }

                if (current.keyCode == KeyCode.RightArrow)
                {
                    GoToChild(activeElement, false);
                    current.Use();
                }

                if (current.keyCode == KeyCode.Escape)
                {
                    Close();
                    current.Use();
                }
            }
        }

        private void RebuildSearch()
        {
            if (!hasSearch)
            {
                m_SearchResultTree = null;
                if (m_Stack[m_Stack.Count - 1].name == kSearchHeader)
                {
                    m_Stack.Clear();
                    m_Stack.Add(m_Tree[0] as GroupElement);
                }
                m_AnimTarget = 1;
                m_LastTime = DateTime.Now.Ticks;
            }
            else
            {
                string[] strArray = m_Search.ToLower().Split(' ');
                List<Element> elementList1 = new List<Element>();
                List<Element> elementList2 = new List<Element>();
                foreach (Element element in m_Tree)
                {
                    if (element is ItemElement)
                    {
                        string str1 = element.name.ToLower().Replace(" ", "");
                        bool flag1 = true;
                        bool flag2 = false;
                        for (int index = 0; index < strArray.Length; ++index)
                        {
                            string str2 = strArray[index];
                            if (str1.Contains(str2))
                            {
                                if (index == 0 && str1.StartsWith(str2))
                                    flag2 = true;
                            }
                            else
                            {
                                flag1 = false;
                                break;
                            }
                        }
                        if (flag1)
                        {
                            if (flag2)
                                elementList1.Add(element);
                            else
                                elementList2.Add(element);
                        }
                    }
                }
                elementList1.Sort();
                elementList2.Sort();
                List<Element> elementList3 = new List<Element>();
                elementList3.Add(new GroupElement(0, kSearchHeader));
                elementList3.AddRange(elementList1);
                elementList3.AddRange(elementList2);
                elementList3.Add(m_Tree[m_Tree.Length - 1]);
                m_SearchResultTree = elementList3.ToArray();
                m_Stack.Clear();
                m_Stack.Add(m_SearchResultTree[0] as GroupElement);
                if (GetChildren(activeTree, activeParent).Count >= 1)
                    activeParent.selectedIndex = 0;
                else
                    activeParent.selectedIndex = -1;
            }
        }

        private GroupElement GetElementRelative(int rel)
        {
            int index = m_Stack.Count + rel - 1;
            if (index < 0)
                return null;
            return m_Stack[index];
        }

        private void GoToParent()
        {
            if (m_Stack.Count <= 1)
                return;
            m_AnimTarget = 0;
            m_LastTime = DateTime.Now.Ticks;
        }

        private void GoToChild(Element e, bool addIfComponent)
        {
            if (e is ItemElement)
            {
                if (!addIfComponent)
                    return;

                if (m_ClickedAction != null)
                {
                    var itemElement = e as ItemElement;
                    m_ClickedAction(itemElement.menuPath, itemElement.info);
                }
                Close();
            }
            else
            {
                if (hasSearch)
                    return;

                m_LastTime = DateTime.Now.Ticks;
                if (m_AnimTarget == 0)
                    m_AnimTarget = 1;
                else if (m_Anim == 1.0)
                {
                    m_Anim = 0.0f;
                    m_Stack.Add(e as GroupElement);
                }
            }
        }

        private void ListGUI(Element[] tree, float anim, GroupElement parent, GroupElement grandParent)
        {
            anim = Mathf.Floor(anim) + Mathf.SmoothStep(0.0f, 1f, Mathf.Repeat(anim, 1f));
            Rect position1 = position;
            position1.x = (float)(position.width * (1.0 - anim) + 1.0);
            position1.y = kHeaderHeight;
            position1.height -= kHeaderHeight;
            position1.width -= 2f;
            GUILayout.BeginArea(position1);
            Rect rect = GUILayoutUtility.GetRect(10f, 25f);
            string name = parent.name;
            GUI.Label(rect, name, s_Styles.header);
            if (grandParent != null)
            {
                Rect position2 = new Rect(rect.x + 4f, rect.y + 7f, 13f, 13f);
                if (Event.current.type == EventType.Repaint)
                    s_Styles.leftArrow.Draw(position2, false, false, false, false);
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    GoToParent();
                    Event.current.Use();
                }
            }
            ListGUI(tree, parent);
            GUILayout.EndArea();
        }

        private void ListGUI(Element[] tree, GroupElement parent)
        {
            parent.scroll = GUILayout.BeginScrollView(parent.scroll);
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            List<Element> children = GetChildren(tree, parent);
            Rect rect1 = new Rect();
            for (int index = 0; index < children.Count; ++index)
            {
                Element e = children[index];
                Rect rect2 = GUILayoutUtility.GetRect(16f, 20f, GUILayout.ExpandWidth(true));
                if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) &&
                    (parent.selectedIndex != index && rect2.Contains(Event.current.mousePosition)))
                {
                    parent.selectedIndex = index;
                    Repaint();
                }

                bool flag1 = false;
                if (index == parent.selectedIndex)
                {
                    flag1 = true;
                    rect1 = rect2;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    GUIStyle guiStyle = s_Styles.groupButton;
                    GUIContent content = e.content;
                    bool flag2 = e is ItemElement;
                    if (flag2)
                    {
                        guiStyle = s_Styles.componentButton;
                    }
                    guiStyle.Draw(rect2, content, false, false, flag1, flag1);
                    if (!flag2)
                    {
                        Rect position = new Rect((float)(rect2.x + (double)rect2.width - 13.0), rect2.y + 4f, 13f, 13f);
                        s_Styles.rightArrow.Draw(position, false, false, false, false);
                    }
                }

                if (Event.current.type == EventType.MouseDown && rect2.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                    parent.selectedIndex = index;
                    GoToChild(e, true);
                }
            }

            EditorGUIUtility.SetIconSize(Vector2.zero);
            GUILayout.EndScrollView();
            if (!m_ScrollToSelected || Event.current.type != EventType.Repaint)
                return;

            m_ScrollToSelected = false;
            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (rect1.yMax - (double)lastRect.height > parent.scroll.y)
            {
                parent.scroll.y = rect1.yMax - lastRect.height;
                Repaint();
            }

            if (rect1.y < (double)parent.scroll.y)
            {
                parent.scroll.y = rect1.y;
                Repaint();
            }
        }

        private List<Element> GetChildren(Element[] tree, Element parent)
        {
            List<Element> elementList = new List<Element>();
            int num = -1;
            int index;
            for (index = 0; index < tree.Length; ++index)
            {
                if (tree[index] == parent)
                {
                    num = parent.level + 1;
                    ++index;
                    break;
                }
            }
            if (num == -1)
                return elementList;
            for (; index < tree.Length; ++index)
            {
                Element element = tree[index];
                if (element.level >= num)
                {
                    if (element.level <= num || hasSearch)
                        elementList.Add(element);
                }
                else
                    break;
            }
            return elementList;
        }

        private class Element : IComparable
        {
            public int level;
            public GUIContent content;

            public string name
            {
                get { return content.text; }
            }

            public virtual int CompareTo(object o)
            {
                return name.CompareTo((o as Element).name);
            }
        }

        private class ItemElement : Element
        {
            public string menuPath;
            public object info;

            public ItemElement(int level, string name, string menuPath, Texture2D texture, object info)
            {
                this.level = level;
                this.menuPath = menuPath;
                this.info = info;

                if (texture == null)
                    content = new GUIContent(name);
                else
                {
                    content = new GUIContent(name, texture);
                }
            }
        }

        private class GroupElement : Element
        {
            public int selectedIndex;
            public Vector2 scroll;

            public GroupElement(int level, string name)
            {
                this.level = level;
                content = new GUIContent(name);
            }
        }

        private class Styles
        {
            public GUIStyle header = new GUIStyle(EditorStylesWrap.inspectorBig);
            public GUIStyle componentButton = new GUIStyle("PR Label");
            public GUIStyle background = "grey_border";
            public GUIStyle previewHeader = new GUIStyle(EditorStyles.label);
            public GUIStyle previewText = new GUIStyle(EditorStyles.wordWrappedLabel);
            public GUIStyle rightArrow = "AC RightArrow";
            public GUIStyle leftArrow = "AC LeftArrow";
            public GUIStyle groupButton;

            public Styles()
            {
                header.font = EditorStyles.boldLabel.font;
                componentButton.alignment = TextAnchor.MiddleLeft;
                componentButton.padding.left -= 15;
                componentButton.fixedHeight = 20f;
                groupButton = new GUIStyle(componentButton);
                groupButton.padding.left += 17;
                previewText.padding.left += 3;
                previewText.padding.right += 3;
                ++previewHeader.padding.left;
                previewHeader.padding.right += 3;
                previewHeader.padding.top += 3;
                previewHeader.padding.bottom += 2;
            }
        }
    }
}