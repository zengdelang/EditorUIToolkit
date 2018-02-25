using UnityEngine;

namespace EUTK
{
    public class GraphStyles
    {
        protected static GUISkin s_GraphSkin;

        public static GUISkin GraphSkin
        {
            get
            {
                if (s_GraphSkin == null)
                    s_GraphSkin = Resources.Load<GUISkin>("GraphSkin");
                return s_GraphSkin;
            }
        }

        public static GUIStyle windowHighlight
        {
            get { return GraphSkin.GetStyle("windowHighlight"); }
        }

        public static GUIStyle window
        {
            get { return GraphSkin.GetStyle("window"); }
        }

        public static GUIStyle selectionRect
        {
            get { return "SelectionRect"; }
        }

        public static GUIStyle selection
        {
            get { return GraphSkin.GetStyle("selection"); }
        }

        public static GUIStyle lightButton
        {
            get { return GraphSkin.GetStyle("lightButton"); }
        }

        public static GUIStyle lightTextField
        {
            get { return GraphSkin.GetStyle("lightTextField"); }
        }

        public static GUIStyle circle
        {
            get { return GraphSkin.GetStyle("circle"); }
        }

        public static GUIStyle textArea
        {
            get { return GraphSkin.GetStyle("textArea"); }
        }

        public static GUIStyle arrowLeft
        {
            get { return GraphSkin.GetStyle("arrowLeft"); }
        }

        public static GUIStyle arrowRight
        {
            get { return GraphSkin.GetStyle("arrowRight"); }
        }

        public static GUIStyle arrowTop
        {
            get { return GraphSkin.GetStyle("arrowTop"); }
        }

        public static GUIStyle arrowBottom
        {
            get { return GraphSkin.GetStyle("arrowBottom"); }
        }

        public static GUIStyle scaleArrow
        {
            get { return GraphSkin.GetStyle("scaleArrow"); }
        }

        public static Texture2D ConnectionPoint
        {
            get { return Resources.Load<Texture2D>("ConnectionPoint"); }
        }      
    }
}