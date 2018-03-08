/*using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteEditorWindow : SpriteUtilityWindow, ISpriteEditor
{
    public static bool s_OneClickDragStarted = false;
    private bool m_RequestRepaint = false;
    private ISpriteEditorModule m_CurrentModule = (ISpriteEditorModule)null;
    private int m_CurrentModuleIndex = 0;
    private const float k_MarginForFraming = 0.05f;
    private const float k_WarningMessageWidth = 250f;
    private const float k_WarningMessageHeight = 40f;
    private const float k_ModuleListWidth = 90f;
    public static SpriteEditorWindow s_Instance;
    public bool m_ResetOnNextRepaint;
    public bool m_IgnoreNextPostprocessEvent;


    private ISpriteEditorDataProvider m_SpriteDataProvider;


    private SerializedObject m_TextureImporterSO;
    public string m_SelectedAssetPath;
    private IEventSystem m_EventSystem;
    private IUndoSystem m_UndoSystem;
    private IAssetDatabase m_AssetDatabase;
    private IGUIUtility m_GUIUtility;
    private UnityEngine.Texture2D m_OutlineTexture;




    public ITexture2D m_OriginalTexture;
    private SpriteRectCache m_RectsCache;``````
    private UnityEngine.Texture2D m_ReadableTexture;
    [SerializeField]
    private SpriteRect m_Selected;


    private GUIContent[] m_RegisteredModuleNames;
    private List<ISpriteEditorModule> m_AllRegisteredModules;
    private List<ISpriteEditorModule> m_RegisteredModules;

    private Rect warningMessageRect
    {
        get
        {
            return new Rect((float)((double)this.position.width - 250.0 - 8.0 - 16.0), 24f, 250f, 40f);
        }
    }

    private bool multipleSprites
    {
        get
        {
            if (this.m_SpriteDataProvider != null)
                return this.m_SpriteDataProvider.spriteImportMode == SpriteImportMode.Multiple;
            return false;
        }
    }

    private bool validSprite
    {
        get
        {
            if (this.m_SpriteDataProvider != null)
                return this.m_SpriteDataProvider.spriteImportMode != SpriteImportMode.None;
            return false;
        }
    }

    private bool activeTextureSelected
    {
        get
        {
            return this.m_SpriteDataProvider != null && this.m_Texture != (ITexture2D)null && this.m_OriginalTexture != (ITexture2D)null;
        }
    }

    public bool textureIsDirty { get; set; }

    public bool selectedTextureChanged
    {
        get
        {
            ITexture2D selectedTexture2D = this.GetSelectedTexture2D();
            return selectedTexture2D != (ITexture2D)null && this.m_OriginalTexture != selectedTexture2D;
        }
    }

    public ISpriteRectCache spriteRects
    {
        get
        {
            return (ISpriteRectCache)this.m_RectsCache;
        }
    }

    public SpriteRect selectedSpriteRect
    {
        get
        {
            if (this.editingDisabled)
                return (SpriteRect)null;
            return this.m_Selected;
        }
        set
        {
            this.m_Selected = value;
        }
    }

    public ISpriteEditorDataProvider spriteEditorDataProvider
    {
        get
        {
            return this.m_SpriteDataProvider;
        }
    }

    public bool enableMouseMoveEvent
    {
        set
        {
            this.wantsMouseMove = value;
        }
    }

    public Rect windowDimension
    {
        get
        {
            return this.position;
        }
    }

    public ITexture2D selectedTexture
    {
        get
        {
            return this.m_OriginalTexture;
        }
    }

    public ITexture2D previewTexture
    {
        get
        {
            return this.m_Texture;
        }
    }

    public bool editingDisabled
    {
        get
        {
            return EditorApplication.isPlayingOrWillChangePlaymode;
        }
    }

    public SpriteEditorWindow()
    {
        this.m_EventSystem = (IEventSystem)new EventSystem();
        this.m_UndoSystem = (IUndoSystem)new UndoSystem();
        this.m_AssetDatabase = (IAssetDatabase)new AssetDatabaseSystem();
        this.m_GUIUtility = (IGUIUtility)new GUIUtilitySystem();
    }

    public static void GetWindow()
    {
        EditorWindow.GetWindow<SpriteEditorWindow>();
    }

    private void ModifierKeysChanged()
    {
        if (!((Object)EditorWindow.focusedWindow == (Object)this))
            return;
        this.Repaint();
    }

    private void OnFocus()
    {
        if (!this.selectedTextureChanged)
            return;
        this.OnSelectionChange();
    }

    public static void TextureImporterApply(SerializedObject so)
    {
        if ((Object)SpriteEditorWindow.s_Instance == (Object)null)
            return;
        SpriteEditorWindow.s_Instance.ApplyCacheSettingsToInspector(so);
    }

    private void ApplyCacheSettingsToInspector(SerializedObject so)
    {
        if (this.m_SpriteDataProvider == null || !(this.m_SpriteDataProvider.targetObject == so.targetObject))
            return;
        if ((SpriteImportMode)so.FindProperty("m_SpriteMode").intValue == this.m_SpriteDataProvider.spriteImportMode)
            SpriteEditorWindow.s_Instance.m_IgnoreNextPostprocessEvent = true;
        else if (this.textureIsDirty && EditorUtility.DisplayDialog(SpriteEditorWindow.SpriteEditorWindowStyles.spriteEditorWindowTitle.text, SpriteEditorWindow.SpriteEditorWindowStyles.pendingChangesDialogContent.text, SpriteEditorWindow.SpriteEditorWindowStyles.yesButtonLabel.text, SpriteEditorWindow.SpriteEditorWindowStyles.noButtonLabel.text))
            this.DoApply(so);
    }

    public void RefreshPropertiesCache()
    {
        this.m_OriginalTexture = this.GetSelectedTexture2D();
        if (this.m_OriginalTexture == (ITexture2D)null)
            return;
        AssetImporter atPath = AssetImporter.GetAtPath(this.m_SelectedAssetPath);
        this.m_SpriteDataProvider = atPath as ISpriteEditorDataProvider;
        if (atPath is TextureImporter)
            this.m_SpriteDataProvider = (ISpriteEditorDataProvider)new UnityEditor.U2D.Interface.TextureImporter((TextureImporter)atPath);
        if ((Object)atPath == (Object)null || this.m_SpriteDataProvider == null)
            return;
        this.m_TextureImporterSO = new SerializedObject((Object)atPath);
        this.m_SpriteDataProvider.InitSpriteEditorDataProvider(this.m_TextureImporterSO);
        int width = 0;
        int height = 0;
        this.m_SpriteDataProvider.GetTextureActualWidthAndHeight(out width, out height);
        this.m_Texture = !(this.m_OriginalTexture == (ITexture2D)null) ? (ITexture2D)new SpriteEditorWindow.PreviewTexture2D((UnityEngine.Texture2D)this.m_OriginalTexture, width, height) : (ITexture2D)null;
    }

    public void InvalidatePropertiesCache()
    {
        if ((bool)((Object)this.m_RectsCache))
        {
            this.m_RectsCache.ClearAll();
            Object.DestroyImmediate((Object)this.m_RectsCache);
        }
        if ((bool)((Object)this.m_ReadableTexture))
        {
            Object.DestroyImmediate((Object)this.m_ReadableTexture);
            this.m_ReadableTexture = (UnityEngine.Texture2D)null;
        }
        this.m_OriginalTexture = (ITexture2D)null;
        this.m_SpriteDataProvider = (ISpriteEditorDataProvider)null;
    }

    public bool IsEditingDisabled()
    {
        return EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private void OnSelectionChange()
    {
        if (this.GetSelectedTexture2D() == (ITexture2D)null || this.selectedTextureChanged)
        {
            this.HandleApplyRevertDialog();
            this.ResetWindow();
            this.RefreshPropertiesCache();
            this.RefreshRects();
        }
        if ((Object)this.m_RectsCache != (Object)null)
        {
            if (Selection.activeObject is Sprite)
                this.UpdateSelectedSpriteRect(Selection.activeObject as Sprite);
            else if ((Object)Selection.activeGameObject != (Object)null && (bool)((Object)Selection.activeGameObject.GetComponent<SpriteRenderer>()))
                this.UpdateSelectedSpriteRect(Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite);
        }
        this.UpdateAvailableModules();
        this.Repaint();
    }

    public void ResetWindow()
    {
        this.InvalidatePropertiesCache();
        this.selectedSpriteRect = (SpriteRect)null;
        this.textureIsDirty = false;
        this.m_Zoom = -1f;
    }

    private void OnEnable()
    {
        this.minSize = new Vector2(360f, 200f);
        this.titleContent = SpriteEditorWindow.SpriteEditorWindowStyles.spriteEditorWindowTitle;
        SpriteEditorWindow.s_Instance = this;
        this.m_UndoSystem.RegisterUndoCallback(new Undo.UndoRedoCallback(this.UndoRedoPerformed));
        EditorApplication.modifierKeysChanged += new EditorApplication.CallbackFunction(this.ModifierKeysChanged);
        this.ResetWindow();
        this.RefreshPropertiesCache();
        this.RefreshRects();
        this.InitModules();
    }

    private void UndoRedoPerformed()
    {
        ITexture2D selectedTexture2D = this.GetSelectedTexture2D();
        if (selectedTexture2D != (ITexture2D)null && this.m_OriginalTexture != selectedTexture2D)
            this.OnSelectionChange();
        this.InitSelectedSpriteRect();
        this.Repaint();
    }

    private void InitSelectedSpriteRect()
    {
        SpriteRect spriteRect = (SpriteRect)null;
        if ((Object)this.m_RectsCache != (Object)null && this.m_RectsCache.Count > 0)
            spriteRect = !this.m_RectsCache.Contains(this.selectedSpriteRect) ? this.m_RectsCache.RectAt(0) : this.selectedSpriteRect;
        this.selectedSpriteRect = spriteRect;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= new Undo.UndoRedoCallback(this.UndoRedoPerformed);
        if ((Object)this.m_RectsCache != (Object)null)
            Undo.ClearUndo((Object)this.m_RectsCache);
        this.HandleApplyRevertDialog();
        this.InvalidatePropertiesCache();
        EditorApplication.modifierKeysChanged -= new EditorApplication.CallbackFunction(this.ModifierKeysChanged);
        SpriteEditorWindow.s_Instance = (SpriteEditorWindow)null;
        if ((Object)this.m_OutlineTexture != (Object)null)
        {
            Object.DestroyImmediate((Object)this.m_OutlineTexture);
            this.m_OutlineTexture = (UnityEngine.Texture2D)null;
        }
        if (!(bool)((Object)this.m_ReadableTexture))
            return;
        Object.DestroyImmediate((Object)this.m_ReadableTexture);
        this.m_ReadableTexture = (UnityEngine.Texture2D)null;
    }

    private void HandleApplyRevertDialog()
    {
        if (!this.textureIsDirty || this.m_SpriteDataProvider == null)
            return;
        if (EditorUtility.DisplayDialog(SpriteEditorWindow.SpriteEditorWindowStyles.applyRevertDialogTitle.text, string.Format(SpriteEditorWindow.SpriteEditorWindowStyles.applyRevertDialogContent.text, (object)this.m_SelectedAssetPath), SpriteEditorWindow.SpriteEditorWindowStyles.applyButtonLabel.text, SpriteEditorWindow.SpriteEditorWindowStyles.revertButtonLabel.text))
            this.DoApply();
        else
            this.DoRevert();
        this.SetupModule(this.m_CurrentModuleIndex);
    }

    private void RefreshRects()
    {
        if ((bool)((Object)this.m_RectsCache))
        {
            this.m_RectsCache.ClearAll();
            Undo.ClearUndo((Object)this.m_RectsCache);
            Object.DestroyImmediate((Object)this.m_RectsCache);
        }
        this.m_RectsCache = ScriptableObject.CreateInstance<SpriteRectCache>();
        if (this.m_SpriteDataProvider != null)
        {
            if (this.multipleSprites)
            {
                for (int i = 0; i < this.m_SpriteDataProvider.spriteDataCount; ++i)
                {
                    SpriteRect r = new SpriteRect();
                    r.LoadFromSpriteData(this.m_SpriteDataProvider.GetSpriteData(i));
                    this.m_RectsCache.AddRect(r);
                    EditorUtility.DisplayProgressBar(SpriteEditorWindow.SpriteEditorWindowStyles.loadProgressTitle.text, string.Format(SpriteEditorWindow.SpriteEditorWindowStyles.loadContentText.text, (object)i, (object)this.m_SpriteDataProvider.spriteDataCount), (float)i / (float)this.m_SpriteDataProvider.spriteDataCount);
                }
            }
            else if (this.validSprite)
            {
                SpriteRect r = new SpriteRect();
                r.LoadFromSpriteData(this.m_SpriteDataProvider.GetSpriteData(0));
                r.rect = new Rect(0.0f, 0.0f, (float)this.m_Texture.width, (float)this.m_Texture.height);
                r.name = this.m_OriginalTexture.name;
                this.m_RectsCache.AddRect(r);
            }
            EditorUtility.ClearProgressBar();
        }
        this.InitSelectedSpriteRect();
    }

    private void OnGUI()
    {
        this.InitStyles();
        if (this.m_ResetOnNextRepaint || this.selectedTextureChanged || (Object)this.m_RectsCache == (Object)null)
        {
            this.ResetWindow();
            this.RefreshPropertiesCache();
            this.RefreshRects();
            this.UpdateAvailableModules();
            this.SetupModule(this.m_CurrentModuleIndex);
            this.m_ResetOnNextRepaint = false;
        }
        Matrix4x4 matrix = Handles.matrix;
        if (!this.activeTextureSelected)
        {
            using (new EditorGUI.DisabledScope(true))
                GUILayout.Label(SpriteEditorWindow.SpriteEditorWindowStyles.noSelectionWarning);
        }
        else
        {
            this.DoToolbarGUI();
            this.DoTextureGUI();
           // this.DoEditingDisabledMessage();
            this.m_CurrentModule.OnPostGUI();
            Handles.matrix = matrix;
            if (!this.m_RequestRepaint)
                return;
            this.Repaint();
        }
    }

    protected override void DoTextureGUIExtras()
    {
        this.HandleFrameSelected();
        if (this.m_EventSystem.current.type == EventType.Repaint)
        {
            SpriteEditorUtilityWrap.BeginLines(new Color(1f, 1f, 1f, 0.5f));
            for (int i = 0; i < this.m_RectsCache.Count; ++i)
            {
                if (this.m_RectsCache.RectAt(i) != this.selectedSpriteRect)
                    SpriteEditorUtility.DrawBox(this.m_RectsCache.RectAt(i).rect);
            }
            SpriteEditorUtilityWrap.EndLines();
        }

        this.m_CurrentModule.DoTextureGUI();
    }

    private void DoToolbarGUI()
    {
        GUIStyle toolbar = EditorStyles.toolbar;
        Rect rect = new Rect(0.0f, 0.0f, this.position.width, 17f);
        if (this.m_EventSystem.current.type == EventType.Repaint)
            toolbar.Draw(rect, false, false, false, false);
        this.m_TextureViewRect = new Rect(0.0f, 17f, this.position.width - 16f, (float)((double)this.position.height - 16.0 - 17.0));
        if (this.m_RegisteredModules.Count > 1)
        {
            float width = Mathf.Min((double)this.position.width <= (double)this.minSize.x ? 90f : this.position.width * (90f / this.minSize.x), EditorStyles.popup.CalcSize(this.m_RegisteredModuleNames[this.m_CurrentModuleIndex]).x);
            int newModuleIndex = EditorGUI.Popup(new Rect(0.0f, 0.0f, width, 17f), this.m_CurrentModuleIndex, this.m_RegisteredModuleNames, EditorStyles.toolbarPopup);
            if (newModuleIndex != this.m_CurrentModuleIndex)
            {
                if (this.textureIsDirty)
                {
                    if (EditorUtility.DisplayDialog(SpriteEditorWindow.SpriteEditorWindowStyles.applyRevertModuleDialogTitle.text, SpriteEditorWindow.SpriteEditorWindowStyles.applyRevertModuleDialogContent.text, SpriteEditorWindow.SpriteEditorWindowStyles.applyButtonLabel.text, SpriteEditorWindow.SpriteEditorWindowStyles.revertButtonLabel.text))
                        this.DoApply();
                    else
                        this.DoRevert();
                }
                this.SetupModule(newModuleIndex);
            }
            rect.x = width;
        }
        rect = this.DoAlphaZoomToolbarGUI(rect);
        Rect position = rect;
        position.x = position.width;
        using (new EditorGUI.DisabledScope(!this.textureIsDirty))
        {
            position.width = EditorStyles.toolbarButton.CalcSize(SpriteEditorWindow.SpriteEditorWindowStyles.applyButtonLabel).x;
            position.x -= position.width;
            if (GUI.Button(position, SpriteEditorWindow.SpriteEditorWindowStyles.applyButtonLabel, EditorStyles.toolbarButton))
            {
                this.DoApply();
                this.SetupModule(this.m_CurrentModuleIndex);
            }
            position.width = EditorStyles.toolbarButton.CalcSize(SpriteEditorWindow.SpriteEditorWindowStyles.revertButtonLabel).x;
            position.x -= position.width;
            if (GUI.Button(position, SpriteEditorWindow.SpriteEditorWindowStyles.revertButtonLabel, EditorStyles.toolbarButton))
            {
                this.DoRevert();
                this.SetupModule(this.m_CurrentModuleIndex);
            }
        }
        rect.width = position.x - rect.x;
        this.m_CurrentModule.DrawToolbarGUI(rect);
    }

    private void DoEditingDisabledMessage()
    {
        if (!this.IsEditingDisabled())
            return;
        GUILayout.BeginArea(this.warningMessageRect);
        EditorGUILayout.HelpBox(SpriteEditorWindow.SpriteEditorWindowStyles.editingDisableMessageLabel.text, MessageType.Warning);
        GUILayout.EndArea();
    }

    private void DoApply(SerializedObject so)
    {
        if (this.multipleSprites)
        {
            List<string> stringList1 = new List<string>();
            List<string> stringList2 = new List<string>();
            this.m_SpriteDataProvider.spriteDataCount = this.m_RectsCache.Count;
            for (int i = 0; i < this.m_RectsCache.Count; ++i)
            {
                SpriteRect spriteRect = this.m_RectsCache.RectAt(i);
                if (string.IsNullOrEmpty(spriteRect.name))
                    spriteRect.name = "Empty";
                if (!string.IsNullOrEmpty(spriteRect.originalName))
                {
                    stringList1.Add(spriteRect.originalName);
                    stringList2.Add(spriteRect.name);
                }
                SpriteDataBase spriteData = this.m_SpriteDataProvider.GetSpriteData(i);
                spriteRect.ApplyToSpriteData(spriteData);
                EditorUtility.DisplayProgressBar(SpriteEditorWindow.SpriteEditorWindowStyles.saveProgressTitle.text, string.Format(SpriteEditorWindow.SpriteEditorWindowStyles.saveContentText.text, (object)i, (object)this.m_RectsCache.Count), (float)i / (float)this.m_RectsCache.Count);
            }
            if (stringList1.Count > 0)
                PatchImportSettingRecycleID.PatchMultiple(so, 213, stringList1.ToArray(), stringList2.ToArray());
        }
        else if (this.m_RectsCache.Count > 0)
            this.m_RectsCache.RectAt(0).ApplyToSpriteData(this.m_SpriteDataProvider.GetSpriteData(0));
        this.m_SpriteDataProvider.Apply(so);
        EditorUtility.ClearProgressBar();
    }

    private void DoApply()
    {
        this.m_UndoSystem.ClearUndo((IUndoableObject)this.m_RectsCache);
        this.DoApply(this.m_TextureImporterSO);
        this.m_TextureImporterSO.ApplyModifiedPropertiesWithoutUndo();
        this.m_IgnoreNextPostprocessEvent = true;
        this.DoTextureReimport(this.m_SelectedAssetPath);
        this.textureIsDirty = false;
        this.InitSelectedSpriteRect();
    }

    private void DoRevert()
    {
        this.textureIsDirty = false;
        this.RefreshRects();
        GUI.FocusControl("");
    }

    public void HandleSpriteSelection()
    {
        if (this.m_EventSystem.current.type != EventType.MouseDown || this.m_EventSystem.current.button != 0 || (GUIUtility.hotControl != 0 || this.m_EventSystem.current.alt))
            return;
        SpriteRect selectedSpriteRect = this.selectedSpriteRect;
        this.selectedSpriteRect = this.TrySelect(this.m_EventSystem.current.mousePosition);
        if (this.selectedSpriteRect != null)
            SpriteEditorWindow.s_OneClickDragStarted = true;
        else
            this.RequestRepaint();
        if (selectedSpriteRect != this.selectedSpriteRect && this.selectedSpriteRect != null)
            this.m_EventSystem.current.Use();
    }

    private void HandleFrameSelected()
    {
        IEvent current = this.m_EventSystem.current;
        if (current.type != EventType.ValidateCommand && current.type != EventType.ExecuteCommand || !(current.commandName == "FrameSelected"))
            return;
        if (current.type == EventType.ExecuteCommand)
        {
            if (this.selectedSpriteRect == null)
                return;
            Rect rect = this.selectedSpriteRect.rect;
            float zoom = this.m_Zoom;
            this.m_Zoom = (double)rect.width >= (double)rect.height ? this.m_TextureViewRect.width / (rect.width + this.m_TextureViewRect.width * 0.05f) : this.m_TextureViewRect.height / (rect.height + this.m_TextureViewRect.height * 0.05f);
            this.m_ScrollPosition.x = (rect.center.x - (float)this.m_Texture.width * 0.5f) * this.m_Zoom;
            this.m_ScrollPosition.y = (float)(((double)rect.center.y - (double)this.m_Texture.height * 0.5) * (double)this.m_Zoom * -1.0);
            this.Repaint();
        }
        current.Use();
    }

    private void UpdateSelectedSpriteRect(Sprite sprite)
    {
        for (int i = 0; i < this.m_RectsCache.Count; ++i)
        {
            if (sprite.rect == this.m_RectsCache.RectAt(i).rect)
            {
                this.selectedSpriteRect = this.m_RectsCache.RectAt(i);
                return;
            }
        }
        this.selectedSpriteRect = (SpriteRect)null;
    }

    private ITexture2D GetSelectedTexture2D()
    {
        UnityEngine.Texture2D texture = (UnityEngine.Texture2D)null;
        if (Selection.activeObject is UnityEngine.Texture2D)
            texture = Selection.activeObject as UnityEngine.Texture2D;
        else if (Selection.activeObject is Sprite)
            texture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(Selection.activeObject as Sprite, false);
        else if ((bool)((Object)Selection.activeGameObject) && (bool)((Object)Selection.activeGameObject.GetComponent<SpriteRenderer>()) && (bool)((Object)Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite))
            texture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite, false);
        if ((Object)texture != (Object)null)
            this.m_SelectedAssetPath = this.m_AssetDatabase.GetAssetPath((Object)texture);
        return (ITexture2D)new UnityEngine.U2D.Interface.Texture2D(texture);
    }

    private SpriteRect TrySelect(Vector2 mousePosition)
    {
        float num1 = float.MaxValue;
        SpriteRect spriteRect1 = (SpriteRect)null;
        mousePosition = (Vector2)Handles.inverseMatrix.MultiplyPoint((Vector3)mousePosition);
        for (int i = 0; i < this.m_RectsCache.Count; ++i)
        {
            SpriteRect spriteRect2 = this.m_RectsCache.RectAt(i);
            if (spriteRect2.rect.Contains(mousePosition))
            {
                if (spriteRect2 == this.selectedSpriteRect)
                    return spriteRect2;
                float width = spriteRect2.rect.width;
                float height = spriteRect2.rect.height;
                float num2 = width * height;
                if ((double)width > 0.0 && (double)height > 0.0 && (double)num2 < (double)num1)
                {
                    spriteRect1 = spriteRect2;
                    num1 = num2;
                }
            }
        }
        return spriteRect1;
    }

    public void DoTextureReimport(string path)
    {
        if (this.m_SpriteDataProvider == null)
            return;
        try
        {
            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(path);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        this.textureIsDirty = false;
    }

    private void SetupModule(int newModuleIndex)
    {
        if ((Object)SpriteEditorWindow.s_Instance == (Object)null)
            return;
        if (this.m_CurrentModule != null)
            this.m_CurrentModule.OnModuleDeactivate();
        if (this.m_RegisteredModules.Count <= newModuleIndex)
            return;
        this.m_CurrentModule = this.m_RegisteredModules[newModuleIndex];
        this.m_CurrentModule.OnModuleActivate();
        this.m_CurrentModuleIndex = newModuleIndex;
    }

    private void UpdateAvailableModules()
    {
        if (this.m_AllRegisteredModules == null)
            return;
        this.m_RegisteredModules = new List<ISpriteEditorModule>();
        foreach (ISpriteEditorModule registeredModule in this.m_AllRegisteredModules)
        {
            if (registeredModule.CanBeActivated())
                this.m_RegisteredModules.Add(registeredModule);
        }
        this.m_RegisteredModuleNames = new GUIContent[this.m_RegisteredModules.Count];
        for (int index = 0; index < this.m_RegisteredModules.Count; ++index)
            this.m_RegisteredModuleNames[index] = new GUIContent(this.m_RegisteredModules[index].moduleName);
        if (!this.m_RegisteredModules.Contains(this.m_CurrentModule))
            this.SetupModule(0);
        else
            this.SetupModule(this.m_CurrentModuleIndex);
    }

    private void InitModules()
    {
        this.m_AllRegisteredModules = new List<ISpriteEditorModule>();
        if ((Object)this.m_OutlineTexture == (Object)null)
        {
            this.m_OutlineTexture = new UnityEngine.Texture2D(1, 16, TextureFormat.RGBA32, false);
            this.m_OutlineTexture.SetPixels(new Color[16]
            {
                new Color(0.5f, 0.5f, 0.5f, 0.5f),
                new Color(0.5f, 0.5f, 0.5f, 0.5f),
                new Color(0.8f, 0.8f, 0.8f, 0.8f),
                new Color(0.8f, 0.8f, 0.8f, 0.8f),
                Color.white,
                Color.white,
                Color.white,
                Color.white,
                new Color(0.8f, 0.8f, 0.8f, 1f),
                new Color(0.5f, 0.5f, 0.5f, 0.8f),
                new Color(0.3f, 0.3f, 0.3f, 0.5f),
                new Color(0.3f, 0.3f, 0.3f, 0.5f),
                new Color(0.3f, 0.3f, 0.3f, 0.3f),
                new Color(0.3f, 0.3f, 0.3f, 0.3f),
                new Color(0.1f, 0.1f, 0.1f, 0.1f),
                new Color(0.1f, 0.1f, 0.1f, 0.1f)
            });
            this.m_OutlineTexture.Apply();
            this.m_OutlineTexture.hideFlags = HideFlags.HideAndDontSave;
        }
        UnityEngine.U2D.Interface.Texture2D texture2D = new UnityEngine.U2D.Interface.Texture2D(this.m_OutlineTexture);
        this.m_AllRegisteredModules.Add((ISpriteEditorModule)new SpriteFrameModule((ISpriteEditor)this, this.m_EventSystem, this.m_UndoSystem, this.m_AssetDatabase));
        this.m_AllRegisteredModules.Add((ISpriteEditorModule)new SpritePolygonModeModule((ISpriteEditor)this, this.m_EventSystem, this.m_UndoSystem, this.m_AssetDatabase));
        this.m_AllRegisteredModules.Add((ISpriteEditorModule)new SpriteOutlineModule((ISpriteEditor)this, this.m_EventSystem, this.m_UndoSystem, this.m_AssetDatabase, this.m_GUIUtility, (IShapeEditorFactory)new ShapeEditorFactory(), (ITexture2D)texture2D));
        this.m_AllRegisteredModules.Add((ISpriteEditorModule)new SpritePhysicsShapeModule((ISpriteEditor)this, this.m_EventSystem, this.m_UndoSystem, this.m_AssetDatabase, this.m_GUIUtility, (IShapeEditorFactory)new ShapeEditorFactory(), (ITexture2D)texture2D));
        this.UpdateAvailableModules();
    }

    public ITexture2D GetReadableTexture2D()
    {
        if ((Object)this.m_ReadableTexture == (Object)null)
        {
            int width = 0;
            int height = 0;
            this.m_SpriteDataProvider.GetTextureActualWidthAndHeight(out width, out height);
            this.m_ReadableTexture = UnityEditor.SpriteUtility.CreateTemporaryDuplicate((UnityEngine.Texture2D)this.m_OriginalTexture, width, height);
            if ((Object)this.m_ReadableTexture != (Object)null)
                this.m_ReadableTexture.filterMode = UnityEngine.FilterMode.Point;
        }
        return (ITexture2D)new UnityEngine.U2D.Interface.Texture2D(this.m_ReadableTexture);
    }

    private class SpriteEditorWindowStyles
    {
        public static readonly GUIContent editingDisableMessageLabel = EditorGUIUtility.TextContent("Editing is disabled during play mode");
        public static readonly GUIContent revertButtonLabel = EditorGUIUtility.TextContent("Revert");
        public static readonly GUIContent applyButtonLabel = EditorGUIUtility.TextContent("Apply");
        public static readonly GUIContent spriteEditorWindowTitle = EditorGUIUtility.TextContent("Sprite Editor");
        public static readonly GUIContent pendingChangesDialogContent = EditorGUIUtility.TextContent("You have pending changes in the Sprite Editor Window.\nDo you want to apply these changes?");
        public static readonly GUIContent yesButtonLabel = EditorGUIUtility.TextContent("Yes");
        public static readonly GUIContent noButtonLabel = EditorGUIUtility.TextContent("No");
        public static readonly GUIContent applyRevertDialogTitle = EditorGUIUtility.TextContent("Unapplied import settings");
        public static readonly GUIContent applyRevertDialogContent = EditorGUIUtility.TextContent("Unapplied import settings for '{0}'");
        public static readonly GUIContent noSelectionWarning = EditorGUIUtility.TextContent("No texture or sprite selected");
        public static readonly GUIContent applyRevertModuleDialogTitle = EditorGUIUtility.TextContent("Unapplied module changes");
        public static readonly GUIContent applyRevertModuleDialogContent = EditorGUIUtility.TextContent("You have unapplied changes from the current module");
        public static readonly GUIContent saveProgressTitle = EditorGUIUtility.TextContent("Saving");
        public static readonly GUIContent saveContentText = EditorGUIUtility.TextContent("Saving Sprites {0}/{1}");
        public static readonly GUIContent loadProgressTitle = EditorGUIUtility.TextContent("Loading");
        public static readonly GUIContent loadContentText = EditorGUIUtility.TextContent("Loading Sprites {0}/{1}");
    }

    internal class PreviewTexture2D : UnityEngine.U2D.Interface.Texture2D
    {
        private int m_ActualWidth = 0;
        private int m_ActualHeight = 0;

        public override int width
        {
            get
            {
                return this.m_ActualWidth;
            }
        }

        public override int height
        {
            get
            {
                return this.m_ActualHeight;
            }
        }

        public PreviewTexture2D(UnityEngine.Texture2D t, int width, int height)
            : base(t)
        {
            this.m_ActualWidth = width;
            this.m_ActualHeight = height;
        }
    }












































    public void RequestRepaint()
    {
        if ((Object)EditorWindow.focusedWindow != (Object)this)
            this.Repaint();
        else
            this.m_RequestRepaint = true;
    }

    public void SetDataModified()
    {
        this.textureIsDirty = true;
    }

    public void DisplayProgressBar(string title, string content, float progress)
    {
        EditorUtility.DisplayProgressBar(title, content, progress);
    }

    public void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }



    public void ApplyOrRevertModification(bool apply)
    {
        if (apply)
            this.DoApply();
        else
            this.DoRevert();
    }

}*/