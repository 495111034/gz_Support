using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Entity;


public class SkillEditorData : MonoBehaviour
{    
    [SerializeField]
    public List<SkillDataBase> skillList;

    //从excel中读取技能数据
    public SkillDataBase GetSkillData(int skillID)
    {

        foreach(var item in skillList)
        {
            if (item.SkillID == skillID) return item;
        }
        return null;
    }

#if UNITY_EDITOR
    public void SaveData()
    {
        if (!Application.isPlaying)
        {
            if (skillList != null)
                foreach (var i in skillList)
                    if (i != null && i.SkillID > 0) i.SaveData();
        }
    }
#endif
}