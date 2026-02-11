using UnityEngine;

namespace Mandible.Entities
{
    public class EntityExtension
    {
        public enum UpdateOrder
        {
            Default,
            PreProcess,
            PostProcess
        }
        public Entity entity;
        public UpdateOrder updateOrder = UpdateOrder.Default;

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            
        }

        //API
        public virtual void Handle(){ }

        //Initialization
        public void Initialize(Entity parentEntity)
        {
            entity = parentEntity;
        }
    }
}
