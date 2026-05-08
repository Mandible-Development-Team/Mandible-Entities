using UnityEngine;

using Mandible.Entities.StatusEffects;

namespace Mandible.Entities
{
    public interface IDamageable
    {
        bool IsDead { get;}
        HitType GetHitType();
        void TakeDamage(float amount, HitData data = default);

        // Status Effects
        void AddStatusEffectContribution(StatusEffectContribution contribution);
    }
}
