using System;
using System.Collections.Generic;
using JsonFx.U3DEditor;
using UnityEngine;

namespace EUTK
{
    [JsonClassType]
    [JsonOptIn]
    [Serializable]
    public class TreeItemContainer : ScriptableObject
    {
        [JsonMember] [SerializeField] public List<TreeViewItem> ItemList = new List<TreeViewItem>();
        [JsonMember] [SerializeField] protected int autoId;

        [JsonIgnore] [NonSerialized] public EditorWindowConfigSource ConfigSource;

        public int GetAutoID()
        {
            return autoId++;
        }

        public void UpdateItemsParent()
        {
            if (ItemList.Count == 0)
                autoId = 0;

            foreach (var item in ItemList)
            {
                UpdateItemParent(item);
            }
        }

        protected void UpdateItemParent(TreeViewItem item)
        {
            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    child.parent = item;
                    child.SetConfigSource(ConfigSource);
                    UpdateItemParent(child);
                }
            }
        }
    }
}