using System.Collections;
using System.Collections.Generic;
using EUTK;
using UnityEditor;
using UnityEngine;

public class NewBehaviourScript : SpriteUtilityWindow
{
    [MenuItem("Tools/Eaxamples/NewBehaviourScript", false, 0)]
    public static void ShowCoreConfigTool()
    {
        GetWindow<NewBehaviourScript>();
    }

    void OnGUI()
    {
        InitStyles();
        m_TextureViewRect = new Rect(0,0, position.width, position.height);
        SetNewTexture(Resources.Load<Texture2D>("test"));
        DoTextureGUI();
    }
}
