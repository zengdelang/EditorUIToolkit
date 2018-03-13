using EUTK;
using JsonFx.U3DEditor;
using UnityEngine;

namespace UGT
{
    public class GMCategoryTreeItem : TreeViewItem
    {
        [JsonMember] [SerializeField] public Graph graph;

        public GMCategoryTreeItem(int id, int depth, TreeViewItem parent, string displayName) : base(id, depth, parent, displayName)
        {
            graph = ScriptableObject.CreateInstance<GMCmdGraph>();
        }

        public GMCategoryTreeItem()
        {

        }
    }
}