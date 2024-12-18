using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGrass
{

    
    public class SimpleGrassProtoInfo : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        public bool inited = false;

        [HideInInspector]
        [SerializeField]
        string backupTag;
        public string BackupTag
        {
            get { return backupTag; }
            set { backupTag = value; }
        }

        [HideInInspector]
        [SerializeField]
        float cullingMaxDistance = Common.DefMaxDistance;
        public float CullingMaxDistance
        {
            get { return cullingMaxDistance; }
            set { cullingMaxDistance = value; }
        }

        [HideInInspector]
        [SerializeField]
        float mergeChunkDistance = Common.MergeChunkDistance;
        public float MergeChunkDistance
        {
            get { return mergeChunkDistance; }
            set { mergeChunkDistance = value; }
        }

        [HideInInspector]
        [SerializeField]
        bool castShadows = false;
        public bool CastShadows
        {
            get { return castShadows; }
            set { castShadows = value; }
        }

        [HideInInspector]
        [SerializeField]
        bool receiveShadows = false;
        public bool ReceiveShadows
        {
            get { return receiveShadows; }
            set { receiveShadows = value; }
        }

        [HideInInspector]
        [SerializeField]
        int layerID = -1;
        public int LayerID
        {
            get { return layerID; }
            set { layerID = value; }
        }

        //显示密度
        [HideInInspector]        
        [SerializeField]
        float density = 1.0f;
        public float Density
        {
            get { return density; }
            set { density = value; }
        }

        //批量增加的带LOD物件，是否进行批处理（BatchMode_LOD模式显示）
        public bool batchMeshLOD = false;

        [HideInInspector]
        [SerializeField]
        public bool useLightProbe = false;

        private void Start()
        {
            gameObject.SetActive(false);
        }

    }

}