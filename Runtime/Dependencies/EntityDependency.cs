using UnityEngine;

namespace Mandible.Entities
{
    public abstract class EntityDependency : MonoBehaviour
    {
        protected Entity owner;
        protected new bool enabled = true;

        public abstract void Handle();

        public virtual void Initialize(Entity owner)
        {
            this.owner = owner;
        }
        

        //Getters / Setters
        public void SetOwner(Entity entity)
        {
            owner = entity;
        }

        public Entity GetOwner()
        {
            return owner;
        }
    }
}