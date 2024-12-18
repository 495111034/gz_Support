using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SimpleGrass
{

    [Serializable]
    public class SimpleGrassLightmapDic
    {
        [Serializable]
        public struct LightmapDatas
        {
            public int index;
            public Texture2D lightmapColor;
            public Texture2D shadowMask;
            public Texture2D lightmapDir;
        };

        public List<LightmapDatas> lightmapDatas = new List<LightmapDatas>();

        public SimpleGrassLightmapDic()
        {
            lightmapDatas = new List<LightmapDatas>();
        }

        //从保存的植被数据中，生成lightmapDatas数据
        public void Refresh(SimpleSaveData savedProfile)
        {             
            SortedSet<int> dict = new SortedSet<int>();
            //savedProfile.GetAllLightmapIndexes(ref dict);
            ProtoTypeData.GetAllLightmapIndexes(savedProfile.ProtoTypes, ref dict);
            RefreshByList(dict);            
        }

        //生成lightmapDatas数据
        public void RefreshByList(SortedSet<int> dict)
        {          
            lightmapDatas.Clear();          
            foreach (var item in dict)
            {
                if (item >= 0 && (item < LightmapSettings.lightmaps.Length))
                {
                    LightmapDatas data = new LightmapDatas();
                    data.index = item;
                    data.lightmapColor = LightmapSettings.lightmaps[item].lightmapColor;
                    data.shadowMask = LightmapSettings.lightmaps[item].shadowMask;
                    data.lightmapDir = LightmapSettings.lightmaps[item].lightmapDir;
                    lightmapDatas.Add(data);

                }
            }
        }
        public int Count()
        {
            return lightmapDatas.Count;
        }
        public bool GetLightMaps(int index, out Texture2D lightmapColor, out Texture2D shadowMask, out Texture2D lightmapDir)
        {
            lightmapColor = null;
            shadowMask = null;
            lightmapDir = null;
            for (int idx = 0; idx < lightmapDatas.Count; ++idx)
            {
                if(lightmapDatas[idx].index == index)
                {
                    lightmapColor = lightmapDatas[idx].lightmapColor;
                    shadowMask = lightmapDatas[idx].shadowMask;
                    lightmapDir = lightmapDatas[idx].lightmapDir;                        
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            lightmapDatas.Clear();
        }
    }

}