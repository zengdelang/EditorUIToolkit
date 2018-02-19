using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace EUTK
{
    public class GUIStyleWrap
    {
        public delegate int GetNumCharactersThatFitWithinWidthDelegate(string text, float width);
        protected static Dictionary<GUIStyle, GetNumCharactersThatFitWithinWidthDelegate> s_ActionMap = new Dictionary<GUIStyle, GetNumCharactersThatFitWithinWidthDelegate>();

        public static int GetNumCharactersThatFitWithinWidth(GUIStyle obj, string text, float width)
        {
            if (s_ActionMap.ContainsKey(obj))
            {
                return s_ActionMap[obj](text, width);
            }

            var type = typeof(GUIStyle);
            var mf = type.GetMethod("GetNumCharactersThatFitWithinWidth",
                BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(float) }, null);

            var action = (GetNumCharactersThatFitWithinWidthDelegate)Delegate.CreateDelegate(typeof(GetNumCharactersThatFitWithinWidthDelegate), obj, mf);
            if (action == null)
                throw new NullReferenceException("action");

            s_ActionMap.Add(obj, action);
            return action(text, width);
        }
    }
}
