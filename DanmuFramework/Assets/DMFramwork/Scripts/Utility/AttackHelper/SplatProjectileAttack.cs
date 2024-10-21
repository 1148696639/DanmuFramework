using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.AttackHelper
{
    /// <summary>
    ///     直线溅射攻击
    /// </summary>
    public class SplatProjectileAttack : IAttackBehavior
    {
        private AttackConfig m_attackConfig;
        private AttackCtrl m_context;
        private Transform m_Target;

        public IEnumerator ExecuteAttack(AttackCtrl context)
        {
            m_context = context;
            m_Target = context.GetTarget();
            m_attackConfig = context.AttackConfig;
            if (m_Target)
            {
                var bulletTran = context.SpawnBullet();

                yield return new WaitForSeconds(m_attackConfig.BulletDelay);

                while (m_Target&&m_Target.gameObject.activeSelf)
                {
                    if (Vector3.SqrMagnitude(bulletTran.position - m_Target.position) < 0.1f)
                    {
                        context.DeSpawnBullet(bulletTran);
                        OnBulletHit();
                        yield break;
                    }

                    bulletTran.position = Vector3.MoveTowards(bulletTran.position, m_Target.position,
                        m_attackConfig.Speed * Time.deltaTime);
                    yield return null;
                }

                context.DeSpawnBullet(bulletTran);
            }
        }


        private void OnBulletHit()
        {
            var hitTargets = new List<Transform>();
            // 溅射到周围的敌人
            var colliders = Physics.OverlapSphere(m_Target.position, m_attackConfig.SputterRange,
                1 << m_Target.gameObject.layer);
            foreach (var collider in colliders)
                if (collider.CompareTag(m_Target.tag))
                    hitTargets.Add(collider.transform);


            var hitEffect = m_context.SpawnHitEffect();
            if (hitEffect) m_context.DeSpawnHitEffect(hitEffect);

            m_context.DespawnSkill();
            m_context.OnHitTrigger?.Invoke(new OnHitArgs
                { AttackType = m_context.AttackConfig.AttackType, HitTargets = hitTargets });
        }
    }
}