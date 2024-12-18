using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SG
{
    [RequireComponent(typeof(UnityEngine.UI.LoopScrollRect))]
    [DisallowMultipleComponent]
    public class InitOnStart : MonoBehaviour
    {
		public int total_count = 100;
        void Start()
        {
            var scroll = GetComponent<LoopScrollRect>();

            ///RefillCells,初始化layoutgroup内容
            ///参数1：Action<Transform, int> ,新节点被创建时的回调,回调参数1为节点transform，参数2为节点索引号；
            ///参数2：Action<Transform>    ,节点被回收时的回调
            ///参数3：节点数
            ///参数4：要刷新的节点偏移量，为空表示全部刷新            

            scroll.RefillCells(
                (transform,idx)=>
                {
                    if(scroll.reverseDirection)
                    {
                        transform.gameObject.name = $"item_{scroll.totalCount - idx}";
                        if(transform.gameObject.FindInChild<MyText>(""))
                            transform.gameObject.FindInChild<MyText>("").text = $"编号{scroll.totalCount - idx}";
                    }
                    else
                    {
                        transform.gameObject.name = $"item_{idx}";
                        if(transform.gameObject.FindInChild<MyText>(""))
                            transform.gameObject.FindInChild<MyText>("").text = $"编号{ idx}";
                    }

                    
                }
                ,
                (transform)=>
                {                
                    if(transform.gameObject.FindInChild<MyText>(""))
                        transform.gameObject.FindInChild<MyText>("").text = "";
                },
                total_count);

            
        }
    }
}