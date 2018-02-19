using UnityEngine;

namespace EUTK
{
    public class GraphStyles
    {
        protected static GUISkin s_GraphStyle;

        public static GUISkin GraphStyle
        {
            get
            {
                if (s_GraphStyle == null)
                    s_GraphStyle = Resources.Load<GUISkin>("GraphStyle");
                return s_GraphStyle;
            }
        }

        public static GUIStyle windowHighlight
        {
            get { return GraphStyle.GetStyle("windowHighlight"); }
        }

        public static GUIStyle window
        {
            get { return GraphStyle.GetStyle("window"); }
        }

        public static GUIStyle selectionRect
        {
            get { return "SelectionRect"; }
        }

        public static GUIStyle selection
        {
            get { return GraphStyle.GetStyle("selection"); }
        }

        public static GUIStyle circle
        {
            get { return GraphStyle.GetStyle("circle"); }
        }

        public static GUIStyle textArea
        {
            get { return GraphStyle.GetStyle("textArea"); }
        }

        public static GUIStyle arrowLeft
        {
            get { return GraphStyle.GetStyle("arrowLeft"); }
        }

        public static GUIStyle arrowRight
        {
            get { return GraphStyle.GetStyle("arrowRight"); }
        }

        public static GUIStyle arrowTop
        {
            get { return GraphStyle.GetStyle("arrowTop"); }
        }

        public static GUIStyle arrowBottom
        {
            get { return GraphStyle.GetStyle("arrowBottom"); }
        }

        public static GUIStyle scaleArrow
        {
            get { return GraphStyle.GetStyle("scaleArrow"); }
        }

        public static Texture2D ConnectionPoint
        {
            get { return Resources.Load<Texture2D>("ConnectionPoint"); }
        }      
    }
}