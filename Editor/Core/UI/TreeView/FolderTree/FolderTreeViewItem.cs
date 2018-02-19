using System;
using System.Collections.Generic;
using System.IO;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [Serializable]
    public class FolderTreeViewItem : TreeViewItem
    {
        [SerializeField] protected bool m_IsFolder;
        [SerializeField] public List<FolderTreeViewItem> FileList = new List<FolderTreeViewItem>();
        [SerializeField] protected string m_Path;

        public bool IsFolder
        {
            get
            {
                return m_IsFolder;
            }
            set
            {
                m_IsFolder = value;
                SetDirty();
            }
        }

        public string Path
        {
            get { return m_Path; }
            set
            {
                m_Path = value;
                SetDirty();
            }
        }

        public bool Rename(string newName)
        {
            var result = false;
            var newPath = EditorFileUtility.RenameFileOrDirectory(Path, newName, out result).Replace("/", "\\");
            if (!result)
            {
                return result;
            }
            RenameChildren(Path, newPath);
            Path = newPath;
            return true;
        }

        protected void RenameChildren(string oldPath, string newParentPath)
        {
            if (hasChildren)
            {
                foreach (var item in children)
                {
                    var child = item as FolderTreeViewItem;
                    child.Path = child.Path.Replace(oldPath, newParentPath).Replace("/", "\\");
                    child.RenameChildren(oldPath, newParentPath);
                }
            }

            if (FileList != null)
            {
                foreach (var item in FileList)
                {
                    item.Path = item.Path.Replace(oldPath, newParentPath).Replace("/", "\\");
                }
            }
        }

        public void Reparent(FolderTreeViewItem parent)
        {
            if (IsFolder)
            {
                DirectoryInfo di = new DirectoryInfo(Path);
                var targetPath = System.IO.Path.Combine(parent.Path, displayName).Replace("/", "\\");
                if (targetPath != Path)
                {
                    di.MoveTo(targetPath);
                    Path = targetPath;
                    ReparentChildren();
                }
            }
            else
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(Path);
                var targetPath = System.IO.Path.Combine(parent.Path, System.IO.Path.GetFileName(Path)).Replace("/", "\\");
                if (targetPath != Path)
                {
                    fi.MoveTo(targetPath);
                    Path = targetPath;
                }
            }
        }

        protected void ReparentChildren()
        {
            if (children != null)
            {
                foreach (var item in children)
                {
                    var child = item as FolderTreeViewItem;
                    if (child != null)
                    {
                        child.Path = System.IO.Path.Combine(Path, System.IO.Path.GetFileName(child.Path)).Replace("/", "\\");
                        child.ReparentChildren();
                    }
                }
            }

            if (FileList != null)
            {
                foreach (var item in FileList)
                {
                    item.Path = System.IO.Path.Combine(Path, System.IO.Path.GetFileName(item.Path)).Replace("/", "\\");
                }
            }
        }
    }
}