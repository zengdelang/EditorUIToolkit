using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderTreeViewTestWindow : ViewGroupEditorWindow
    {
        [MenuItem("Tools/Eaxamples/FolderTreeViewTestWindow", false, 0)]
        public static void ShowCoreConfigTool()
        {
            GetWindow<FolderTreeViewTestWindow>();
        }

        private static string[] ExtNames = { };

        private FolderTreeViewGroup m_FolderTreeViewGroup;
        private TreeView m_TreeView;
        private FolderTreeItemContainer m_DataContainer;

        private TipsViewGroup m_TipsViewGroup;

        protected override void InitData()
        {
            m_WindowConfigSource = FileConfigSource.CreateFileConfigSource("ViewConfig/TestWindow/config4.txt", true, typeof(FolderTreeViewTestWindowConfig));

            m_FolderTreeViewGroup = new FolderTreeViewGroup(m_LayoutGroupMgr, m_WindowConfigSource, "TreeViewStateConfig", "TreeViewDataContainer");
            m_FolderTreeViewGroup.Active = false;

            m_DataContainer = m_FolderTreeViewGroup.GetDataContainer();
            m_DataContainer.ExtNames = ExtNames;
            m_DataContainer.UpdateValidItems();

            m_TreeView = m_FolderTreeViewGroup.GetTreeView();
            m_TreeView.useExpansionAnimation = true;
            m_TreeView.deselectOnUnhandledMouseDown = true;
            m_TreeView.contextClickItemCallback = ContextClickItemCallback;
            m_TreeView.contextClickOutsideItemsCallback = ContextClickOutsideItemsCallback;

            m_LayoutGroupMgr.AddViewGroup(m_FolderTreeViewGroup);

            m_TipsViewGroup = new TipsViewGroup(m_LayoutGroupMgr);
            m_TipsViewGroup.Active = false;
            m_TipsViewGroup.TipStr = "当前根目录路径不存在,请配置根目录路径";
            m_TipsViewGroup.DrawAction += () =>
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("选择目录"))
                {
                    var path = EditorUtility.OpenFolderPanel("选择目录", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (m_DataContainer.RootFolderPath != path)
                        {
                            m_DataContainer.RootFolderPath = path;
                            m_TreeView.state.expandedIDs = new List<int>();
                            m_TreeView.state.selectedIDs = new List<int>();
                            m_TreeView.state.lastClickedID = Int32.MinValue;
                            m_TreeView.state.scrollPos = Vector2.zero;
                            if (m_TreeView.data != null)
                                m_TreeView.data.ReloadData();
                            m_WindowConfigSource.SetConfigDirty();
                        }
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            };
            m_LayoutGroupMgr.AddViewGroup(m_TipsViewGroup);
            CheckViewVisible();
        }

        protected override void OnInspectorUpdate()
        {
            base.OnInspectorUpdate();
            CheckViewVisible();
        }

        private void CheckViewVisible()
        {
            if (m_FolderTreeViewGroup != null)
            {
                if (m_DataContainer != null)
                {
                    var rootPath = m_DataContainer.RootFolderPath;
                    if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                    {
                        m_TipsViewGroup.Active = true;
                        m_FolderTreeViewGroup.Active = false;
                    }
                    else
                    {
                        m_TipsViewGroup.Active = false;
                        m_FolderTreeViewGroup.Active = true;
                    }
                }
            }
        }

        private void ContextClickItemCallback(int itemId)
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Ping Item"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]);
                m_TreeView.Frame(item.id, true, true);

            });

            g.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                var item = m_TreeView.data.FindItem(m_TreeView.state.selectedIDs[0]) as FolderTreeViewItem;
                WindowsOSUtility.ExploreDirectory(item.Path);
            });
            g.ShowAsContext();
            Event.current.Use();
        }

        private void ContextClickOutsideItemsCallback()
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                var item = m_TreeView.data.root as FolderTreeViewItem;
                WindowsOSUtility.ExploreDirectory(item.Path);
            });
            g.ShowAsContext();
        }
    }
}
 