using System.Collections.Generic;
using UnityEngine;

namespace Utility.AttackHelper
{
    public struct OnHitArgs
    {
        public AttackConfig.AttackTypeEnum AttackType;
        public int HitCount;
        public List<Transform> HitTargets;
    }
}