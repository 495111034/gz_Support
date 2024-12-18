using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Entity
{
    public enum PosType
    {
        E1 = 1,
        E2,
        E3,
        E4,
        C1,
        C2,
        C3,
        P1,
        P2,
        P3,
        Scene_center,
        Character_Center,
        Enmey_Center,
        Camera_Normal,
        Camera_P,
    }
    /// <summary>
    /// 战斗阵型中的一个位置
    /// </summary>
    [System.Serializable]
    public class FightPositionPoint: MonoBehaviour
    {
        public PosType posType;
    }
    
}