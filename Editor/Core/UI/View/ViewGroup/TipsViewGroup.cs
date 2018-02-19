using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class TipsViewGroup : ViewGroup
    {
        public string TipStr { get; set; }
        public Action DrawAction { get; set; }

        public TipsViewGroup(ViewGroupManager owner) : base(owner)
        {

        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (!string.IsNullOrEmpty(TipStr))
            {
                EditorGUILayout.LabelField(TipStr);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (DrawAction != null)
            {
                DrawAction();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }
    }
}