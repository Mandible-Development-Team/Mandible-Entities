using UnityEngine;

using Mandible.Entities.StatusEffects;

namespace Mandible.Entities
{
    public interface IDamageable
    {
        bool IsDead { get;}
        HitType GetHitType();
        void TakeDamage(float amount);

        // Status Effects
        #if STATUS_EFFECTS
        void AddStatusEffectContribution(StatusEffectContribution contribution);
        #endif
    }
}
