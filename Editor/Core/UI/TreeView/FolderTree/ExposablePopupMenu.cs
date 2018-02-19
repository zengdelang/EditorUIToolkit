using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EUTK
{
    public class ExposablePopupMenu
    {
        private Action<ItemData> m_SelectionChangedCallback;
        private List<ItemData> m_Items;
        private float m_WidthOfButtons;
        private float m_ItemSpacing;
        private PopupButtonData m_PopupButtonData;
        //private float m_WidthOfPopup;
       // private float m_MinWidthOfPopup;

        public void Init(List<ItemData> items, float itemSpacing, float minWidthOfPopup, PopupButtonData popupButtonData, Action<ItemData> selectionChangedCallback)
        {
            m_Items = items;
            m_ItemSpacing = itemSpacing;
            m_PopupButtonData = popupButtonData;
            m_SelectionChangedCallback = selectionChangedCallback;
            //m_MinWidthOfPopup = minWidthOfPopup;
            CalcWidths();
        }

        public float OnGUI(Rect rect)
        {
            //if (rect.width >= m_WidthOfButtons && rect.width > m_MinWidthOfPopup)
            {
                Rect position = rect;
                foreach (ItemData itemData in m_Items)
                {
                    position.width = itemData.m_Width;
                    EditorGUI.BeginChangeCheck();
                    using (new EditorGUI.DisabledScope(!itemData.m_Enabled))
                        GUI.Toggle(position, itemData.m_On, itemData.m_GUIContent, itemData.m_Style);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SelectionChanged(itemData);
                        GUIUtility.ExitGUI();
                    }
                    position.x += itemData.m_Width + m_ItemSpacing;
                }
                return m_WidthOfButtons;
            }
           /* if (m_WidthOfPopup < rect.width)
                rect.width = m_WidthOfPopup;
            //if (EditorGUI.DropdownButton(rect, m_PopupButtonData.m_GUIContent, FocusType.Passive, m_PopupButtonData.m_Style))
            //    PopUpMenu.Show(rect, m_Items, this);
            return m_WidthOfPopup;*/
        }

        private void CalcWidths()
        {
            m_WidthOfButtons = 0.0f;
            foreach (ItemData itemData in m_Items)
            {
                itemData.m_Width = itemData.m_Style.CalcSize(itemData.m_GUIContent).x;
                m_WidthOfButtons += itemData.m_Width;
            }
            m_WidthOfButtons += (m_Items.Count - 1) * m_ItemSpacing;
            Vector2 vector2 = m_PopupButtonData.m_Style.CalcSize(m_PopupButtonData.m_GUIContent);
            vector2.x += 3f;
           // m_WidthOfPopup = vector2.x;
        }

        private void SelectionChanged(ItemData item)
        {
            if (m_SelectionChangedCallback != null)
                m_SelectionChangedCallback(item);
            else
                Debug.LogError("Callback is null");
        }

        public class ItemData
        {
            public GUIContent m_GUIContent;
            public GUIStyle m_Style;
            public bool m_On;
            public bool m_Enabled;
            public object m_UserData;
            public float m_Width;

            public ItemData(GUIContent content, GUIStyle style, bool on, bool enabled, object userData)
            {
                m_GUIContent = content;
                m_Style = style;
                m_On = on;
                m_Enabled = enabled;
                m_UserData = userData;
            }
        }

        public class PopupButtonData
        {
            public GUIContent m_GUIContent;
            public GUIStyle m_Style;

            public PopupButtonData(GUIContent content, GUIStyle style)
            {
                m_GUIContent = content;
                m_Style = style;
            }
        }

        internal class PopUpMenu
        {
            private static List<ItemData> m_Data;
            private static ExposablePopupMenu m_Caller;

            internal static void Show(Rect activatorRect, List<ItemData> buttonData, ExposablePopupMenu caller)
            {
                m_Data = buttonData;
                m_Caller = caller;
                GenericMenu genericMenu1 = new GenericMenu();
                foreach (ItemData itemData1 in m_Data)
                {
                    if (itemData1.m_Enabled)
                    {
                       /* GenericMenu genericMenu2 = genericMenu1;
                        GUIContent guiContent = itemData1.m_GUIContent;
                        int num = itemData1.m_On ? 1 : 0;*/
                        /*  // ISSUE: reference to a compiler-generated field
                          if (ExposablePopupMenu.PopUpMenu.\u003C\u003Ef__mg\u0024cache0 == null)
                          {
                              // ISSUE: reference to a compiler-generated field
                              ExposablePopupMenu.PopUpMenu.\u003C\u003Ef__mg\u0024cache0 = new GenericMenu.MenuFunction2(ExposablePopupMenu.PopUpMenu.SelectionCallback);
                          }
                          // ISSUE: reference to a compiler-generated field
                          GenericMenu.MenuFunction2 fMgCache0 = ExposablePopupMenu.PopUpMenu.\u003C\u003Ef__mg\u0024cache0;*/
                        //  ExposablePopupMenu.ItemData itemData2 = itemData1;
                        //   genericMenu2.AddItem(guiContent, num != 0, fMgCache0, (object)itemData2);
                    }
                    else
                        genericMenu1.AddDisabledItem(itemData1.m_GUIContent);
                }
                genericMenu1.DropDown(activatorRect);
            }

            private static void SelectionCallback(object userData)
            {
                ItemData itemData = (ItemData)userData;
                m_Caller.SelectionChanged(itemData);
                m_Caller = null;
                m_Data = null;
            }
        }
    }
}

