using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mandible.Core.Data;

namespace Mandible.Entities.AI
{
    public abstract class AIDecision : RuntimeObject
    {
        [HideInInspector] public EntityAI owner;
        public virtual string description { get; } = "(No description)";
        public string stateTag = "Default";
        public float weight = 1.0f;

        public void Initialize(EntityAI owner)
        {
            this.owner = owner;
        }

        public virtual float Evaluate(EntityAI ai = default)
        {
            if(isInstance) owner = ai;
            
            return 0f;
        }
    }
}

/*
[CreateAssetMenu(fileName = "TargetHoverDecision", menuName = "AI/Decisions/TargetHoverDecision", order = 2)]
public class TargetHoverDecision : AIDecision
{
    
}
*/