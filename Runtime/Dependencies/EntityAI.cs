using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Mandible.Entities.AI;

namespace Mandible.Entities
{
    public class EntityAI : EntityDependency
    {
        private EntityStateMachine stateMachine;
        public EntityTargetingSystem targetingSystem = new EntityTargetingSystem(default);

        [Header("AI")]
        public AIDecision currentDecision;
        public List<AIDecision> decisions = new List<AIDecision>();
        private AIDecision _prevDecision = default;

        //Helpers
        public Entity Target => GetTarget();
        public float Health => owner.GetHealth();
        public float HealthPercentage => owner.GetHealthPercentage();
        public bool IsDead => owner.IsDead;

        const string DEATH_STATE_TAG = "Dead";

        public override void Initialize(Entity owner)
        {
            base.Initialize(owner);

            CreateRuntimeInstances(decisions);
            InitializeDecisions();

            targetingSystem.Initialize(owner);
        }

        public void Start()
        {
            if(decisions.Count == 0) return;
            
            stateMachine = owner.GetComponent<EntityStateMachine>();
            QueryStateChange(decisions.FirstOrDefault());
        }

        public override void Handle()
        {
            //Information Gathering
            targetingSystem?.UpdateTargets();

            //Decisions
            if(owner.IsDead)
            {
                if(!enabled) return;

                bool foundDeadState = QueryStateChange(DEATH_STATE_TAG);
                if (!foundDeadState){
                    ClearState();
                    stateMachine?.OnDeathDefault();
                }

                enabled = false;
            }
            else{
                currentDecision = EvaluateDecisions();
            }

            //Handle state change
            if(_prevDecision != currentDecision)
            {
                QueryStateChange(currentDecision);
                _prevDecision = currentDecision;
            }
        }

        //Targeting

        public Entity GetTarget() => targetingSystem?.GetTarget();

        //Generic Decisions

        public AIDecision EvaluateDecisions()
        {
            AIDecision bestDecision = null;
            float highestScore = float.MinValue;

            foreach (var decision in decisions)
            {
                float score = EvaluateDecision(decision);
                score *= decision.weight;

                if (score > highestScore)
                {
                    highestScore = score;
                    bestDecision = decision;
                }
            }

            return bestDecision;
        }

        public float EvaluateDecision(AIDecision decision)
        { 
            return decision.Evaluate(this);
        }

        public bool QueryStateChange(string stateName)
        {
            return stateMachine?.ChangeState(stateName);
        }

        public bool QueryStateChange(AIDecision decision)
        {
            return QueryStateChange(decision.stateTag);
        }

        public void ClearState()
        {
            stateMachine?.ClearState();
        }

        //Helpers

        public void CreateRuntimeInstances(List<AIDecision> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && !list[i].IsInstance)
                {
                    AIDecision instance = list[i].CreateRuntimeInstance() as AIDecision;
                    list[i] = instance;
                }
            }
        }

        private void InitializeDecisions()
        {
            foreach(var decision in decisions)
            {
                decision.Initialize(this);
            }
        }
    }
}