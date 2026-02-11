using UnityEngine;

namespace Mandible.Entities.StatusEffects
{
    [CreateAssetMenu(menuName = "StatusEffects/Shock")]
    public class ShockEffectData : StatusEffectData
    {
        public float tickRate;
        public float damagePerTick;

        public override StatusEffect CreateRuntimeEffect(Entity owner)
        {
            return new ShockEffect(owner, this);
        }
    }
}
