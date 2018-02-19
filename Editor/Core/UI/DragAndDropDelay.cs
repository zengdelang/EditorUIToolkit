using UnityEngine;

namespace EUTK
{
    public class DragAndDropDelay
    {
        public Vector2 mouseDownPosition;

        public bool CanStartDrag()
        {
            return Vector2.Distance(mouseDownPosition, Event.current.mousePosition) > 6.0;
        }
    }
}
