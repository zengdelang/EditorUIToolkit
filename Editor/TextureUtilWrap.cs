using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TextureUtilWrap
{
    public static bool GetLinearSampled(Texture t)
    {
        Type type = typeof (EditorUtility);
        type = type.Assembly.GetType("UnityEditor.TextureUtil");
        var mf = type.GetMethod("GetLinearSampled",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture) }, null);
        return (bool)mf.Invoke(null, new object[] { t });
    }

    public static int CountMipmaps(Texture t)
    {
        Type type = typeof(EditorUtility);
        type = type.Assembly.GetType("UnityEditor.TextureUtil");
        var mf = type.GetMethod("CountMipmaps",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture) }, null);
        return (int)mf.Invoke(null, new object[] { t });
    }

    public static void SetMipMapBiasNoDirty(Texture tex, float bias)
    {
        Type type = typeof(EditorUtility);
        type = type.Assembly.GetType("UnityEditor.TextureUtil");
        var mf = type.GetMethod("SetMipMapBiasNoDirty",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture),typeof(float) }, null);
        mf.Invoke(null, new object[] { tex, bias });
    }

    public static void SetFilterModeNoDirty(Texture tex, FilterMode mode)
    {
        Type type = typeof(EditorUtility);
        type = type.Assembly.GetType("UnityEditor.TextureUtil");
        var mf = type.GetMethod("SetFilterModeNoDirty",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture), typeof(FilterMode) }, null);
        mf.Invoke(null, new object[] { tex, mode });
    }

    public static bool IsCompressedTextureFormat(TextureFormat format)
    {
        Type type = typeof(EditorUtility);
        type = type.Assembly.GetType("UnityEditor.TextureUtil");
        var mf = type.GetMethod("IsCompressedTextureFormat",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(TextureFormat)}, null);
        return (bool)mf.Invoke(null, new object[] { format });
    }


    public static int GetMipmapCount(Texture t)
    {
        Type type = typeof(EditorUtility);
        type = type.Assembly.GetType("UnityEditor.TextureUtil");
        var mf = type.GetMethod("GetMipmapCount",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture) }, null);
        return (int)mf.Invoke(null, new object[] { t });
    }
}
