using System;
using UnityEngine;
using System.Reflection;
using UnityEditor;

public class SpriteEditorHandlesWrap
{
    public static Vector2 PivotSlider(Rect sprite, Vector2 pos, GUIStyle pivotDot, GUIStyle pivotDotActive)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorHandles");
        var mf = type.GetMethod("PivotSlider",
            BindingFlags.Static | BindingFlags.NonPublic, null,
            new Type[] {typeof (Rect), typeof (Vector2), typeof (GUIStyle), typeof (GUIStyle)},
            null);
        return (Vector2) mf.Invoke(null, new object[] {sprite, pos, pivotDot, pivotDotActive});
    }

    public static Vector2 ScaleSlider(Vector2 pos, MouseCursor cursor, Rect cursorRect)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorHandles");
        var mf = type.GetMethod("ScaleSlider",
            BindingFlags.Static | BindingFlags.NonPublic, null,
            new Type[] {typeof (Vector2), typeof (MouseCursor), typeof (Rect)},
            null);
        return (Vector2) mf.Invoke(null, new object[] {pos, cursor, cursorRect});
    }

    public static Vector2 PointSlider(Vector2 pos, MouseCursor cursor, GUIStyle dragDot, GUIStyle dragDotActive)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorHandles");
        var mf = type.GetMethod("PointSlider",
            BindingFlags.Static | BindingFlags.NonPublic, null,
            new Type[] {typeof (Vector2), typeof (MouseCursor), typeof (GUIStyle), typeof (GUIStyle)},
            null);
        return (Vector2) mf.Invoke(null, new object[] {pos, cursor, dragDot, dragDotActive});
    }

    public static Rect SliderRect(Rect pos)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorHandles");
        var mf = type.GetMethod("SliderRect",
            BindingFlags.Static | BindingFlags.NonPublic, null,
            new Type[] {typeof (Rect)},
            null);
        return (Rect) mf.Invoke(null, new object[] {pos});
    }

    public static Rect RectCreator(float textureWidth, float textureHeight, GUIStyle rectStyle)
    {
        Type type = typeof(UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorHandles");
        var mf = type.GetMethod("RectCreator",
            BindingFlags.Static | BindingFlags.NonPublic, null,
            new Type[] { typeof(float), typeof(float), typeof(GUIStyle) },
            null);
        return (Rect)mf.Invoke(null, new object[] { textureWidth, textureHeight , rectStyle });
    }
}
