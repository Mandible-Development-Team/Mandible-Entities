using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mandible.Entities.StatusEffects
{
    public class BurnEffect : StatusEffect
    {
        float timer;
        GameObject vfx;
        Collider col;
        Transform root;

        BurnEffectData Burn => (BurnEffectData)data;

        public BurnEffect(Entity owner, BurnEffectData data) 
            : base(owner, data) {}

        public override void OnApply()
        {
            //Spawn VFX
            if(vfx != null) return;
            vfx = Object.Instantiate(Burn.vfxPrefab, owner.transform.position, Quaternion.identity);
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
            if (timer >= Burn.tickRate)
            {
                HitData data = new HitData()
                {
                    hitTarget = owner,
                    hitType = HitType.Normal,
                    hitAmount = Burn.damagePerTick,
                    hitPosition = target,
                };

                owner.TakeDamage(Burn.damagePerTick, data);
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
