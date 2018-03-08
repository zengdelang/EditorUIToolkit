using System;
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

        SetNewTexture(null);
        DoTextureGUI();
    }
} 
 