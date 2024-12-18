namespace UnityEngine.UI
{
    public class NScrollIndexCallbackBase : MonoBehaviour
    {
        #region Fields

        private bool m_IsUpdateGameObjectName = true;

        #endregion

        /// <summary>
        /// Gets or sets the IndexID.
        /// </summary>
        public int IndexID { get; set; }

        /// <summary>
        /// Gets or sets the m_Element.
        /// </summary>
        public LayoutElement m_Element { get; set; }

        /// <summary>
        /// Gets the ObjectContent.
        /// </summary>
        public object ObjectContent { get; private set; }

        /// <summary>
        /// Gets or sets the PrefabIndex.
        /// </summary>
        public int PrefabIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the PrefabName.
        /// </summary>
        public string PrefabName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UniqueID.
        /// </summary>
        public string UniqueID { get; set; }

        public virtual void RefreshUI(string ClickUniqueID, object ClickContent)
        {
        }

        public virtual void ScrollCellIndex(int idx, object content, string ClickUniqueID = "", object ClickObject = null)
        {
            IndexID = idx;
            ObjectContent = content;

            if (m_IsUpdateGameObjectName)
            {
                gameObject.name = string.Format("{0} Cell {1}", PrefabName, idx.ToString());
            }
        }

        public void SetIsUpdateGameObjectName(bool value)
        {
            m_IsUpdateGameObjectName = value;
        }

        // Set Element PreferredHeight
        public virtual void SetLayoutElementPreferredHeight(float value)
        {
            m_Element.preferredHeight = value;
        }

        // Set Element PreferredWidth
        public virtual void SetLayoutElementPreferredWidth(float value)
        {
            m_Element.preferredWidth = value;
        }
    }
}
