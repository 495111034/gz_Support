using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Entity
{
    /// <summary>
    /// 战斗中的一个阵型
    /// </summary>
    [System.Serializable]
    public class FightPositionData: MonoBehaviour
    {
        
        public FightPositionType positionType;
    }
}