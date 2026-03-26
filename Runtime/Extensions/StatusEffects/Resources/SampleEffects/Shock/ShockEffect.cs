using UnityEngine;

namespace Mandible.Entities.StatusEffects
{
    public class ShockEffect : StatusEffect
    {
        float timer;
        GameObject vfx;

        ShockEffectData Shock => (ShockEffectData)data;

        public ShockEffect(Entity owner, ShockEffectData data) 
            : base(owner, data) {}

        public override void OnApply()
        {
            //Spawn VFX
            if (vfx != null) return;
            vfx = Object.Instantiate(Shock.vfxPrefab);
        }

        public override void OnTick(float dt)
        {
            //Damage over time
            timer += dt;
            if (timer >= Shock.tickRate)
            {
                HitData data = new HitData()
                {
                    hitTarget = owner,
                    hitType = HitType.Normal,
                    hitAmount = Shock.damagePerTick
                };

                owner.TakeDamage(Shock.damagePerTick, data);
                timer = 0f;
            }

            //VFX follow
            if (vfx != null)
                vfx.transform.position = owner.transform.position;
        }

        public override void OnRemove()
        {
            //Remove VFX
            if (vfx != null) Object.Destroy(vfx);
        }
    }
}
