using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    public interface IAttackBehavior
    {
        IEnumerator ExecuteAttack(AttackCtrl context);

    }
}