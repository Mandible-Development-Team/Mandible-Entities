using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities.AI
{
    [CreateAssetMenu(fileName = "MoveToTargetDecision", menuName = "Mandible/Entities/Entity AI/Generic/MoveToTargetDecision", order = 1)]
    public class MoveToTargetDecision : AIDecision
    {
        public override string description => 
            "Generic decision used for knowing when to move to the target. Queries the target distance and if within range, enacts a movement state.";
        
        public override float Evaluate(EntityAI ai = default)
        {
            return ai.Target != null ? 1f : 0f; 
        }
    }
}
