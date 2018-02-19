using System.Collections.Generic;
using UnityEngine;

namespace EUTK
{
    public interface ITreeViewDataSource
    {
        TreeViewItem root { get; }

        int rowCount { get; }

        void OnInitialize();

        void ReloadData();

        void InitIfNeeded();

        TreeViewItem FindItem(int id);

        int GetRow(int id);

        TreeViewItem GetItem(int row);

        List<TreeViewItem> GetRows();

        bool IsRevealed(int id);

        void RevealItem(int id);

        void SetExpandedWithChildren(TreeViewItem item, bool expand);

        void SetExpanded(TreeViewItem item, bool expand);

        bool IsExpanded(TreeViewItem item);

        bool IsExpandable(TreeViewItem item);

        int[] GetExpandedIDs();

        void SetExpandedIDs(int[] ids);

        bool SetExpanded(int id, bool expand);

        bool IsExpanded(int id);

        bool CanBeMultiSelected(TreeViewItem item);

        bool CanBeParent(TreeViewItem item);

        bool IsRenamingItemAllowed(TreeViewItem item);

        void InsertFakeItem(int id, int parentID, string name, Texture2D icon);

        void RemoveFakeItem();

        bool HasFakeItem();

        void OnSearchChanged();

        void RefreshData();
    }
}
