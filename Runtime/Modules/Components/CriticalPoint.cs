using UnityEngine;
using System.Collections;

using Mandible.Entities.StatusEffects;

namespace Mandible.Entities
{
    public class CriticalPoint : EntityTransform
    {
        const float damageMultiplier = 1.5f;

        //API
        public override void TakeDamage(float amount, HitData data = default)
        {
            data.hitTarget = target;
            data.hitType = GetHitType();
            data.hitAmount = amount * damageMultiplier;

            target?.TakeDamage(amount * damageMultiplier, data);
        }

        //Extension Conduct
        public void AddStatusEffectContribution(StatusEffectContribution contribution)
        {
            target?.AddStatusEffectContribution(contribution);
        }

        //Getters / Setters
        public override HitType GetHitType()
        {
            return HitType.Critical;
        }
    }
}