using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    /// <summary>
    ///  会被挡住的直线弹道
    /// </summary>
    public class LinearProjectileCanBeBlockedAttack : IAttackBehavior
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
                yield return new WaitForSeconds(context.AttackConfig.BulletDelay); // 等待攻击前置时间

                var direction = (m_Target.position - bulletTran.position).normalized;
                var layerMask = 1 << m_Target.gameObject.layer; // 确定要检测的层级

                while (m_Target && m_Target.gameObject.activeSelf)
                {
                    // 进行射线检测，判断是否有碰撞发生
                    var stepDistance = context.AttackConfig.Speed * Time.deltaTime;

                    if (Physics.Raycast(bulletTran.position, direction, out var hit, stepDistance, layerMask))
                    {
                        m_BreakPoint = hit.point;
                        context.DeSpawnBullet(bulletTran);
                        // 如果子弹没有被挡住，并且到达目标位置，处理命中逻辑
                        OnBulletHit();
                        yield break; // 退出协程
                    }

                    // 如果没有被挡住，继续移动子弹
                    bulletTran.position += direction * stepDistance;

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