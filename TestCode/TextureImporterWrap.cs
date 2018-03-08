using System;
using System.Reflection;
using UnityEditor;

public class TextureImporterWrap
{
    public static void GetWidthAndHeight(TextureImporter ti, ref int width, ref int height)
    {
        Type type = typeof (TextureImporter);
        var mi = type.GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int).MakeByRefType(),typeof(int).MakeByRefType()}, null);
        var param = new Object[] { width,height};
        mi.Invoke(ti, param);
        width = (int) param[0];
        height = (int)param[1];
    }
}
