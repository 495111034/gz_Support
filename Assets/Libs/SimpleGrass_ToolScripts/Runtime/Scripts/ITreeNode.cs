using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGrass
{
    public interface ITreeNode
    {
        string Code { get; set; }

        Bounds Bound { get; set; }


        bool MoveInView { get; set; }
        /// <summary>
        /// 初始化插入一个数据
        /// </summary>
        /// <param name="obj"></param>
        //void InsertObj(ObjData obj);
        ITreeNode InsertObj(Vector3 objPos);

        ITreeNode BuildNode(Vector3 NodePos);
        /// <summary>
        /// 当触发者（摄像头）移动时显示/隐藏物体
        /// </summary>
        /// <param name="camera"></param>
        // void TriggerMove(Camera camera);
        void TriggerInView(Matrix4x4 cameraMat);

        void TriggerPosition(Vector3 cameraPositon);

        void DrawBound(int maxDepth);
    }
}
