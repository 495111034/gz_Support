using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Entity
{
    public enum FightPositionType
    {
        FightPositionCommon = 1,
        FightPositionBoss = 2,
        FightPositionRaid = 3,
        FightPositionDanger = 4,
    }

    [System.Serializable]
    public class ScenePointList
    {
#if UNITY_EDITOR
        //public GameObject OurCenterObj;
        //public GameObject EnemyCenterObj;
        //public GameObject SceneCenterObj;
        //public List<GameObject> OurPosListObj;
        //public List<GameObject> EnemyPosListObj;
        //public List<GameObject> PetPosListObj;

        //public void SaveData()
        //{
        //    if (OurCenterObj)
        //    {
        //        OurCenterPosition = OurCenterObj.transform.position;
        //        OurCenterRotation = OurCenterObj.transform.rotation.eulerAngles;
        //    }

        //    if (EnemyCenterObj)
        //    {
        //        EnemyCenterPosition = EnemyCenterObj.transform.position;
        //        EnemyCenterRotation = EnemyCenterObj.transform.rotation.eulerAngles;
        //    }

        //    if (SceneCenterObj)
        //    {
        //        SceneCenterPosition = SceneCenterObj.transform.position;
        //        SceneCenterRotation = SceneCenterObj.transform.rotation.eulerAngles;
        //    }


        //    if (OurPosListObj != null)
        //    {
        //        OurPositionList = new List<Vector3>();
        //        OurRotationList = new List<Vector3>();
        //        for (int i = 0; i < OurPosListObj.Count; ++i)
        //        {
        //            if (OurPosListObj[i])
        //            {
        //                OurPositionList.Add(OurPosListObj[i].transform.position);
        //                OurRotationList.Add(OurPosListObj[i].transform.rotation.eulerAngles);
        //            }
        //        }
                
        //    }

        //    if (PetPosListObj != null)
        //    {
        //        PetPositionList = new List<Vector3>();
        //        PetRotationList = new List<Vector3>();
        //        for (int i = 0; i < PetPosListObj.Count; ++i)
        //        {
        //            if (PetPosListObj[i])
        //            {
        //                PetPositionList.Add(PetPosListObj[i].transform.position);
        //                PetRotationList.Add(PetPosListObj[i].transform.rotation.eulerAngles);
        //            }
        //        }

        //    }

        //    if (EnemyPosListObj != null)
        //    {
        //        EnemyPositionList = new List<Vector3>();
        //        EnemyRotationList = new List<Vector3>();
        //        for (int i = 0; i < EnemyPosListObj.Count; ++i)
        //        {
        //            if (EnemyPosListObj[i])
        //            {
        //                EnemyPositionList.Add(EnemyPosListObj[i].transform.position);
        //                EnemyRotationList.Add(EnemyPosListObj[i].transform.rotation.eulerAngles);
        //            }
        //        }
                
        //    }
            
        //}
#endif

        //[HideInInspector]
        public Vector3 OurCenterPosition;
        //[HideInInspector]
        public Vector3 EnemyCenterPosition;
        //[HideInInspector]
        public Vector3 SceneCenterPosition;
       // [HideInInspector]
        public List<Vector3> OurPositionList;
       // [HideInInspector]
        public List<Vector3> EnemyPositionList;
       // [HideInInspector]
        public Vector3 OurCenterRotation;
        //[HideInInspector]
        public Vector3 EnemyCenterRotation;
       // [HideInInspector]
        public Vector3 SceneCenterRotation;
        //[HideInInspector]
        public List<Vector3> OurRotationList;
       // [HideInInspector]
        public List<Vector3> EnemyRotationList;
       // [HideInInspector]
        public List<Vector3> PetRotationList;
        //[HideInInspector]
        public List<Vector3> PetPositionList;
    }

    [System.Serializable]
    public class FithtCameraConfig
    {
        public float FiledView;
        public SkillTargetType LookatTarget;
        public float AngleX;
        public float AngleY;
        public float Distance;
        public Vector3 TargetOffset;
        public Vector3 PetOffset;
    }

    [System.Serializable]
    public class SceneDataBase
    {
        public int id;
        public string SceneTitle;

        public ScenePointList commonPoints;
        public ScenePointList bossPoints;
        public ScenePointList raidPoints;
        public ScenePointList dangerPoints;

        public FithtCameraConfig CommonCamera;
        public FithtCameraConfig BossCamera;
        public FithtCameraConfig RaidCamera;
        public FithtCameraConfig DangerCamera;

#if UNITY_EDITOR
        public void SaveData()
        {
            //if (commonPoints != null) commonPoints.SaveData();
            //if (bossPoints != null) bossPoints.SaveData();
            //if (raidPoints != null) raidPoints.SaveData();
            //if (dangerPoints != null) dangerPoints.SaveData();            
        }
#endif
    }

}
