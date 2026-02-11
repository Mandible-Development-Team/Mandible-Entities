using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Mandible.Entities.AI;

namespace Mandible.Entities
{
    [CreateAssetMenu(fileName = "New Entity Definition", menuName = "Mandible/Entities/Entity Definition", order = 1)]
    public class EntityDefinition : ScriptableObject
    {
        [Header("AI")]
        public List<AIDecision> aiTemplate = new List<AIDecision>();

        [Header("States")]
        public List<EntityState> stateTemplate = new List<EntityState>();

        public void ApplyToEntity(Entity entity)
        {
            if (entity == null)
            {
                Debug.LogError("EntityDefinition: ApplyToEntity - Entity is null!");
                return;
            }

            //AI
            EntityAI ai = entity.GetComponent<EntityAI>();
            if(ai != null)
            {
                ai.decisions = new List<AIDecision>();
                foreach(AIDecision decision in aiTemplate)
                {
                    ai.decisions.Add(decision);
                }
            }

            //States
            EntityStateMachine stateMachine = entity.GetComponent<EntityStateMachine>();
            if(stateMachine != null)
            {
                stateMachine.states = new List<EntityState>();
                foreach(EntityState state in stateTemplate)
                {
                    stateMachine.states.Add(state);
                }
            }
        }
    }
}
