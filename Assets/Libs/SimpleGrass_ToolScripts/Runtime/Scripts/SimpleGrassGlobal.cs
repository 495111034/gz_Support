using UnityEngine;
using UnityEngine.Events;

namespace SimpleGrass
{
    public class SimpleGrassGlobal : MonoBehaviour
    {
        static public SimpleGrassGlobal Global = null;

        public static int PID_HEROSTOP = Shader.PropertyToID("_HeroStop");
        
        public float OffsetY = 1.0f;
        private Vector3 heroPos = Vector3.zero;
        public Vector3 heroFootPos = Vector3.zero;

        public bool isStop = false;
        private Vector3 heroPriorPos = Vector3.zero;
        private Vector3 heroMoveDir = Vector3.zero;
        private float heroStandTime = 0.0f;

        private bool _isUseHeroPos = true;

        ////Hero Trace
        
        public bool GetHeroPos(out Vector3 pos)
        {
            if (heroPos == Vector3.zero)
            {
                pos = Vector3.zero;
                return false;
            }
            pos = heroPos;
            return true;
        }

        static SimpleGrassGlobal()
        {
            GameObject go = new GameObject("GrassGlobal");
            DontDestroyOnLoad(go);
            Global = go.AddComponent<SimpleGrassGlobal>();
            //英雄轨迹
            //SimpleGrassTrace.ClearHeroTraceData();           
        }

        void OnApplicationQuit()
        {
       
        }

        private void Start()
        {

        }

        private void OnDestroy()
        {
            
        }

        private void Update()
        {
            //if (hero == null)
            //{
            //    return;
            //}

            //heroPos = hero.position;
            //heroPos.y += OffsetY;
            //Shader.SetGlobalVector("_HeroPos", heroPos);
        }

        public void UseHeroPos(bool use)
        {
            _isUseHeroPos = use;
            Shader.SetGlobalVector(Common.PID_HEROPOS, new Vector4(heroPos.x, heroPos.y, heroPos.z, _isUseHeroPos ? 1 : 0));
        }

        public void UpdateHeroPos(Vector3 pos)//herofoot pos
        {
            heroFootPos = pos;

            Vector3 oldPos = heroPos;

            heroPos = pos;
            if (heroPos != Vector3.zero)
            {
                heroPos.y += OffsetY;
            }
            //Shader.SetGlobalVector("_HeroPos", heroPos);
            Shader.SetGlobalVector(Common.PID_HEROPOS, new Vector4(heroPos.x , heroPos.y , heroPos.z, _isUseHeroPos ? 1 : 0));

            ///////英雄是否没有移动（0.5秒内）
            if (heroPriorPos == Vector3.zero)
            {
                heroPriorPos = heroPos;
            }
            else
            {
                heroPriorPos = oldPos;
            }
           
            float dist = (heroPos - heroPriorPos).magnitude;

            if (dist < 0.001f)
            {
                heroStandTime += Time.deltaTime;
                isStop = (heroStandTime > 0.5f);                
            }
            else
            {
                heroStandTime = 0.0f;
                isStop = false;
            }
            Shader.SetGlobalFloat(PID_HEROSTOP, isStop ? 1.0f : 0.0f);

            //英雄轨迹：响应位置更新            
            SimpleGrassTrace.OnUpdateHeroTrace?.Invoke(dist, pos, isStop);
            //SimpleGrassTrace.UpdateGrassTraceShaderVars();
        }

        
        public void Test(Camera camera, SimpleInstancingMgr mgr)
        {
            mgr.mainCamera = camera;
            if (mgr.cullingGroup != null)
            {
                mgr.cullingGroup.enabled = false;
                mgr.cullingGroup.targetCamera = camera;
                mgr.cullingGroup.enabled = true;
            }
        }
    }


}