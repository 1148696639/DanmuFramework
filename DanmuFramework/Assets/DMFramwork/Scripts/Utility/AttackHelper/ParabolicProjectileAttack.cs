using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    /// <summary>
    ///     抛物线弹道
    /// </summary>
    public class ParabolicProjectileAttack : IAttackBehavior
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

                var distanceToTarget = Vector3.Distance(bulletTran.position, m_Target.position);

                while (m_Target && m_Target.gameObject.activeSelf)
                {
                    bulletTran.LookAt(m_Target.position);
                    var angle = Mathf.Min(1,
                        Vector3.Distance(bulletTran.position, m_Target.position) / distanceToTarget) * 45;
                    bulletTran.rotation *= Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);
                    var currentDist = Vector3.Distance(bulletTran.position, m_Target.position);

                    if (currentDist < 0.1f)
                    {
                        context.DeSpawnBullet(bulletTran);
                        OnBulletHit();
                        yield break;
                    }

                    bulletTran.Translate(Vector3.forward *
                                         Mathf.Min(context.AttackConfig.ParabolicSpeed * Time.deltaTime, currentDist));
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