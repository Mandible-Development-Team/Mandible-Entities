using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Mandible.Entities.StatusEffects
{
    [System.Serializable]
    public abstract class StatusEffect
    {
        public Entity owner;
        public StatusEffectData data;

        protected StatusEffect(Entity owner, StatusEffectData data)
        {
            this.owner = owner;
            this.data = data;
        }

        //API
        public virtual void OnApply() {}
        public virtual void OnTick(float deltaTime) {}
        public virtual void OnRemove() {}

        //Getters / Setters
        public void SetOwner(Entity owner)
        {
            this.owner = owner;
        } 

        //Operator Override
        public override bool Equals(object obj)
        {
            return obj is StatusEffect other && this.GetType() == other.GetType();
        }

        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }
    }
}
