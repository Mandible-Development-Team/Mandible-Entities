using UnityEngine;

namespace Mandible.Entities.StatusEffects
{
    public class BurnEffect : StatusEffect
    {
        float timer;
        GameObject vfx;

        BurnEffectData Burn => (BurnEffectData)data;

        public BurnEffect(Entity owner, BurnEffectData data) 
            : base(owner, data) {}

        public override void OnApply()
        {
            //Spawn VFX
            if(vfx != null) return;
            vfx = Object.Instantiate(Burn.vfxPrefab, owner.transform.position, Quaternion.identity);
        }

        public override void OnTick(float dt)
        {
            //Damage over time
            timer += dt;
            if (timer >= Burn.tickRate)
            {
                owner.TakeDamage(Burn.damagePerTick);
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
