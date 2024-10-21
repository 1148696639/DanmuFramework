using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    /// <summary>
    ///   追踪弹道
    /// </summary>
    public class TrackingMissileAttack : IAttackBehavior
    {
        private Vector3 m_BreakPoint;
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
                var layerMask = 1 << m_Target.gameObject.layer; // 确定要检测的层级

                while (m_Target&&m_Target.gameObject.activeSelf)
                {
                    var direction = (m_Target.position - bulletTran.position).normalized;

                    var stepDistance = context.AttackConfig.Speed * Time.deltaTime;

                    if (Physics.Raycast(bulletTran.position, direction, out var hit, stepDistance, layerMask))
                    {
                        m_BreakPoint = hit.point;
                        context.DeSpawnBullet(bulletTran);
                        OnBulletHit();
                        yield break; // 退出协程
                    }

                    bulletTran.position = Vector3.MoveTowards(bulletTran.position, m_Target.position,
                        context.AttackConfig.Speed * Time.deltaTime);
                    bulletTran.LookAt(m_Target);
                    yield return null;
                }

                context.DeSpawnBullet(bulletTran);

            }
        }

        private void OnBulletHit()
        {
            var hitEffect = m_context.SpawnHitEffect(m_BreakPoint);
            if (hitEffect) m_context.DeSpawnHitEffect(hitEffect);


            m_context.DespawnSkill();
            m_context.OnHitTrigger?.Invoke(new OnHitArgs { AttackType = m_context.AttackConfig.AttackType });
        }
    }
}