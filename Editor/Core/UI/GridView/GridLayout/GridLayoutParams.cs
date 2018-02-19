using UnityEngine;

namespace EUTK
{
    public class GridLayoutParams
    {
        private int m_Columns = 1;
        private int m_Rows;
        private float m_Height;
        private float m_HorizontalSpacing;

        /// <summary>
        /// 最小的Icon大小
        /// </summary>
        public int MinIconSize
        {
            get { return 32; }
        }

        /// <summary>
        /// 最小的网格大小
        /// </summary>
        public int MinGridSize
        {
            get { return 16; }
        }

        /// <summary>
        /// 最大的网格大小
        /// </summary>
        public int MaxGridSize
        {
            get { return 96; }
        }

        /// <summary>
        /// 是否是列表模式，false为网格模式
        /// </summary>
        public bool ListMode { get; set; }

        /// <summary>
        /// 网格布局的列数
        /// </summary>
        public int Columns
        {
            get { return m_Columns; }
        }

        /// <summary>
        /// 网格布局的列数
        /// </summary>
        public int Rows
        {
            get { return m_Rows; }
        }

        /// <summary>
        /// 所有Item对象布局后需要的高度
        /// </summary>
        public float Height
        {
            get { return m_Height; }
        }

        /// <summary>
        /// 网格Item之间的水平间隔
        /// </summary>
        public float HorizontalSpacing
        {
            get { return m_HorizontalSpacing; }
        }

        /// <summary>
        /// 网格显示区域的固定宽度
        /// </summary>
        public float FixedWidth { get; set; }

        /// <summary>
        /// 网格Item的大小
        /// </summary>
        public Vector2 ItemSize { get; set; }

        /// <summary>
        /// 网格Item的竖直间隔
        /// </summary>
        public float VerticalSpacing { get; set; }

        /// <summary>
        /// 网格Item之间的最小水平间隔
        /// </summary>
        public float MinHorizontalSpacing { get; set; }

        /// <summary>
        /// 网格Item实际显示区域距离显示整个布局显示区域的上间隔
        /// </summary>
        public float TopMargin { get; set; }

        /// <summary>
        /// 网格Item实际显示区域距离显示整个布局显示区域的下间隔
        /// </summary>
        public float BottomMargin { get; set; }

        /// <summary>
        /// 网格Item实际显示区域距离显示整个布局显示区域的右间隔
        /// </summary>
        public float RightMargin { get; set; }

        /// <summary>
        /// 网格Item实际显示区域距离显示整个布局显示区域的左间隔
        /// </summary>
        public float LeftMargin { get; set; }

        public void CalculateLayoutParams(int itemCount, int maxNumRows)
        {
            m_Columns = CalculateColumns();
            m_HorizontalSpacing = Mathf.Max(0.0f,
                (FixedWidth - (m_Columns * ItemSize.x + LeftMargin + RightMargin)) / m_Columns);
            m_Rows = Mathf.Min(maxNumRows, CalculateRows(itemCount));
            if (m_Rows == 1)
                m_HorizontalSpacing = MinHorizontalSpacing;
            m_Height = m_Rows * (ItemSize.y + VerticalSpacing) - VerticalSpacing + TopMargin + BottomMargin;
        }

        public int CalculateColumns()
        {
            return Mathf.Max(
                (int)Mathf.Floor(((FixedWidth - LeftMargin - RightMargin) / (ItemSize.x + MinHorizontalSpacing))), 1);
        }

        public int CalculateRows(int itemCount)
        {
            int num = (int)Mathf.Ceil(itemCount / (float)CalculateColumns());
            if (num < 0)
                return int.MaxValue;
            return num;
        }

        public Rect CalculateItemRect(int itemIndex)
        {
            float num1 = Mathf.Floor((float)itemIndex / Columns);
            float num2 = itemIndex - num1 * Columns;
            return new Rect((float)(LeftMargin + HorizontalSpacing * 0.5 + num2 * (ItemSize.x + HorizontalSpacing)),
                num1 * (ItemSize.y + VerticalSpacing) + TopMargin, ItemSize.x, ItemSize.y);
        }

        public int GetMaxVisibleItems(float height)
        {
            return (int)Mathf.Ceil(((height - TopMargin - BottomMargin) / (ItemSize.y + VerticalSpacing))) * Columns;
        }

        public bool IsVisibleInScrollView(float scrollViewHeight, float scrollPos, float gridStartY, int maxIndex,
            out int startIndex, out int endIndex)
        {
            startIndex = endIndex = 0;
            float num1 = scrollPos;
            float num2 = scrollPos + scrollViewHeight;
            float num3 = gridStartY + TopMargin;
            if (num3 > num2 || num3 + Height < num1)
                return false;
            float num4 = ItemSize.y + VerticalSpacing;
            int num5 = Mathf.FloorToInt((num1 - num3) / num4);
            startIndex = num5 * Columns;
            startIndex = Mathf.Clamp(startIndex, 0, maxIndex);
            int num6 = Mathf.FloorToInt((num2 - num3) / num4);
            endIndex = (num6 + 1) * Columns - 1;
            endIndex = Mathf.Clamp(endIndex, 0, maxIndex);
            return true;
        }
    }
}

