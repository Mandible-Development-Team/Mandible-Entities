using UnityEngine;
using System.Linq;

using Mandible.Core.Data;

namespace Mandible.Entities
{
    public class EntityState : RuntimeObject
    {
        [HideInInspector] protected Entity owner;
        public virtual string description { get; } = "(No description)";
        public string tag = "Default";

        public void Initialize(Entity owner) 
        {
            this.owner = owner;
        }

        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnExit() { }
    }
}