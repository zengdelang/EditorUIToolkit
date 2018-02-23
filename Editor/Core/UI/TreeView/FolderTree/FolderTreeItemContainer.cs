using System;
using System.Collections.Generic;
using System.IO;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class FolderTreeItemContainer : ScriptableObject
    {
        [JsonMember] [SerializeField] public FolderTreeViewItem RootItem;
        [JsonMember] [SerializeField] protected int autoId;
        [JsonMember] [SerializeField] protected string rootFolderPath;

        [JsonIgnore] [NonSerialized] public EditorWindowConfigSource ConfigSource;
        [JsonIgnore] [NonSerialized] public string[] ExtNames = { };

        public Action UpdateItemChangedAction { get; set; }

        public string RootFolderPath
        {
            get { return rootFolderPath; }
            set
            {
                if (rootFolderPath != value)
                {
                    rootFolderPath = value;
                    autoId = 0;
                    RootItem = new FolderTreeViewItem();
                    var di = new DirectoryInfo(value);
                    RootItem.Path = di.FullName.Replace("/", "\\"); ;
                    RootItem.IsFolder = true;
                    RootItem.displayName = di.Name;
                    RootItem.id = GetAutoID();
                    GetNewRootPathContent(RootItem);
                }
            }
        }

        public int GetAutoID()
        {
            return autoId++;
        }

        public void UpdateItemsParent()
        {
            if (RootItem != null)
                UpdateItemParent(RootItem);
        }

        protected void UpdateItemParent(TreeViewItem item)
        {
            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    child.SetConfigSource(ConfigSource);
                    child.parent = item;
                    UpdateItemParent(child);
                }
            }

            var folderItem = item as FolderTreeViewItem;
            if (folderItem != null && folderItem.FileList != null)
            {
                foreach (var child in folderItem.FileList)
                {
                    child.SetConfigSource(ConfigSource);
                    child.parent = item;
                }
            }
        }

        public void UpdateValidItems()
        {
            if (RootItem != null)
            {
                if (!Directory.Exists(RootItem.Path))
                {
                    RootItem = null;
                    autoId = 0;
                }
                else
                {
                    var change = false;
                    UpdateItemInfo(RootItem, ref change);
                    if (change && UpdateItemChangedAction != null)
                        UpdateItemChangedAction();
                }
            }
        }

        protected void UpdateItemInfo(FolderTreeViewItem item, ref bool change)
        {
            if (item.hasChildren)
            {
                for (int i = item.children.Count - 1; i >= 0; --i)
                {
                    var child = item.children[i] as FolderTreeViewItem;
                    if (child == null || !Directory.Exists(child.Path) ||
                        !child.Path.StartsWith(item.Path))
                    {
                        change = true;
                        item.children.RemoveAt(i);
                    }
                }
            }

            if (item.FileList != null)
            {
                for (int i = item.FileList.Count - 1; i >= 0; --i)
                {
                    var child = item.FileList[i];
                    if (!File.Exists(child.Path))
                    {
                        change = true;
                        item.FileList.RemoveAt(i);
                    }
                }
            }

            DirectoryInfo dirInfo = new DirectoryInfo(item.Path);
            foreach (var di in dirInfo.GetDirectories())
            {
                if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if (item.hasChildren)
                {
                    bool exist = false;
                    foreach (var childItem in item.children)
                    {
                        var folderItem = childItem as FolderTreeViewItem;
                        if (folderItem != null && folderItem.Path == di.FullName)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (exist)
                        continue;
                }

                var child = new FolderTreeViewItem();
                child.Path = di.FullName.Replace("/", "\\"); ;
                child.IsFolder = true;
                child.id = GetAutoID();
                child.displayName = di.Name;
                child.parent = item;
                child.SetConfigSource(ConfigSource);
                item.AddChild(child);
                change = true;
            }

            foreach (var fi in dirInfo.GetFiles())
            {
                if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }
                foreach (var extName in ExtNames)
                {
                    if (extName.ToLower() == fi.Extension.ToLower())
                    {
                        if (item.FileList != null)
                        {
                            bool exist = false;
                            foreach (var childItem in item.FileList)
                            {
                                var fileItem = childItem as FolderTreeViewItem;
                                if (fileItem != null && fileItem.Path == fi.FullName)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (exist)
                                break;
                        }

                        var child = new FolderTreeViewItem();
                        child.Path = fi.FullName.Replace("/", "\\"); ;
                        child.IsFolder = false;
                        child.id = GetAutoID();
                        child.displayName = Path.GetFileNameWithoutExtension(fi.Name);
                        child.parent = item;
                        child.SetConfigSource(ConfigSource);
                        if (item.FileList == null)
                            item.FileList = new List<FolderTreeViewItem>();
                        item.FileList.Add(child);
                        change = true;
                        break;
                    }
                }
            }

            if (item.hasChildren)
            {
                var comparator = new AlphanumComparator.AlphanumComparator();
                item.children.Sort((viewItem, treeViewItem) =>
                {
                    return comparator.Compare(viewItem.displayName, treeViewItem.displayName);
                });
            }

            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    UpdateItemInfo(child as FolderTreeViewItem, ref change);
                }
            }
        }

        protected void GetNewRootPathContent(FolderTreeViewItem item)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(item.Path);
            foreach (var di in dirInfo.GetDirectories())
            {
                if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }
                var child = new FolderTreeViewItem();
                child.Path = di.FullName.Replace("/", "\\"); ;
                child.IsFolder = true;
                child.id = GetAutoID();
                child.displayName = di.Name;
                child.parent = item;
                child.SetConfigSource(ConfigSource);
                item.AddChild(child);
                GetNewRootPathContent(child);
            }

            foreach (var fi in dirInfo.GetFiles())
            {
                if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }
                foreach (var extName in ExtNames)
                {
                    if (extName.ToLower() == fi.Extension.ToLower())
                    {
                        var child = new FolderTreeViewItem();
                        child.Path = fi.FullName.Replace("/", "\\"); ;
                        child.IsFolder = false;
                        child.id = GetAutoID();
                        child.displayName = Path.GetFileNameWithoutExtension(fi.Name);
                        child.parent = item;
                        child.SetConfigSource(ConfigSource);
                        if (item.FileList == null)
                            item.FileList = new List<FolderTreeViewItem>();
                        item.FileList.Add(child);
                        break;
                    }
                }
            }
        }
    }
}

