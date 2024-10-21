using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utility.AttackHelper
{
    [Serializable]
    public class AttackConfig
    {
        public enum AttackTypeEnum
        {
            直线, // 直线弹道
            直线_可被挡, // 直线弹道
            追踪,
            抛物线, // 抛物线弹道
            近战, // 近战攻击
            连射,
            直线溅射
        }
        public AttackTypeEnum AttackType;
        public GameObject PreAttackPrefab;
        public float PreAttackDelay;
        public float PreGoDuration;
        public GameObject BulletPrefab;
        public float BulletDelay;
        public float Speed;
        public GameObject HitEffectPrefab;
        public float HitEffectDuraion;
        [ShowIfEnum("AttackType", (int)AttackTypeEnum.抛物线)]
        public float ParabolicSpeed;
        [ShowIfEnum("AttackType", (int)AttackTypeEnum.连射)]
        public float BulletPerDelay;
        [ShowIfEnum("AttackType", (int)AttackTypeEnum.连射)]
        public int BulletMultipleCount;
        [ShowIfEnum("AttackType", (int)AttackTypeEnum.连射)]
        public Vector3 OffsetRandomPos;
        [ShowIfEnum("AttackType", (int)AttackTypeEnum.直线溅射)]
        public float SputterRange;
    }
}