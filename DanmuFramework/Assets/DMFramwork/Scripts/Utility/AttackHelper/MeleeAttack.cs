using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    public class MeleeAttack : IAttackBehavior
    {
        private AttackCtrl m_context;
        private Transform m_Target;

        public IEnumerator ExecuteAttack(AttackCtrl context)
        {
            m_context = context;
            m_Target = context.GetTarget();
            // 近战攻击直接击中目标，无弹道
            OnBulletHit();
            yield return null;
        }

        private void OnBulletHit()
        {
            var hitEffect = m_context.SpawnHitEffect();
            if (hitEffect) m_context.DeSpawnHitEffect(hitEffect);

            m_context.DespawnSkill();
            m_context.OnHitTrigger?.Invoke(new OnHitArgs { AttackType = m_context.AttackConfig.AttackType });
        }
    }
}