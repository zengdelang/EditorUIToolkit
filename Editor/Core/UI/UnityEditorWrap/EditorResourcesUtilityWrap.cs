using System.Reflection;

namespace EUTK
{
    public class EditorResourcesUtilityWrap
    {
        private static string folderIconNameStr;

        public static string folderIconName
        {
            get
            {
                if (folderIconNameStr != null)
                {
                    return folderIconNameStr;
                }
                var type = typeof(UnityEditorInternal.AssetStore);
                type = type.Assembly.GetType("UnityEditorInternal.EditorResourcesUtility");
                var p = type.GetProperty("folderIconName", BindingFlags.Static | BindingFlags.Public);
                folderIconNameStr = (string)p.GetValue(null, null);
                return folderIconNameStr;
            }
        }
    }
}