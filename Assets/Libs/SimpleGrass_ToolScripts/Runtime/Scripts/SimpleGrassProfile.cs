using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleGrass{

    //[CreateAssetMenu (fileName = "SimpleGrassProfile", menuName = "SimpleGrass Profile", order = 100)]
    public class SimpleGrassProfile : ScriptableObject
    {
        public string KindName = "";

        public GameObject GrassChunkPrefab = null;

        [Range(1, 10000)]
        public int Editor_ViewDist = 300;

        //[Tooltip("可种植掩码")]
        public LayerMask PaintingLayerMask = -1;

       
        //[Tooltip("随机朝向")]
        public bool RandomRot = true;

        [Range(0f, 360.0f)]
        public float RandomRotMin = 0;

        [Range(0f, 360.0f)]
        public float RandomRotMax = 360;

        //[Tooltip("随机最终缩放比例")]
        public Vector2 EndMinMaxScale = new Vector2(1.0f, 1.0f);

       // [Tooltip("半径")]
        public float BrushRadius = 5;//maxspread

        //[Tooltip("密度")]
        public Vector2 Density = new Vector2(0.0f, 0.0f);

        [Range(0.01f, 10000.0f)]
        public float StartRadi = 1f;

        [Range(0.01f, 10000.0f)]
        public float StartLength = 1f;

        [Range(0.01f, 10000.0f)]
        public float MinRadi = 1f;

        //[Tooltip("种值回避距离")]
        [Range(0.01f, 10000.0f)]
        public float MinAvoidDist = 0.1f;

        //[Tooltip("是否在交点法线上种植")]
        public bool OnNormal = true;

        //[Tooltip("是否跟随父对象")]
        [HideInInspector]
        public bool MoveWithObject = false;

        //是否批量删除
        [HideInInspector]
        public bool BrushErase = true;

        //是否需要交互
        [HideInInspector]
        public bool Interactive = false;

        [Range(0.01f, 10000.0f)]
        public float RayCastDist = 10f;






        public void Load(SimpleGrassSys grass)
        {
            grass.KindName = KindName;
            grass.GrassChunkPrefab = GrassChunkPrefab;
            grass.RandomRot = RandomRot;
            grass.RandomRotMin = Mathf.Clamp(RandomRotMin, 0.0f, 360);
            grass.RandomRotMax = Mathf.Clamp(RandomRotMax, 0.0f, 360);
            
            grass.EndMinMaxScale = EndMinMaxScale;            
            grass.BrushRadius = BrushRadius;
            grass.Density = Density;
            grass.StartRadi = StartRadi;
            grass.StartLength = StartLength;
            grass.MinRadi = MinRadi;
            grass.MinAvoidDist = MinAvoidDist;
            grass.OnNormal = OnNormal;
            grass.MoveWithObject = MoveWithObject;
            grass.BrushErase = BrushErase;
            grass.Interactive = Interactive;
            grass.RayCastDist = RayCastDist;
            grass.PaintingLayerMask = PaintingLayerMask;
            grass.Editor_ViewDist = Editor_ViewDist;
        }

        public void Save(SimpleGrassSys grass)
        {
            KindName = grass.KindName;
            GrassChunkPrefab = grass.GrassChunkPrefab;
            RandomRot = grass.RandomRot;
            RandomRotMin = Mathf.Clamp(grass.RandomRotMin, 0.0f, 360);
            RandomRotMax = Mathf.Clamp(grass.RandomRotMax, 0.0f, 360);
            EndMinMaxScale = grass.EndMinMaxScale;
            BrushRadius = grass.BrushRadius;
            Density = grass.Density;
            StartRadi = grass.StartRadi;
            StartLength = grass.StartLength;
            MinRadi = grass.MinRadi;
            MinAvoidDist = grass.MinAvoidDist;
            OnNormal = grass.OnNormal;
            MoveWithObject = grass.MoveWithObject;
            BrushErase = grass.BrushErase;
            Interactive = grass.Interactive;
            RayCastDist = grass.RayCastDist;
            PaintingLayerMask = grass.PaintingLayerMask;
            Editor_ViewDist = grass.Editor_ViewDist;
        }

            // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
