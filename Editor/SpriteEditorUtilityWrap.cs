using System;
using System.Reflection;
using UnityEngine;

public class SpriteEditorUtilityWrap
{
    public static void BeginLines(Color color)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("BeginLines",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof (Color)}, null);
        mf.Invoke(null, new object[] {color});
    }

    public static void EndLines()
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("EndLines",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] {}, null);
        mf.Invoke(null, new object[] {});
    }

    public static void DrawLine(Vector3 p1, Vector3 p2)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("DrawLine",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof (Vector3), typeof (Vector3)}, null);
        mf.Invoke(null, new object[] {p1, p2});
    }

    public static void DrawBox(Rect position)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("DrawBox",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof (Rect)}, null);
        mf.Invoke(null, new object[] {position});
    }

    public static Rect RoundedRect(Rect rect)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("RoundedRect",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof (Rect)}, null);
        return (Rect) mf.Invoke(null, new object[] {rect});
    }

    public static Rect ClampedRect(Rect rect, Rect clamp, bool maintainSize)
    {
        Type type = typeof (UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("ClampedRect",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof (Rect), typeof (Rect), typeof (bool)},
            null);
        return (Rect) mf.Invoke(null, new object[] {rect, clamp, maintainSize});
    }

    public static Vector2 GetPivotValue(SpriteAlignment alignment, Vector2 customOffset)
    {
        Type type = typeof(UnityEditorInternal.AssetStore);
        type = type.Assembly.GetType("UnityEditorInternal.SpriteEditorUtility");
        var mf = type.GetMethod("GetPivotValue",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(SpriteAlignment), typeof(Vector2) },
            null);
        return (Vector2)mf.Invoke(null, new object[] { alignment, customOffset });
    }
}
