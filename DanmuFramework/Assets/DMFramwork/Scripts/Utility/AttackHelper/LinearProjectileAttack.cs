using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    public class LinearProjectileAttack : IAttackBehavior
    {
        private AttackCtrl m_context;
        private Transform m_Target;

        public IEnumerator ExecuteAttack(AttackCtrl context)
        {
            m_context = context;
            m_Target = context.GetTarget();
            if (m_Target != null)
            {
                var bulletTran = context.SpawnBullet();
                yield return new WaitForSeconds(context.AttackConfig.BulletDelay);

                while (m_Target && m_Target.gameObject.activeSelf)
                {
                    if (Vector3.Distance(bulletTran.position, m_Target.position) < 0.1f)
                    {
                        context.DeSpawnBullet(bulletTran);
                        OnBulletHit();
                        yield break;
                    }

                    bulletTran.position = Vector3.MoveTowards(bulletTran.position, m_Target.position,
                        context.AttackConfig.Speed * Time.deltaTime);
                    yield return null;
                }

                context.DeSpawnBullet(bulletTran);
            }
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