using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class BreadcrumbBar : View
    {
        public GUIStyle foldout = "AC RightArrow";
        public GUIStyle topBarBg = "ProjectBrowserTopBarBg";
        public GUIStyle exposablePopup = "ExposablePopupMenu";
        public GUIContent SearchIn = new GUIContent("Search:");

        private List<KeyValuePair<GUIContent, int>> m_BreadCrumbs = new List<KeyValuePair<GUIContent, int>>();
        protected int m_KeyboardControlID;
        private float m_SearchAreaMenuOffset = -1f;
        private ExposablePopupMenu m_SearchAreaMenu;

        public int KeyboardControlID
        {
            set { m_KeyboardControlID = value; }
        }

        public TreeView FolderTreeView { get; set; }

        public bool ShowMultipleFolders { get; set; }

        public bool LastFolderHasSubFolders { get; set; }

        public bool IsSearching { get; set; }

        public List<KeyValuePair<GUIContent, int>> BreadCrumbs
        {
            get { return m_BreadCrumbs; }
        }

        public Action<int> ShowFolderContentsAction { get; set; }

        public BreadcrumbBar(ViewGroupManager owner) : base(owner)
        {
            m_SearchAreaMenu = new ExposablePopupMenu();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (IsSearching)
                SearchAreaBar(rect);
            else
                BreadCrumbBar(rect);
        }

        private void SearchAreaBar(Rect rect)
        {
            GUI.Label(rect, GUIContent.none, topBarBg);
            rect.x += 5f;
            rect.width -= 10f;
            ++rect.y;
            GUIStyle boldLabel = EditorStyles.boldLabel;
            GUI.Label(rect, SearchIn, boldLabel);
            if (m_SearchAreaMenuOffset < 0)
                m_SearchAreaMenuOffset = boldLabel.CalcSize(SearchIn).x;
            rect.x += m_SearchAreaMenuOffset + 7f;
            rect.width -= m_SearchAreaMenuOffset + 7f;
            rect.width = this.m_SearchAreaMenu.OnGUI(rect);
        }

        private void BreadCrumbBar(Rect rect)
        {
            Event current = Event.current;
            if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
            {
                GUIUtility.keyboardControl = m_KeyboardControlID;
                Repaint();
            }

            GUI.Label(rect, GUIContent.none, topBarBg);

            Rect listHeaderRect = rect;
            ++listHeaderRect.y;
            listHeaderRect.x += 4f;
            if (!ShowMultipleFolders)
            {
                for (int index = 0; index < m_BreadCrumbs.Count; ++index)
                {
                    bool flag = index == m_BreadCrumbs.Count - 1;
                    GUIStyle style = !flag ? EditorStyles.label : EditorStyles.boldLabel;
                    GUIContent key = m_BreadCrumbs[index].Key;
                    int id = m_BreadCrumbs[index].Value;
                    Vector2 vector2 = style.CalcSize(key);
                    listHeaderRect.width = vector2.x;
                    if (GUI.Button(listHeaderRect, key, style))
                    {
                        if (ShowFolderContentsAction != null)
                            ShowFolderContentsAction(id);
                    }
                    listHeaderRect.x += vector2.x + 3f;
                    if (!flag || LastFolderHasSubFolders)
                    {
                        Rect arrowRect = new Rect(listHeaderRect.x, listHeaderRect.y + 2f, 13f, 13f);
                        if (EditorGUI.DropdownButton(arrowRect, GUIContent.none, FocusType.Passive, foldout))
                        {
                            int childItemId = Int32.MinValue;
                            if (!flag)
                                childItemId = m_BreadCrumbs[index + 1].Value;
                            BreadCrumbListMenu.Show(id, childItemId, FolderTreeView, arrowRect, this);
                        }
                    }
                    listHeaderRect.x += 11f;
                }
            }
            else
            {
                GUI.Label(listHeaderRect, GUIContentWrap.Temp("Showing multiple folders..."), EditorStyles.miniLabel);
            }
        }

        public void InitSearchMenu(GUIContent firstItemContent, int firsteItemId)
        {
            List<ExposablePopupMenu.ItemData> items = new List<ExposablePopupMenu.ItemData>();
            GUIStyle guiStyle = "ExposablePopupItem";

            items.Add(new ExposablePopupMenu.ItemData(firstItemContent, guiStyle, true, true, firsteItemId));
            //items.Add(new ExposablePopupMenu.ItemData(secondItemContent, guiStyle, false,  true, secondItemId));

            ExposablePopupMenu.PopupButtonData popupButtonData = new ExposablePopupMenu.PopupButtonData(firstItemContent, exposablePopup);
            m_SearchAreaMenu.Init(items, 10f, 450f, popupButtonData, (itemData) => { });
        }
    }

    public class BreadCrumbListMenu
    {
        private int m_SubFolderId;
        private BreadcrumbBar m_BreadcrumbBar;

        private BreadCrumbListMenu(int subFolderId, BreadcrumbBar breadcrumbBar)
        {
            m_SubFolderId = subFolderId;
            m_BreadcrumbBar = breadcrumbBar;
        }

        public static void Show(int id, int childItemId, TreeView treeView, Rect activatorRect, BreadcrumbBar breadcrumbBar)
        {
            GenericMenu genericMenu = new GenericMenu();
            var item = treeView.FindItem(id);
            var childItem = treeView.FindItem(childItemId);
            if (item != null && item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    genericMenu.AddItem(new GUIContent(child.displayName),
                        childItem != null && childItem.displayName == child.displayName, new GenericMenu.MenuFunction(new BreadCrumbListMenu(child.id, breadcrumbBar).SelectSubFolder));
                    genericMenu.ShowAsContext();
                }
            }
            else
                genericMenu.AddDisabledItem(new GUIContent("No sub folders..."));
            genericMenu.DropDown(activatorRect);
        }

        private void SelectSubFolder()
        {
            if (m_BreadcrumbBar.ShowFolderContentsAction != null)
                m_BreadcrumbBar.ShowFolderContentsAction(m_SubFolderId);
        }
    }
}

