using UnityEngine;

namespace EUTK
{
    public class EditorGUIUtilityWrap
    {
        public static bool HasHolddownKeyModifiers(Event evt)
        {
            return evt.shift | evt.control | evt.alt | evt.command;
        }
    }
}