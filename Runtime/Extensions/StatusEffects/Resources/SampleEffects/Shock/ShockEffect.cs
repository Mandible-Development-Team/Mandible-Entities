using UnityEngine;

namespace Mandible.Entities.StatusEffects
{
    public class ShockEffect : StatusEffect
    {
        float timer;
        GameObject vfx;
        Collider col;
        Transform root;

        ShockEffectData Shock => (ShockEffectData)data;

        public ShockEffect(Entity owner, ShockEffectData data) 
            : base(owner, data) {}

        public override void OnApply()
        {
            //Spawn VFX
            if (vfx != null) return;
            vfx = Object.Instantiate(Shock.vfxPrefab);
            col = owner.transform.GetComponentInChildren<Collider>();
            root = owner.root;
        }

        public override void OnTick(float dt)
        {
            Vector3 target = owner.transform.position;
            if (root != null) target = root.position;
            else if (col != null) target = col.bounds.center;

            //Damage over time
            timer += dt;
            if (timer >= Shock.tickRate)
            {
                HitData data = new HitData()
                {
                    hitTarget = owner,
                    hitType = HitType.Normal,
                    hitAmount = Shock.damagePerTick,
                    hitPosition = target,
                };

                owner.TakeDamage(Shock.damagePerTick, data);
                timer = 0f;
            }

            //VFX follow
            if (vfx != null)
                vfx.transform.position = target;
        }

        public override void OnRemove()
        {
            //Remove VFX
            if (vfx != null) Object.Destroy(vfx);
        }
    }
}
