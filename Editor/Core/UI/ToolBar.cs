using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class ToolBar : View
    {
        public Action OnGUIAction
        {
            get; set;
        }

        public ToolBar(ViewGroupManager owner) : base(owner)
        {

        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginArea(new Rect(0.0f, 0.0f, rect.width, EditorStyles.toolbar.fixedHeight), EditorStyles.toolbar);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (OnGUIAction != null)
                OnGUIAction();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
