    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/TextOneByOne逐字出现")]
    public class TextOneByOne : MonoBehaviour
    {
        public float charsPerSecond = 0;//打字时间间隔
        public string words;//需要显示的文字

        private bool isActive = false;
        private float timer;//计时器
        private MyText myText;
        private int currentPos = 0;//当前打字位置

        bool isToEnd = false;
        void OnEnable()
        {
            timer = 0;
            isActive = words.Length > 0;
            isToEnd = charsPerSecond == 0;
            myText = GetComponent<MyText>();
            myText.text = "";
        }

        private void OnDisable()
        {
            isActive = false;
            timer = 0;
            currentPos = 0;
            myText.text = "";
        }

        private void FixedUpdate()
        {
            OnStartWriter();
        }

        /// <summary>
        /// 执行打字任务
        /// </summary>
        void OnStartWriter()
        {
            if (isActive)
            {
                timer += Time.fixedDeltaTime;
                if (isToEnd)
                {
                    currentPos = words.Length;
                }
                else
                {
                    currentPos = Mathf.FloorToInt(timer / charsPerSecond);
                }
                if (currentPos > words.Length) currentPos = words.Length;
                myText.text = words.Substring(0, currentPos);//刷新文本显示内容

                if (currentPos >= words.Length)
                {
                    OnFinish();
                    SendMessageUpwards("__OnTextEffectFinish", this);
                }
            }
        }
        /// <summary>
        /// 结束打字，初始化数据
        /// </summary>
        void OnFinish()
        {
            isActive = false;
            timer = 0;
            currentPos = 0;
            myText.text = words;
        }

        //跳到本一行的结尾
        public void ToEnd()
        {
            isToEnd = true;
        }
    }
}