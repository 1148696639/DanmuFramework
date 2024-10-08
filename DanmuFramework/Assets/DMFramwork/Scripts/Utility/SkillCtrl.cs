using System;
using System.Collections;
using PathologicalGames;
using UnityEngine;

public class SkillCtrl : MonoBehaviour
{
    public enum SkillTypeEnum
    {
        直线, // 直线弹道
        抛物线, // 抛物线弹道
        近战, // 近战攻击
        连射
    }


    [Header("技能类型")] public SkillTypeEnum SkillType; // 技能类型

    [ShowIfEnum("SkillType", (int)SkillTypeEnum.直线)]
    public float disCheck=0.1f;

    [Header("-----------基础设置------------")] [Header("前置特效预制体")]
    public GameObject PreAttackPrefab; // 前置特效预制体

    [Header("前置特效生成之后第几秒触发子弹生成")] public float PreAttackDelay; // 攻击前置时间（可选）

    [Header("前置特效生成之后的第几秒销毁")] public float PreGoDuration;

    [Header("弹道预制体")] public GameObject BulletPrefab; // 弹道预制体

    [Header("子弹发射延时")] public float BulletDelay; // 攻击前置时间（可选）

    [Header("弹道速度")] public float Speed; // 弹道速度

    [Header("命中特效预制体")] public GameObject HitEffectPrefab; // 命中特效预制体

    [Header("受击特效销毁延时")] public float HitEffectDelay;

    [Header("-----------抛物线------------")] [Header("抛物线运动速度")]
    public float ParabolicSpeed;

    [Header("-----------连射------------")] [Header("每个子弹之间延迟")]
    public float BulletPerDelay;

    [Header("连射子弹数量")] public int BulletMultipleCount;

    [Header("子弹生成偏移范围")] public Vector3 OffsetRandomPos;


    private Transform _bulletParentTran;

    private bool _isRuning;

    private bool _moveFlag = true;

    private int _multipleBulletFinishCount;

    private Action<SkillTypeEnum, int> _onHitTrigger;

    private Transform _preParentTran;

    private SpawnPool _skillPool;

    private SpawnPool _spawnPool;

    private Transform _target; // 目标

    private void Awake()
    {
        _preParentTran = transform.Find("PreParent");
        _bulletParentTran = transform.Find("BulletParent");
    }

    private void OnDisable()
    {
        _isRuning = false;
        _multipleBulletFinishCount = 0;
    }

    public void Initialize(Transform target, Action<SkillTypeEnum, int> onHitTrigger,SpawnPool skillPool,SpawnPool effectPool)
    {
        _isRuning = true;
        _target = target;
        _onHitTrigger = onHitTrigger;
        _spawnPool = effectPool;
        _skillPool = skillPool;

        // 如果有攻击前置时间，延迟后再进行攻击
        StartCoroutine(HandlePreAttackDelay());
    }

    private Vector3 breakPoint;
    /// <summary>
    ///     处理直线弹道，不会被挡住直奔目标
    /// </summary>
    private IEnumerator HandleLinearProjectile()
    {
        if (_target != null)
        {
            var bulletTran = _spawnPool.Spawn(BulletPrefab, _bulletParentTran.position, _bulletParentTran.rotation)
                .transform; // 播放前置特效（可选）(播放前置特效)
            yield return new WaitForSeconds(BulletDelay); // 等待攻击前置时间

            while (bulletTran && _target && !(Vector3.Distance(bulletTran.position, _target.position) < disCheck))
            {
                if (!_isRuning) _spawnPool.Despawn(bulletTran);
                bulletTran.position =
                    Vector3.MoveTowards(bulletTran.position, _target.position, Speed * Time.deltaTime);
                yield return null;
            }
            breakPoint = bulletTran.position;
            if (_target) bulletTran.position = _target.position;
            StopCoroutine(nameof(HandleLinearProjectile));
            _spawnPool.Despawn(bulletTran);
        }
    }


    /// <summary>
    ///     处理直线弹道,会被挡住
    /// </summary>
    private IEnumerator HandleLinearProjectileCanBeBlocked()
    {
        if (_target != null)
        {
            var bulletTran = _spawnPool.Spawn(BulletPrefab, _bulletParentTran.position, _bulletParentTran.rotation).transform;
            yield return new WaitForSeconds(BulletDelay); // 等待攻击前置时间

            Vector3 direction = (_target.position - bulletTran.position).normalized;

            while (bulletTran && _target)
            {
                if (!_isRuning)
                {
                    _spawnPool.Despawn(bulletTran);
                    yield break;
                }

                // 射线检测逻辑
                RaycastHit hit;
                float stepDistance = Speed * Time.deltaTime;

                if (Physics.Raycast(bulletTran.position, direction, out hit, stepDistance))
                {
                    // 如果射线命中目标
                    breakPoint = hit.point; // 记录命中点
                    _spawnPool.Despawn(bulletTran);
                    yield break; // 退出协程
                }

                bulletTran.position += direction * stepDistance;

                yield return null;
            }

            breakPoint = bulletTran.position;
            StopCoroutine(nameof(HandleLinearProjectile));
            _spawnPool.Despawn(bulletTran);
        }
    }

    /// <summary>
    ///     跟踪目标
    /// </summary>
    private IEnumerator HandleTrackingMissiles()
    {
        if (_target != null)
        {
            var bulletTran = _spawnPool.Spawn(BulletPrefab, _bulletParentTran.position, _bulletParentTran.rotation).transform;
            yield return new WaitForSeconds(BulletDelay); // 等待攻击前置时间


            while (bulletTran && _target)
            {
                if (!_isRuning)
                {
                    _spawnPool.Despawn(bulletTran);
                    yield break;
                }

                // 射线检测逻辑
                RaycastHit hit;
                float stepDistance = Speed * Time.deltaTime;

                if (Physics.Raycast(bulletTran.position, bulletTran.forward, out hit, stepDistance))
                {
                    // 如果射线命中目标
                    breakPoint = hit.point; // 记录命中点
                    _spawnPool.Despawn(bulletTran);
                    yield break; // 退出协程
                }

                bulletTran.position =
                    Vector3.MoveTowards(bulletTran.position, _target.position, Speed * Time.deltaTime);
                bulletTran.LookAt(_target);
                yield return null;
            }

            breakPoint = bulletTran.position;
            StopCoroutine(nameof(HandleLinearProjectile));
            _spawnPool.Despawn(bulletTran);
        }
    }



    /// <summary>
    ///     处理抛物线弹道
    /// </summary>
    private IEnumerator HandleParabolicProjectile()
    {
        if (_target != null)
        {
            var bulletTran = _spawnPool.Spawn(BulletPrefab, _bulletParentTran.position, Quaternion.identity)
                .transform; // 播放前置特效（可选）(播放前置特效)
            yield return new WaitForSeconds(BulletDelay); // 等待攻击前置时间

            var distanceToTarget = Vector3.Distance(bulletTran.position, _target.position);
            while (_target && _moveFlag)
            {
                if (!_isRuning) _spawnPool.Despawn(bulletTran);
                var targetPos = _target.position;
                // 朝向目标, 以计算运动
                bulletTran.LookAt(targetPos);
                // 根据距离衰减 角度
                var angle = Mathf.Min(1, Vector3.Distance(bulletTran.position, targetPos) / distanceToTarget) * 45;
                // 旋转对应的角度（线性插值一定角度，然后每帧绕X轴旋转）
                bulletTran.rotation *= Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);
                // 当前距离目标点
                var currentDist = Vector3.Distance(bulletTran.position, _target.position);
                // 很接近目标了, 准备结束循环
                if (currentDist < 0.1f) _moveFlag = false;
                // 平移 (朝向Z轴移动)
                bulletTran.Translate(Vector3.forward * Mathf.Min(ParabolicSpeed * Time.deltaTime, currentDist));
                // 暂停执行, 等待下一帧再执行while
                yield return null;
            }

            if (_target)
            {
                if (_moveFlag == false)
                {
                    // 使自己的位置, 跟[目标点]重合
                    bulletTran.position = _target.position;
                    // [停止]当前协程任务,参数是协程方法名
                    StopCoroutine(nameof(HandleParabolicProjectile));
                    _spawnPool.Despawn(bulletTran);
                }
            }
            else
            {
                StopCoroutine(nameof(HandleParabolicProjectile));
                _spawnPool.Despawn(bulletTran);
            }
        }
    }


    /// <summary>
    ///     处理近战攻击
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleMeleeAttack()
    {
        yield return null;
    }

    /// <summary>
    ///     处理攻击前置时间
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandlePreAttackDelay()
    {
        // 播放前置特效（可选）(播放前置特效)
        if (PreAttackPrefab)
        {
            var preGo = _spawnPool.Spawn(PreAttackPrefab, _preParentTran.position, _preParentTran.rotation).transform;
            _spawnPool.Despawn(preGo, PreGoDuration);
        }

        yield return new WaitForSeconds(PreAttackDelay); // 等待攻击前置时间

        yield return HandleAttack();
    }

    /// <summary>
    ///     处理攻击
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleAttack()
    {
        switch (SkillType)
        {
            // 如果是近战攻击，立即处理
            case SkillTypeEnum.近战:
                yield return HandleMeleeAttack();
                HitTarget();

                break;
            case SkillTypeEnum.直线:
                yield return HandleLinearProjectile();
                HitTarget();

                break;
            case SkillTypeEnum.抛物线:
                yield return HandleParabolicProjectile();
                HitTarget();

                break;
            case SkillTypeEnum.连射:
                yield return HandleMultiProjectile();
                break;
        }
    }

    /// <summary>
    ///     处理连发技能
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleMultiProjectile()
    {
        yield return new WaitForSeconds(BulletDelay); // 等待攻击前置时间

        for (var i = 0; i < BulletMultipleCount; i++)
        {
            // 计算生成对象的位置，基于当前对象的位置加上一个随机偏移，并乘以缩放因子
            var pos = _bulletParentTran.position + RandomHelper.GetRandomVector(OffsetRandomPos);
            var bulletTran = _spawnPool.Spawn(BulletPrefab, pos, Quaternion.identity).transform;

            yield return new WaitForSeconds(BulletPerDelay); // 等待攻击前置时间

            if (_target)
            {
                StartCoroutine(MultiProjectileMove(bulletTran));
            }
            else
            {
                _spawnPool.Despawn(bulletTran);
                yield break;
            }
        }
    }

    /// <summary>
    ///     处理连发技能移动
    /// </summary>
    /// <param name="bulletTran"></param>
    /// <returns></returns>
    private IEnumerator MultiProjectileMove(Transform bulletTran)
    {
        while (_target && !(Vector3.Distance(bulletTran.position, _target.position) < 0.1f))
        {
            if (!_isRuning)
            {
                _spawnPool.Despawn(bulletTran);
                yield break;
            }

            bulletTran.LookAt(_target);
            bulletTran.position =
                Vector3.MoveTowards(bulletTran.position, _target.position, Speed * Time.deltaTime);
            yield return null;
        }

        if (_target) bulletTran.position = _target.position;

        _spawnPool.Despawn(bulletTran);
        MultipleHitTarget();
    }

    /// <summary>
    ///     击中目标
    /// </summary>
    private void HitTarget()
    {
        if (_target == null)
        {
            _skillPool.Despawn(gameObject.transform);
            return;
        }

        if (HitEffectPrefab != null)
        {
            var hitEffect = _spawnPool.Spawn(HitEffectPrefab, breakPoint, Quaternion.identity);
            _spawnPool.Despawn(hitEffect, HitEffectDelay);
        }

        _skillPool.Despawn(gameObject.transform);
        _onHitTrigger?.Invoke(SkillType, 1);
    }

    /// <summary>
    ///     连击击中目标
    /// </summary>
    private void MultipleHitTarget()
    {
        //无论目标是否为空 都要等到所有子弹发射完毕后回收技能母体，要不然子弹的协程会无法继续执行
        _multipleBulletFinishCount++;
        if (_multipleBulletFinishCount >= BulletMultipleCount) _skillPool.Despawn(gameObject.transform);

        if (_target == null)
        {
            return;
        }

        if (HitEffectPrefab != null)
        {
            var hitEffect = _spawnPool.Spawn(HitEffectPrefab, _target.position, Quaternion.identity);
            _spawnPool.Despawn(hitEffect, HitEffectDelay);
        }

        _onHitTrigger?.Invoke(SkillType, BulletMultipleCount);

    }
}