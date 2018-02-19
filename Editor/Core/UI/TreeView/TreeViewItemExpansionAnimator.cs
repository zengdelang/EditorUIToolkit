using System;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class TreeViewItemExpansionAnimator
    {
        private TreeViewAnimationInput m_Setup;
        private bool m_InsideGUIClip;
        private Rect m_CurrentClipRect;
        private static bool s_Debug = false;

        public float expandedValueNormalized
        {
            get
            {
                float elapsedTimeNormalized = m_Setup.elapsedTimeNormalized;
                if (m_Setup.expanding)
                    return elapsedTimeNormalized;
                return 1f - elapsedTimeNormalized;
            }
        }

        public int startRow
        {
            get { return m_Setup.startRow; }
        }

        public int endRow
        {
            get { return m_Setup.endRow; }
        }

        public float deltaHeight
        {
            get { return m_Setup.rowsRect.height - m_Setup.rowsRect.height * expandedValueNormalized; }
        }

        public bool isAnimating
        {
            get { return m_Setup != null; }
        }

        public bool isExpanding
        {
            get { return m_Setup.expanding; }
        }

        private bool printDebug
        {
            get
            {
                if (s_Debug && m_Setup != null && m_Setup.treeView != null)
                    return Event.current.type == EventType.Repaint;
                return false;
            }
        }

        public void BeginAnimating(TreeViewAnimationInput setup)
        {
            if (m_Setup != null)
            {
                if (m_Setup.item.id == setup.item.id)
                {
                    if (m_Setup.elapsedTime >= 0.0)
                        setup.elapsedTime = m_Setup.animationDuration - m_Setup.elapsedTime;
                    else
                        Debug.LogError(("Invaid duration " + m_Setup.elapsedTime));
                    m_Setup = setup;
                }
                else
                {
                    m_Setup.FireAnimationEndedEvent();
                    m_Setup = setup;
                }
                m_Setup.expanding = setup.expanding;
            }
            m_Setup = setup;
            if (m_Setup == null)
                Debug.LogError("Setup is null");
            if (printDebug)
                Console.WriteLine("Begin animating: " + m_Setup);
            m_CurrentClipRect = GetCurrentClippingRect();
        }

        public bool CullRow(int row, ITreeViewGUI gui)
        {
            if (!isAnimating)
                return false;
            if (printDebug && row == 0)
                Console.WriteLine("--------");
            if (row <= m_Setup.startRow || row > m_Setup.endRow ||
                (double)gui.GetRowRect(row, 1f).y - m_Setup.startRowRect.y <= m_CurrentClipRect.height)
                return false;
            if (m_InsideGUIClip)
                EndClip();
            return true;
        }

        public void OnRowGUI(int row)
        {
            if (!printDebug)
                return;
            Console.WriteLine(row + " Do item " + DebugItemName(row));
        }

        public Rect OnBeginRowGUI(int row, Rect rowRect)
        {
            if (!isAnimating)
                return rowRect;
            if (row == m_Setup.startRow)
                BeginClip();
            if (row >= m_Setup.startRow && row <= m_Setup.endRow)
                rowRect.y -= m_Setup.startRowRect.y;
            else if (row > m_Setup.endRow)
                rowRect.y -= m_Setup.rowsRect.height - m_CurrentClipRect.height;
            return rowRect;
        }

        public void OnEndRowGUI(int row)
        {
            if (!isAnimating || !m_InsideGUIClip || row != m_Setup.endRow)
                return;
            EndClip();
        }

        private void BeginClip()
        {
            GUI.BeginClip(m_CurrentClipRect);
            m_InsideGUIClip = true;
            if (!printDebug)
                return;
            Console.WriteLine("BeginClip startRow: " + m_Setup.startRow);
        }

        private void EndClip()
        {
            GUI.EndClip();
            m_InsideGUIClip = false;
            if (!printDebug)
                return;
            Console.WriteLine("EndClip endRow: " + m_Setup.endRow);
        }

        public void OnBeforeAllRowsGUI()
        {
            if (!isAnimating)
                return;
            m_CurrentClipRect = GetCurrentClippingRect();
            if (m_Setup.elapsedTime <= m_Setup.animationDuration)
                return;
            m_Setup.FireAnimationEndedEvent();
            m_Setup = null;
            if (!printDebug)
                return;
            Debug.Log("Animation ended");
        }

        public void OnAfterAllRowsGUI()
        {
            if (m_InsideGUIClip)
                EndClip();
            if (!isAnimating)
                return;
            HandleUtility.Repaint();
        }

        public bool IsAnimating(int itemID)
        {
            if (!isAnimating)
                return false;
            return m_Setup.item.id == itemID;
        }

        private Rect GetCurrentClippingRect()
        {
            Rect rowsRect = m_Setup.rowsRect;
            rowsRect.height *= expandedValueNormalized;
            return rowsRect;
        }

        private string DebugItemName(int row)
        {
            return m_Setup.treeView.data.GetRows()[row].displayName;
        }
    }
}