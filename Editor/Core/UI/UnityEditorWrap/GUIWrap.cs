using UnityEngine;

namespace EUTK
{
    public class GUIWrap
    {
        public static bool CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, ref Rect outScreenRect, ref Rect outSourceRect)
        {
            float num1 = position.width / position.height;
            bool flag = false;
            if (scaleMode != ScaleMode.StretchToFill)
            {
                if (scaleMode != ScaleMode.ScaleAndCrop)
                {
                    if (scaleMode == ScaleMode.ScaleToFit)
                    {
                        if (num1 > imageAspect)
                        {
                            float num2 = imageAspect / num1;
                            outScreenRect = new Rect(position.xMin + position.width * (1.0f - num2) * 0.5f, position.yMin, num2 * position.width, position.height);
                            outSourceRect = new Rect(0.0f, 0.0f, 1f, 1f);
                            flag = true;
                        }
                        else
                        {
                            float num2 = num1 / imageAspect;
                            outScreenRect = new Rect(position.xMin, position.yMin + position.height * (1.0f - num2) * 0.5f, position.width, num2 * position.height);
                            outSourceRect = new Rect(0.0f, 0.0f, 1f, 1f);
                            flag = true;
                        }
                    }
                }
                else if (num1 > imageAspect)
                {
                    float height = imageAspect / num1;
                    outScreenRect = position;
                    outSourceRect = new Rect(0.0f, (1.0f - height) * 0.5f, 1f, height);
                    flag = true;
                }
                else
                {
                    float width = num1 / imageAspect;
                    outScreenRect = position;
                    outSourceRect = new Rect(0.5f - width * 0.5f, 0.0f, width, 1f);
                    flag = true;
                }
            }
            else
            {
                outScreenRect = position;
                outSourceRect = new Rect(0.0f, 0.0f, 1f, 1f);
                flag = true;
            }
            return flag;
        }
    }
}