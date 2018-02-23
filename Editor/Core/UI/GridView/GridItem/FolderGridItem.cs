using JsonFx.U3DEditor;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class FolderGridItem : GridItem
    {
        [JsonMember] [SerializeField] public string Path;
        [JsonMember] [SerializeField] public bool IsFolder;
        [JsonMember] [SerializeField] public int ParentId;

        public override Texture Texture
        {
            get
            {
                return EditorGUIUtility.FindTexture(EditorResourcesUtilityWrap.folderIconName);
            }
        }
    }
}
