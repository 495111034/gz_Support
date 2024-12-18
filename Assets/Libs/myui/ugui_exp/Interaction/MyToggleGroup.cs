using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/My Toggle Group", 32)]
    [DisallowMultipleComponent]
    public class MyToggleGroup : UIBehaviour
    {
        [SerializeField] private bool m_AllowSwitchOff = false;
        public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

        private List<MyToggle> m_Toggles = new List<MyToggle>();

        protected MyToggleGroup()
        { }

        private void ValidateToggleIsInGroup(MyToggle toggle)
        {
            if (toggle == null || !m_Toggles.Contains(toggle))
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
        }

        public void NotifyToggleOn(MyToggle toggle)
        {
            ValidateToggleIsInGroup(toggle);

            // disable all toggles in the group
            for (var i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i] == toggle)
                    continue;

                m_Toggles[i].isOn = false;
            }
        }

        public MyToggle SelectItemById(string id,bool raiseError=true)
        {
            MyToggle toggle = null;
            for (int i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i].name == id)
                {
                    toggle = m_Toggles[i];
                    break;
                }
            }
            if (toggle != null)
            {
                toggle.isOn = true;
                NotifyToggleOn(toggle);
            }
            else if(raiseError)
            {
                if(Application.isEditor) Log.LogInfo($"not find toggle {id}");
            }
            return toggle;
        }
        /// <summary>
        /// 用于ToggleGroup
        /// </summary>
        /// <param name="index"></param>
        public void SelectItemByIndex(int index)
        {
            for (int i = 0; i < m_Toggles.Count; i++)
            {
                var toggle = m_Toggles[i];
                toggle.isOn = index == i;
            }   
        }
        public void UnregisterToggle(MyToggle toggle)
        {
            if (m_Toggles.Contains(toggle))
                m_Toggles.Remove(toggle);
        }

        public void RegisterToggle(MyToggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
                m_Toggles.Add(toggle);
        }

        public bool AnyTogglesOn()
        {
            return m_Toggles.Find(x => x.isOn) != null;
        }

        public IEnumerable<MyToggle> ActiveToggles()
        {
            return m_Toggles.Where(x => x.isOn);
        }

        public void SetAllTogglesOff()
        {
            bool oldAllowSwitchOff = m_AllowSwitchOff;
            m_AllowSwitchOff = true;

            for (var i = 0; i < m_Toggles.Count; i++)
                m_Toggles[i].isOn = false;

            m_AllowSwitchOff = oldAllowSwitchOff;
        }

        public MyToggle GetFirstSelected()
        {
            for (var i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i].isOn)
                {
                    return m_Toggles[i];
                }
            }
            return null;
        }

        public int GetFirstSelectedIndex()
        {
            for (var i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i].isOn)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool HasSelection()
        {
            for (int i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i].isOn)
                {
                    return true;
                }
            }
            return false;
        }
        
        public object GetSelectedItemData()
        {
            var item = GetFirstSelected();
            return item != null ? item.GetParam() : null;
        }
        
        public List<MyToggle> GetAllToggles()
        {
            return m_Toggles;
        }
    }
}
