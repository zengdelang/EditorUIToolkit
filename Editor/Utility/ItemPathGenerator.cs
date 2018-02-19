using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EUTK
{
    public class ItemInfo
    {
        public string[] pathArray;
        public Texture2D[] texture2DArray;
        public object[] infoArray;
    }

    public class ItemDirInfo
    {
        public string fullPath;
        public string prefix;
    }

    public class ItemFileInfo
    {
        public string path;
        public string pathWithExt;
    }

    public class ItemPathGenerator
    {
        public static ItemInfo GetItemInfo(string folderPath, string extName)
        {
            if (!Directory.Exists(folderPath))
                return null;

            List<string> pathList = new List<string>();
            List<object> infoList = new List<object>();

            GetCurrentFolderInfo(pathList, infoList, "", folderPath, extName);
            return new ItemInfo()
            {
                pathArray = pathList.ToArray(),
                infoArray = infoList.ToArray()
            };
        }

        public static void GetCurrentFolderInfo(List<string> pathList, List<object> infoList, string prefix, string folderPath, string extName)
        {
            List<ItemDirInfo> dirList = new List<ItemDirInfo>();
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            foreach (var di in dirInfo.GetDirectories())
            {
                if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                dirList.Add(new ItemDirInfo()
                {
                    fullPath = di.FullName,
                    prefix = string.IsNullOrEmpty(prefix) ? di.Name : prefix + "/" + di.Name,

                });
            }

            var comparator = new AlphanumComparator.AlphanumComparator();
            dirList.Sort((obj1, obj2) =>
            {
                return comparator.Compare(obj1.prefix, obj2.prefix);
            });

            foreach (var dir in dirList)
            {
                GetCurrentFolderInfo(pathList, infoList, dir.prefix, dir.fullPath, extName);
            }

            List<ItemFileInfo> fileList = new List<ItemFileInfo>();
            foreach (var fi in dirInfo.GetFiles())
            {
                if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if (extName.ToLower() == fi.Extension.ToLower())
                {
                    var path = string.IsNullOrEmpty(prefix) ? Path.GetFileNameWithoutExtension(fi.Name) : prefix + "/" + Path.GetFileNameWithoutExtension(fi.Name);
                    var pathWithExt = string.IsNullOrEmpty(prefix) ? Path.GetFileName(fi.Name) : prefix + "/" + Path.GetFileName(fi.Name);

                    fileList.Add(new ItemFileInfo()
                    {
                        path = path,
                        pathWithExt = pathWithExt
                    });
                    break;
                }
            }

            fileList.Sort((obj1, obj2) =>
            {
                return comparator.Compare(obj1.path, obj2.path);
            });

            foreach (var file in fileList)
            {
                pathList.Add(file.path);
                infoList.Add(file.pathWithExt);
            }
        }
    }
}

