using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class TreeViewAnimationInput
    {
        public Action<TreeViewAnimationInput> animationEnded;

        public float elapsedTimeNormalized
        {
            get { return Mathf.Clamp01((float)elapsedTime / (float)animationDuration); }
        }

        public double elapsedTime
        {
            get { return EditorApplication.timeSinceStartup - startTime; }
            set { startTime = EditorApplication.timeSinceStartup - value; }
        }

        public int startRow { get; set; }

        public int endRow { get; set; }

        public Rect rowsRect { get; set; }

        public Rect startRowRect { get; set; }

        public double startTime { get; set; }

        public double animationDuration { get; set; }

        public bool expanding { get; set; }

        public TreeViewItem item { get; set; }

        public TreeView treeView { get; set; }

        public void FireAnimationEndedEvent()
        {
            if (animationEnded == null)
                return;
            animationEnded(this);
        }

        public override string ToString()
        {
            return string.Concat("Input: startRow ", " endRow ", endRow, " rowsRect ", " startTime ", startTime,
                " anitmationDuration ", animationDuration, " ", "expanding ", expanding ? true : false, " ",
                item.displayName);
        }
    }
}