using EUTK;
using UnityEditor;
using UnityEngine;

namespace UGT
{
    [Name("GM Node")]
    [Description("GM指令设置结点")]
    public class GMCmdNode : GMCmdNodeBase
    {
        protected override bool showOpenScriptBtnInInspector
        {
            get { return true; }
            set { }
        }

        public GMCmdNode()
        {
            m_NodeColor = new Color(0, 1, 1);
        }

        protected override void OnNodeGUI()
        {
            base.OnNodeGUI();
            cmd = EditorGUILayout.TextArea(cmd, GraphStyles.lightTextField);
            if (GUILayout.Button("执行", GraphStyles.lightButton))
            {
                EditorWindow.focusedWindow.ShowNotification(new GUIContent("LuaEnv和ChatSys还没有初始化完成,无法执行"));
            }
        }
    }
}
