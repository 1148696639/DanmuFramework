using System;
using System.Collections;
using PathologicalGames;
using UnityEngine;

namespace Utility.AttackHelper
{
    public class AttackCtrl : MonoBehaviour
    {
        public AttackConfig AttackConfig; // 使用配置类代替单个变量
        private SpawnPool m_EffectPool;
        public Action<OnHitArgs> OnHitTrigger;
        private Transform m_PreParentTran;
        private Transform m_BulletParentTran;
        private SpawnPool _skillPool;
        private Transform _target;

        public SpawnPool GetEffectPool()
        {
            return m_EffectPool;
        }
        public Transform GetPreParentTran()
        {
            return m_PreParentTran;
        }

        public Transform GetBulletParentTran()
        {
            return m_BulletParentTran;
        }
        private void Awake()
        {
            m_PreParentTran = transform.Find("PreParent");
            m_BulletParentTran = transform.Find("BulletParent");
        }

        public void Initialize(Transform target, Action<OnHitArgs> onHitTrigger,
            SpawnPool skillPool, SpawnPool effectPool)
        {
            _target = target;
            OnHitTrigger = onHitTrigger;
            _skillPool = skillPool;
            m_EffectPool = effectPool;

            StartCoroutine(HandlePreAttackDelay());
        }

        private IEnumerator HandlePreAttackDelay()
        {
            if (AttackConfig.PreAttackPrefab != null)
            {
                var preGo = m_EffectPool.Spawn(AttackConfig.PreAttackPrefab, m_PreParentTran.position, m_PreParentTran.rotation);
                m_EffectPool.Despawn(preGo, AttackConfig.PreGoDuration);
            }

            yield return new WaitForSeconds(AttackConfig.PreAttackDelay);
            yield return HandleAttack();
        }

        private IEnumerator HandleAttack()
        {
            var attackBehavior = AttackBehaviorFactory.Create(AttackConfig.AttackType);
            if (attackBehavior != null) yield return attackBehavior.ExecuteAttack(this);
        }

        // 提供必要的辅助方法供攻击行为类调用
        public Transform GetTarget()
        {
            return _target&&_target.gameObject.activeSelf?_target:null;
        }

        public Transform SpawnBullet()
        {
            return m_EffectPool.Spawn(AttackConfig.BulletPrefab, m_BulletParentTran.position, transform.rotation).transform;
        }

        public void DeSpawnBullet(Transform bullet)
        {
            m_EffectPool.Despawn(bullet);
        }

        public Transform SpawnBullet(Vector3 transformPosition, Quaternion quaternion)
        {
            return m_EffectPool.Spawn(AttackConfig.BulletPrefab, transformPosition, quaternion).transform;
        }

        public Transform SpawnHitEffect(Vector3 transformPosition)
        {
            return AttackConfig.HitEffectPrefab ? m_EffectPool.Spawn(AttackConfig.HitEffectPrefab, transformPosition, Quaternion.identity).transform : null;
        }
        public Transform SpawnHitEffect()
        {
            return AttackConfig.HitEffectPrefab?m_EffectPool.Spawn(AttackConfig.HitEffectPrefab, _target.position, Quaternion.identity).transform:null;
        }

        public void DeSpawnHitEffect(Transform hitEffect)
        {
            m_EffectPool.Despawn(hitEffect, AttackConfig.HitEffectDuraion);
        }

        public void DespawnSkill()
        {
            _skillPool.Despawn(gameObject.transform);
        }

    }
}