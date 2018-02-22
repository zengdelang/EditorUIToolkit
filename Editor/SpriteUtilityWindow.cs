using System;
using EUTK;
using UnityEditor;
using UnityEngine;

public class SpriteUtilityWindow : EditorWindow
{
    protected const float k_BorderMargin = 10f;
    protected const float k_ScrollbarMargin = 16f;
    protected const float k_InspectorWindowMargin = 8f;
    protected const float k_InspectorWidth = 330f;
    protected const float k_MinZoomPercentage = 0.9f;
    protected const float k_MaxZoom = 50f;
    protected const float k_WheelZoomSpeed = 0.03f;
    protected const float k_MouseZoomSpeed = 0.005f;
    protected const float k_ToolbarHeight = 17f;

    protected bool    m_ShowAlpha;
    protected float   m_Zoom = -1f;
    protected float   m_MipLevel;
    protected Vector2 m_ScrollPosition;
    protected Styles  m_Styles;
   

    protected Texture2D m_Texture;
    protected Texture2D m_TextureAlphaOverride;
    protected Rect      m_TextureViewRect;
    protected Rect      m_TextureRect;

    protected Rect maxScrollRect
    {
        get
        {
            float num1 = m_Texture.width * 0.5f * m_Zoom;
            float num2 = m_Texture.height * 0.5f * m_Zoom;
            return new Rect(-num1, -num2, m_TextureViewRect.width + num1 * 2f, m_TextureViewRect.height + num2 * 2f);
        }
    }

    protected Rect maxRect
    {
        get
        {
            float num1 = m_TextureViewRect.width * 0.5f / GetMinZoom();
            float num2 = m_TextureViewRect.height * 0.5f / GetMinZoom();
            return new Rect(-num1, -num2, m_Texture.width + num1 * 2f, m_Texture.height + num2 * 2f);
        }
    }

    protected void InitStyles()
    {
        if (m_Styles != null)
            return;
        m_Styles = new Styles();
    }

    protected float GetMinZoom()
    {
        if (m_Texture ==  null)
            return 1f;
        return Mathf.Min((float) ((double) m_TextureViewRect.width / (double) m_Texture.width),
                   (float) ((double) m_TextureViewRect.height / (double) m_Texture.height), 50f) * 0.9f;
    }

    protected void HandleZoom()
    {
        bool flag = Event.current.alt && Event.current.button == 1;
        if (flag)
            EditorGUIUtility.AddCursorRect(m_TextureViewRect, MouseCursor.Zoom);
        if ((Event.current.type == EventType.MouseUp ||
             Event.current.type == EventType.MouseDown) && flag ||
            (Event.current.type == EventType.KeyUp ||
             Event.current.type == EventType.KeyDown) &&
            Event.current.keyCode == KeyCode.LeftAlt)
            Repaint();
        if (Event.current.type != EventType.ScrollWheel &&
            (Event.current.type != EventType.MouseDrag || !Event.current.alt ||
             Event.current.button != 1))
            return;
        float num1 = (float) (1.0 - Event.current.delta.y *
                              (Event.current.type != EventType.ScrollWheel
                                  ? -0.00499999988824129
                                  : 0.0299999993294477));
        float num2 = m_Zoom * num1;
        float num3 = Mathf.Clamp(num2, GetMinZoom(), 50f);
        if (num3 != (double) m_Zoom)
        {
            m_Zoom = num3;
            if (num2 != (double) num3)
                num1 /= num2 / num3;
            m_ScrollPosition *= num1;
            float num4 = (float) (Event.current.mousePosition.x /
                                  (double) m_TextureViewRect.width - 0.5);
            float num5 = (float) (Event.current.mousePosition.y /
                                  (double) m_TextureViewRect.height - 0.5);
            float num6 = num4 * (num1 - 1f);
            float num7 = num5 * (num1 - 1f);
            Rect maxScrollRect = this.maxScrollRect;
            m_ScrollPosition.x += num6 * (maxScrollRect.width / 2f);
            m_ScrollPosition.y += num7 * (maxScrollRect.height / 2f);
            Event.current.Use();
        }
    }

    protected void HandlePanning()
    {
        bool flag = !Event.current.alt && Event.current.button > 0 ||
                    Event.current.alt && Event.current.button <= 0;
        if (flag && GUIUtility.hotControl == 0)
        {
            EditorGUIUtility.AddCursorRect(m_TextureViewRect, MouseCursor.Pan);
            if (Event.current.type == EventType.MouseDrag)
            {
                m_ScrollPosition -= Event.current.delta;
                Event.current.Use();
            }
        }
        if ((Event.current.type != EventType.MouseUp &&
             Event.current.type != EventType.MouseDown || !flag) &&
            (Event.current.type != EventType.KeyUp && Event.current.type != EventType.KeyDown ||
             Event.current.keyCode != KeyCode.LeftAlt))
            return;
        Repaint();
    }

    protected void DrawTexturespaceBackground()
    {
        float num1 = Mathf.Max(maxRect.width, maxRect.height);
        Vector2 vector2 = new Vector2(maxRect.xMin, maxRect.yMin);
        float num2 = num1 * 0.5f;
        float a = !EditorGUIUtility.isProSkin ? 0.08f : 0.15f;
        float num3 = 8f;
        SpriteEditorUtilityWrap.BeginLines(new Color(0.0f, 0.0f, 0.0f, a));
        float num4 = 0.0f;
        while (num4 <= (double) num1)
        {
            SpriteEditorUtilityWrap.DrawLine(new Vector2(-num2 + num4, num2 + num4) + vector2,
                new Vector2(num2 + num4, -num2 + num4) + vector2);
            num4 += num3;
        }
        SpriteEditorUtilityWrap.EndLines();
    }

    private float Log2(float x)
    {
        return (float) (Math.Log(x) / Math.Log(2.0));
    }

    protected void DrawTexture()
    {
        int num1 = Mathf.Max(m_Texture.width, 1);
        float num2 = Mathf.Min(m_MipLevel,
            TextureUtilWrap.GetMipmapCount(m_Texture) - 1);
        float mipMapBias = m_Texture.mipMapBias;
        TextureUtilWrap.SetMipMapBiasNoDirty(m_Texture,
            num2 - Log2(num1 / m_TextureRect.width));
        FilterMode filterMode = m_Texture.filterMode;
        TextureUtilWrap.SetFilterModeNoDirty(m_Texture,
            FilterMode.Point);
        if (m_ShowAlpha)
        {
            if (m_TextureAlphaOverride !=  null)
                EditorGUI.DrawTextureTransparent(m_TextureRect,
                    m_TextureAlphaOverride);
            else
                EditorGUI.DrawTextureAlpha(m_TextureRect, m_Texture);
        }
        else
            EditorGUI.DrawTextureTransparent(m_TextureRect, m_Texture);
        TextureUtilWrap.SetMipMapBiasNoDirty(m_Texture, mipMapBias);
        TextureUtilWrap.SetFilterModeNoDirty(m_Texture, filterMode);
    }

    protected void DrawScreenspaceBackground()
    {
        if (Event.current.type != EventType.Repaint)
            return;
        m_Styles.preBackground.Draw(m_TextureViewRect, false, false, false, false);
    }

    protected void HandleScrollbars()
    {
        m_ScrollPosition.x = GUI.HorizontalScrollbar(
            new Rect(m_TextureViewRect.xMin, m_TextureViewRect.yMax, m_TextureViewRect.width, 16f),
            m_ScrollPosition.x, m_TextureViewRect.width, maxScrollRect.xMin, maxScrollRect.xMax);
        m_ScrollPosition.y = GUI.VerticalScrollbar(
            new Rect(m_TextureViewRect.xMax, m_TextureViewRect.yMin, 16f, m_TextureViewRect.height),
            m_ScrollPosition.y, m_TextureViewRect.height, maxScrollRect.yMin, maxScrollRect.yMax);
    }

    protected void SetupHandlesMatrix()
    {
        Handles.matrix = Matrix4x4.TRS(new Vector3(m_TextureRect.x, m_TextureRect.yMax, 0.0f),
            Quaternion.identity, new Vector3(m_Zoom, -m_Zoom, 1f));
    }


    //工具条ui
    protected Rect DoAlphaZoomToolbarGUI(Rect area)
    {
        int a = 1;
        if (m_Texture != null)
            a = Mathf.Max(a, TextureUtilWrap.GetMipmapCount(m_Texture));
        Rect position = new Rect(area.width, 0.0f, 0.0f, area.height);
        //mipmap条
        using (new EditorGUI.DisabledScope(a == 1))
        {
            position.width = m_Styles.largeMip.image.width;
            position.x -= position.width;
            GUI.Box(position, m_Styles.largeMip, m_Styles.preLabel);
            position.width = 60f;
            position.x -= position.width;
            m_MipLevel = Mathf.Round(GUI.HorizontalSlider(position, m_MipLevel, a - 1, 0.0f,
                m_Styles.preSlider, m_Styles.preSliderThumb));
            position.width = m_Styles.smallMip.image.width;
            position.x -= position.width;
            GUI.Box(position, m_Styles.smallMip, m_Styles.preLabel);
        }
        //alpha条
        position.width = 60f;
        position.x -= position.width;
        m_Zoom = GUI.HorizontalSlider(position, m_Zoom, GetMinZoom(), 50f, m_Styles.preSlider,
            m_Styles.preSliderThumb);
        position.width = 32f;
        position.x -= position.width + 5f;
        m_ShowAlpha = GUI.Toggle(position, m_ShowAlpha,
            !m_ShowAlpha ? m_Styles.RGBIcon : m_Styles.alphaIcon, "toolbarButton");
        return new Rect(area.x, area.y, position.x, area.height);
    }

    protected void DoTextureGUI()
    {
        if (m_Texture == null)
            return;
        if (m_Zoom < 0.0)
            m_Zoom = GetMinZoom();
        m_TextureRect =
            new Rect(
                (float) (m_TextureViewRect.width / 2.0 -
                         m_Texture.width * (double) m_Zoom / 2.0),
                (float) (m_TextureViewRect.height / 2.0 -
                         m_Texture.height * (double) m_Zoom / 2.0),
                m_Texture.width * m_Zoom, m_Texture.height * m_Zoom);
        HandleScrollbars();
        SetupHandlesMatrix();
        HandleZoom();
        HandlePanning();
        DrawScreenspaceBackground();
        GUIClipWrap.Push(m_TextureViewRect, -m_ScrollPosition, Vector2.zero, false);
        if (Event.current.type == EventType.Repaint)
        {
            DrawTexturespaceBackground();
            DrawTexture();
            DrawGizmos();
        }
        DoTextureGUIExtras();
        GUIClipWrap.Pop();
    }

    protected virtual void DoTextureGUIExtras()
    {
    }

    protected virtual void DrawGizmos()
    {
    }

    protected void SetNewTexture(Texture2D texture)
    {
        if (!(texture != m_Texture))
            return;
        m_Texture = texture;
        m_Zoom = -1f;
        m_TextureAlphaOverride = null;
    }

    protected void SetAlphaTextureOverride(Texture2D alphaTexture)
    {
        if (!(alphaTexture != m_TextureAlphaOverride))
            return;
        m_TextureAlphaOverride = alphaTexture;
        m_Zoom = -1f;
    }

    /*internal override void OnResized()
    {
        if (!(this.m_Texture !=  null) || UnityEngine.Event.current == null)
            return;
        this.HandleZoom();
    }*/

   /* internal static void DrawToolBarWidget(ref Rect drawRect, ref Rect toolbarRect, Action<Rect> drawAction)
    {
        toolbarRect.width -= drawRect.width;
        if ((double) toolbarRect.width < 0.0)
            drawRect.width += toolbarRect.width;
        if ((double) drawRect.width <= 0.0)
            return;
        drawAction(drawRect);
    }*/

    protected class Styles
    {
        public readonly GUIStyle dragdot = "U2D.dragDot";
        public readonly GUIStyle dragdotDimmed = "U2D.dragDotDimmed";
        public readonly GUIStyle dragdotactive = "U2D.dragDotActive";
        public readonly GUIStyle createRect = "U2D.createRect";
        public readonly GUIStyle preToolbar = "preToolbar";
        public readonly GUIStyle preButton = "preButton";
        public readonly GUIStyle preLabel = "preLabel";
        public readonly GUIStyle preSlider = "preSlider";
        public readonly GUIStyle preSliderThumb = "preSliderThumb";
        public readonly GUIStyle preBackground = "preBackground";
        public readonly GUIStyle pivotdotactive = "U2D.pivotDotActive";
        public readonly GUIStyle pivotdot = "U2D.pivotDot";
        public readonly GUIStyle dragBorderdot = new GUIStyle();
        public readonly GUIStyle dragBorderDotActive = new GUIStyle();
        public readonly GUIStyle toolbar;
        public readonly GUIContent alphaIcon;
        public readonly GUIContent RGBIcon;
        public readonly GUIStyle notice;
        public readonly GUIContent smallMip;
        public readonly GUIContent largeMip;

        public Styles()
        {
            toolbar = new GUIStyle(EditorStylesWrap.inspectorBig);
            toolbar.margin.top = 0;
            toolbar.margin.bottom = 0;
            alphaIcon = EditorGUIUtility.IconContent("PreTextureAlpha");
            RGBIcon = EditorGUIUtility.IconContent("PreTextureRGB");
            preToolbar.border.top = 0;
            createRect.border = new RectOffset(3, 3, 3, 3);
            notice = new GUIStyle(GUI.skin.label);
            notice.alignment = TextAnchor.MiddleCenter;
            notice.normal.textColor = Color.yellow;
            dragBorderdot.fixedHeight = 5f;
            dragBorderdot.fixedWidth = 5f;
            dragBorderdot.normal.background = EditorGUIUtility.whiteTexture;
            dragBorderDotActive.fixedHeight = dragBorderdot.fixedHeight;
            dragBorderDotActive.fixedWidth = dragBorderdot.fixedWidth;
            dragBorderDotActive.normal.background = EditorGUIUtility.whiteTexture;
            smallMip = EditorGUIUtility.IconContent("PreTextureMipMapLow");
            largeMip = EditorGUIUtility.IconContent("PreTextureMipMapHigh");
        }
    }
}