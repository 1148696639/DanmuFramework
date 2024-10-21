using System.Collections;
using UnityEngine;

namespace Utility.AttackHelper
{
    public class MultiProjectileAttack : IAttackBehavior
    {
        private AttackConfig m_AttackConfig;
        private Transform m_BulletParentTran;
        private AttackCtrl m_context;
        private int m_MultipleBulletFinishCount;
        private Transform m_Target;

        public IEnumerator ExecuteAttack(AttackCtrl context)
        {
            m_BulletParentTran = context.GetBulletParentTran();
            m_context = context;
            m_AttackConfig = context.AttackConfig;

            for (var i = 0; i < m_AttackConfig.BulletMultipleCount; i++)
            {
                var randomOffset = RandomHelper.GetRandomVector(m_AttackConfig.OffsetRandomPos);
                var bulletTran = context.SpawnBullet(m_BulletParentTran.position + randomOffset,
                    m_BulletParentTran.rotation);

                context.StartCoroutine(MoveBullet(bulletTran, context));
                yield return new WaitForSeconds(m_AttackConfig.BulletPerDelay);
            }
        }

        private IEnumerator MoveBullet(Transform bulletTran, AttackCtrl context)
        {
            m_Target = context.GetTarget();

            while (m_Target && m_Target.gameObject.activeSelf)
            {
                bulletTran.position = Vector3.MoveTowards(bulletTran.position, m_Target.position,
                    m_AttackConfig.Speed * Time.deltaTime);
                if (Vector3.Distance(bulletTran.position, m_Target.position) < 0.1f)
                {
                    context.DeSpawnBullet(bulletTran);
                    OnBulletHit();
                    yield break;
                }

                yield return null;
            }

            context.DeSpawnBullet(bulletTran);
        }

        private void OnBulletHit()
        {
            //无论目标是否为空 都要等到所有子弹发射完毕后回收技能母体，要不然子弹的协程会无法继续执行
            m_MultipleBulletFinishCount++;
            if (m_MultipleBulletFinishCount >= m_AttackConfig.BulletMultipleCount) m_context.DespawnSkill();

            if (!m_Target || !m_Target.gameObject.activeSelf) return;


            var hitEffect = m_context.SpawnHitEffect();
            if (hitEffect) m_context.DeSpawnHitEffect(hitEffect);


            m_context.OnHitTrigger?.Invoke(new OnHitArgs
                { AttackType = m_context.AttackConfig.AttackType, HitCount = m_AttackConfig.BulletMultipleCount });
        }
    }
}