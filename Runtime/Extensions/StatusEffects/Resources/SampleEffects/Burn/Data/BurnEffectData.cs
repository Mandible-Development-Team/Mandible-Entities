using UnityEngine;

namespace Mandible.Entities.StatusEffects
{
    [CreateAssetMenu(menuName = "StatusEffects/Burn")]
    public class BurnEffectData : StatusEffectData
    {
        public float tickRate;
        public float damagePerTick;

        public override StatusEffect CreateRuntimeEffect(Entity owner)
        {
            return new BurnEffect(owner, this);
        }
    }
}
