
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class MessageItemGetPopupAnima : MonoBehaviour
    {

        class MessageItem
        {
            public RectTransform trans = null;
            public CanvasGroup canvasGroup = null;
            public MyImageText myText = null;
            public MessageItem upCell = null;
            public bool isActive = false;
            public int m_nIndex = 0;
            public Vector3 m_nVector3;
            public float m_nSpeed = 0;
            public float m_nHoldSec = 0;
            public string sMsg = "";
            public bool isFormat = false;
            public bool isImageText = false;
            public float nMsgHeight = 0;
        }

        public GameObject go;
        private List<MessageItem> tMsgList = new List<MessageItem>();
        private List<string> tWaitList = new List<string>();
        public int nSpeedUpCount = 4;
        public float nEndPosY = 200;
        public float nBeginPosY = 0;
        public float nDefHoldSec = 1;
        public float nDefFlyTime = 0.8f;
        public int nDefMsgHeight = 30;
        public int startAddSpeedCount = 5;
        public float addSpeed = 2;

        private bool isCanAddWait = false;

        private List<MessageItem> pools = new List<MessageItem>();

        private void Start()
        {
            go.SetActive(false);
            nDefMsgHeight = (int)go.GetRectTransform().sizeDelta.y + 10;
        }

        public void PopupTips(string langId, object pamas, bool bCanRepeat = false)
        {
            string sMsg = MyUITools.UIResPoolInstans.LangFromId(langId, pamas);
            PopupTips(sMsg, bCanRepeat);
        }

        public void PopupTips(string sMsg, bool bCanRepeat = false)
        {
            if (!bCanRepeat)
            {
                if (tWaitList.Count > 0)
                {
                    if (this.tWaitList[tWaitList.Count - 1] == sMsg)
                    {
                        return;
                    }
                }
                else if (tMsgList.Count > 0)
                {
                    MessageItem lastMsg = this.tMsgList[this.tMsgList.Count - 1];
                    if (lastMsg.sMsg == sMsg)
                    {
                        this.tMsgList.RemoveAt(this.tMsgList.Count - 1);
                        this.CheckWaitList();
                        Release(lastMsg);
                    }
                }
            }
            this.ShowWithoutStrip(sMsg);
        }

        private void ShowWithoutStrip(string sMsg)
        {
            if (tMsgList.Count >= 1)
            {
                this.tWaitList.Add(sMsg);
                return;
            }
            this.enabled = true;
            this.AddFlushItemPamas(sMsg);
        }

        private void LateUpdate()
        {
            isCanAddWait = false;
            for (int i = 0; i < tMsgList.Count; i++)
            {
                MessageItem messageItem = tMsgList[i];
                if (messageItem.isFormat)
                {
                    if (messageItem.isImageText && messageItem.myText.transform.childCount > 0)
                    {
                        float height = messageItem.myText.rectTransform.sizeDelta.y;
                        MySpriteImage mySpriteImage = messageItem.myText.transform.GetChild(0).GetComponent<MySpriteImage>();
                        if (mySpriteImage.mainTexture != null)
                        {
                            messageItem.isFormat = false;
                            mySpriteImage.SetNativeSize();
                            int size = (int)mySpriteImage.rectTransform.sizeDelta.x;
                            if (size > 50) size = 50;
                            mySpriteImage.rectTransform.sizeDelta = new Vector2(size, size);
                            height = mySpriteImage.rectTransform.sizeDelta.y;
                            if (height > messageItem.myText.fontSize)
                            {
                                float y = mySpriteImage.rectTransform.sizeDelta.x * 0.5f - messageItem.myText.fontSize * 0.5f - 5;
                                messageItem.myText.transform.localPosition = Vector3.up * y;
                            }
                            messageItem.myText.text = messageItem.sMsg.Replace("size=0", $"size={size}");
                            mySpriteImage.enabled = false;
                            mySpriteImage.enabled = true;
                        }
                    }
                    else
                    {
                        messageItem.isFormat = false;
                    }
                }
                if (i == tMsgList.Count - 1)
                {
                    if (messageItem.trans.localPosition.y - messageItem.nMsgHeight * 0.5f > nBeginPosY)
                    {
                        isCanAddWait = true;
                    }
                }
                int nResult = UpdateMsgLocal(messageItem);
                if (nResult == -1)
                {
                    tMsgList.RemoveAt(i);
                    i--;
                    Release(messageItem);
                }
            }
            if (isCanAddWait)
            {
                CheckWaitList();
            }
            if (tMsgList.Count + this.tWaitList.Count == 0)
            {
                this.enabled = false;
            }
        }

        private void CheckWaitList()
        {
            if (this.tWaitList.Count > 0)
            {
                string tMsgInfo = tWaitList[0];
                tWaitList.RemoveAt(0);
                if (!string.IsNullOrEmpty(tMsgInfo))
                {
                    AddFlushItemPamas(tMsgInfo);
                }
            }
        }

        private MessageItem GetPool()
        {
            MessageItem messageItem;
            if (pools.Count > 0)
            {
                messageItem = pools[0];
                pools.RemoveAt(0);
            }
            else
            {
                messageItem = new MessageItem();
                GameObject goItem = GameObject.Instantiate(go, this.transform);
                messageItem.trans = goItem.transform as RectTransform;
                messageItem.trans.localScale = Vector3.one;
                messageItem.trans.localPosition = Vector3.zero;
                messageItem.canvasGroup = goItem.GetComponentInChildren<CanvasGroup>(true);
                messageItem.myText = goItem.GetComponentInChildren<MyImageText>(true);
                messageItem.myText.MaxWidth = 1000;
            }
            messageItem.isFormat = true;
            messageItem.isActive = true;
            messageItem.myText.rectTransform.localPosition = Vector3.zero;
            return messageItem;
        }

        public void ReleaseAll()
        {
            tWaitList.Clear();
            for (int i = 0; i < tMsgList.Count; i++)
            {
                Release(tMsgList[i]);
            }
            tMsgList.Clear();
        }

        private void Release(MessageItem poolobj)
        {
            poolobj.isActive = false;
            poolobj.trans.gameObject.SetActiveX(false);
            pools.Add(poolobj);
        }

        #region Item
        private void AddFlushItemPamas(string sMsg)
        {
            MessageItem poolObj = GetPool();
            poolObj.trans.localPosition = Vector3.zero;
            poolObj.trans.localScale = Vector3.one;
            poolObj.canvasGroup.alpha = 1;
            poolObj.myText.text = sMsg;
            poolObj.isImageText = sMsg.IndexOf("<quad") > -1;
            poolObj.sMsg = sMsg;
            poolObj.m_nHoldSec = nDefHoldSec;
            poolObj.nMsgHeight = nDefMsgHeight;
            poolObj.upCell = null;
            if (tMsgList.Count > 0)
            {
                poolObj.upCell = tMsgList[tMsgList.Count - 1];
                poolObj.m_nVector3 = new Vector3(0, poolObj.upCell.m_nVector3.y - poolObj.nMsgHeight, 0);
            }
            else
            {
                poolObj.m_nVector3 = new Vector3(0, nEndPosY, 0);
            }
            poolObj.trans.localPosition = new Vector3(0, nBeginPosY, 0);
            poolObj.m_nSpeed = (nEndPosY - poolObj.trans.localPosition.y) / nDefFlyTime;
            poolObj.trans.gameObject.SetActiveX(true);
            poolObj.m_nIndex = tMsgList.Count;
            tMsgList.Add(poolObj);
        }


        private int UpdateMsgLocal(MessageItem poolObj)
        {
            if (poolObj.m_nVector3.y <= poolObj.trans.localPosition.y)
            {
                if (poolObj.upCell != null)
                {
                    if (poolObj.upCell.isActive && poolObj.upCell.trans.localPosition.y > poolObj.trans.localPosition.y)
                    {
                        poolObj.m_nVector3.y = poolObj.upCell.m_nVector3.y - poolObj.nMsgHeight;
                    }
                    else
                    {
                        poolObj.upCell = null;
                        poolObj.m_nVector3.y = nEndPosY;
                    }
                }
                else
                {
                    if (tMsgList.Count + tWaitList.Count > nSpeedUpCount)
                    {
                        poolObj.m_nHoldSec = 0;
                    }
                    else
                    {
                        poolObj.m_nHoldSec = poolObj.m_nHoldSec - Time.deltaTime;
                    }
                }
                if ((poolObj.m_nIndex == 0 || poolObj.trans.localPosition.y >= (nEndPosY - poolObj.nMsgHeight)) && poolObj.m_nHoldSec <= 0)
                {
                    return -1;
                }
                return 0;
            }
            float nPosY = Time.deltaTime * poolObj.m_nSpeed * (tMsgList.Count > startAddSpeedCount ? addSpeed : 1);
            Vector3 oPos = poolObj.trans.localPosition;
            if (oPos.y + nPosY > poolObj.m_nVector3.y)
            {
                nPosY = poolObj.m_nVector3.y - oPos.y;
            }
            poolObj.trans.localPosition = new Vector3(oPos.x, oPos.y + nPosY);
            return 0;
        }
        #endregion
    }
}
