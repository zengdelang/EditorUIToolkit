using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace EUTK
{
    public struct FileInfo
    {
        public string FullName;
        public string Name;
    }

    public class EditorFileUtility
    {
        public static List<FileInfo> FilterDirectory(string path, string[] extNames, bool includeChilrenDir = true)
        {
            List<FileInfo> result = new List<FileInfo>();
            if (!Directory.Exists(path))
            {
                return null;
            }

            Queue<DirectoryInfo> dirQueue = new Queue<DirectoryInfo>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            dirQueue.Enqueue(dirInfo);
            while (dirQueue.Count > 0)
            {
                dirInfo = dirQueue.Dequeue();

                if (includeChilrenDir)
                {
                    foreach (var di in dirInfo.GetDirectories())
                    {
                        if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                        {
                            continue;
                        }
                        dirQueue.Enqueue(di);
                    }
                }

                foreach (var fi in dirInfo.GetFiles())
                {
                    if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }
                    foreach (var extName in extNames)
                    {
                        if (extName.ToLower() == fi.Extension.ToLower())
                        {
                            FileInfo efi = new FileInfo();
                            efi.FullName = fi.FullName;
                            efi.Name = fi.Name;
                            result.Add(efi);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static List<string> FilterDirectoryIgnoreExt(string path, string[] extNames, bool includeChilrenDir = true)
        {
            List<string> result = new List<string>();
            if (!Directory.Exists(path))
            {
                return null;
            }

            Queue<DirectoryInfo> dirQueue = new Queue<DirectoryInfo>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            dirQueue.Enqueue(dirInfo);
            while (dirQueue.Count > 0)
            {
                dirInfo = dirQueue.Dequeue();

                if (includeChilrenDir)
                {
                    foreach (var di in dirInfo.GetDirectories())
                    {
                        dirQueue.Enqueue(di);
                    }
                }

                foreach (var fi in dirInfo.GetFiles())
                {
                    if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }

                    bool isIgnore = false;
                    foreach (var extName in extNames)
                    {
                        if (extName.ToLower() == fi.Extension.ToLower())
                        {
                            isIgnore = true;
                            break;
                        }
                    }

                    if (!isIgnore)
                    {
                        result.Add(fi.FullName);
                    }
                }
            }
            return result;
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            return null;
        }

        public static void CopyDirectory(string sourceDirectory, string destDirectory)
        {
            //判断源目录和目标目录是否存在，如果不存在，则创建一个目录
            if (!Directory.Exists(sourceDirectory))
            {
                Directory.CreateDirectory(sourceDirectory);
            }
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }
            //拷贝文件
            CopyFile(sourceDirectory, destDirectory);

            //拷贝子目录       
            //获取所有子目录名称
            string[] directionName = Directory.GetDirectories(sourceDirectory);

            foreach (string directionPath in directionName)
            {
                //根据每个子目录名称生成对应的目标子目录名称
                string directionPathTemp = destDirectory + "\\" + directionPath.Substring(sourceDirectory.Length + 1);

                //递归下去
                CopyDirectory(directionPath, directionPathTemp);
            }
        }
        public static void CopyFile(string sourceDirectory, string destDirectory)
        {
            //获取所有文件名称
            string[] fileName = Directory.GetFiles(sourceDirectory);

            foreach (string filePath in fileName)
            {
                //根据每个文件名称生成对应的目标文件名称
                string filePathTemp = destDirectory + "\\" + filePath.Substring(sourceDirectory.Length + 1);

                //若不存在，直接复制文件；若存在，覆盖复制
                if (File.Exists(filePathTemp))
                {
                    File.Copy(filePath, filePathTemp, true);
                }
                else
                {
                    File.Copy(filePath, filePathTemp);
                }
            }
        }

        public static void ClearDirectory(string path)
        {
            DeleteDirectory(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void CreateFile(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.Create(path);
        }

        public static void CreateParentDirecotry(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
        }

        public static string GetPathWithoutExt(string filePath)
        {
            var path = Path.GetDirectoryName(filePath);
            path = path.Replace("\\", "/");
            return path + "/" + Path.GetFileNameWithoutExtension(filePath);
        }

        public static string GetPath(string dirPath)
        {
            var path = Path.GetDirectoryName(dirPath);
            path = path.Replace("\\", "/");
            return path;
        }

        public static string RenameFileOrDirectory(string filePath, string newFileName, out bool result)
        {
            result = false;
            if (Directory.Exists(filePath))
            {
                DirectoryInfo di = new DirectoryInfo(filePath);
                var newPath = Path.Combine(di.Parent.FullName, newFileName);
                if (!Directory.Exists(newPath))
                {
                    di.MoveTo(newPath);
                    result = true;
                }
                return newPath;
            }
            else
            {
                string extensName = Path.GetExtension(filePath);
                string newName = newFileName + extensName;
                var newPath = Path.Combine(Directory.GetParent(filePath).FullName, newName);
                if (!File.Exists(newPath))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                    fi.MoveTo(newPath);
                    result = true;
                }
                return newPath;
            }
        }

        public static string CreateNewFolder(string folder, string name)
        {
            var path = Path.Combine(folder, name);
            if (Directory.Exists(path))
            {
                while (true)
                {
                    name = GetName(name);
                    path = Path.Combine(folder, name);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        return path;
                    }
                }
            }

            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetNewFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                var parentFolder = Path.GetDirectoryName(folder);
                var name = Path.GetFileName(folder);
                while (true)
                {
                    name = GetName(name);
                    var path = Path.Combine(parentFolder, name);
                    if (!Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }
            return folder;
        }

        public static string GetNewFile(string folder, string name, string ext)
        {
            var path = Path.Combine(folder, name + "." + ext);
            if (File.Exists(path))
            {
                int i = 0;
                while (true)
                {
                    name = GetName(name);
                    path = Path.Combine(folder, name + "." + ext);
                    if (!File.Exists(path))
                    {
                        return path;
                    }
                    ++i;
                }
            }
            return path;
        }

        public static string GetNewFile(string path)
        {
            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path);
                var dirctoryName = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                while (true)
                {
                    name = GetName(name);
                    path = Path.Combine(dirctoryName, name + ext);
                    if (!File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            return path;
        }

        public static string GetName(string name)
        {
            int index = -1;
            int count = 0;
            for (int i = name.Length - 1; i >= 0; --i)
            {
                var c = name[i];
                if (char.IsDigit(c))
                {
                    index = i;
                    ++count;
                }
            }

            if (index == -1)
                return name + " 1";

            var num = int.Parse(name.Substring(index, count));
            ++num;
            var prefix = name.Substring(0, index);
            return prefix + num.ToString().PadLeft(count, '0');
        }

        private const int FO_DELETE = 0x3;
        private const ushort FOF_NOCONFIRMATION = 0x10;
        private const ushort FOF_ALLOWUNDO = 0x40;

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation([In, Out] _SHFILEOPSTRUCT str);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class _SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public UInt32 wFunc;
            public string pFrom;
            public string pTo;
            public UInt16 fFlags;
            public Int32 fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        //删除文件到回收站
        //此方法删除一个文件需要100ms左右的时间，删除大量文件需要不少时间
        //不过好处是可以从回收站中找回被删文件
        public static int DeleteToTrash(string path)
        {
            _SHFILEOPSTRUCT pm = new _SHFILEOPSTRUCT();
            pm.wFunc = FO_DELETE;
            pm.pFrom = path + '\0';
            pm.pTo = null;
            pm.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
            return SHFileOperation(pm);
        }
    }
}