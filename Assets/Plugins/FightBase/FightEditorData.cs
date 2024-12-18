
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Entity;


public class FightEditorData : MonoBehaviour
{
    //场景数据
    [SerializeField]
    public SceneDataBase sceneData;

#if UNITY_EDITOR
    public void SaveData()
    {
        if(sceneData != null)
        {
            sceneData.SaveData();
        }

    }

#endif
}
