using System;

namespace Utility.AttackHelper
{
    public static class AttackBehaviorFactory
    {
        public static IAttackBehavior Create(AttackConfig.AttackTypeEnum attackType)
        {
            return attackType switch
            {
                AttackConfig.AttackTypeEnum.直线 => new LinearProjectileAttack() // 不会被挡的直线弹道
                ,
                AttackConfig.AttackTypeEnum.直线_可被挡 => new LinearProjectileCanBeBlockedAttack() // 会被挡住的直线弹道
                ,
                AttackConfig.AttackTypeEnum.追踪 => new TrackingMissileAttack() // 追踪弹道
                ,
                AttackConfig.AttackTypeEnum.抛物线 => new ParabolicProjectileAttack() // 抛物线弹道
                ,
                AttackConfig.AttackTypeEnum.连射 => new MultiProjectileAttack() // 连射攻击
                ,
                AttackConfig.AttackTypeEnum.近战 => new MeleeAttack() // 近战攻击
                ,
                AttackConfig.AttackTypeEnum.直线溅射 => new SplatProjectileAttack(),
                _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
            };
        }
    }
}