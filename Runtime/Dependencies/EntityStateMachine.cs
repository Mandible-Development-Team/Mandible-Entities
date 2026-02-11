using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mandible.Entities.Actions;
using Mandible.Core.Data;

namespace Mandible.Entities
{
    public class EntityStateMachine : EntityDependency
    {
        [SerializeField]
        public Animator animator;

        [Header("States")]
        [SerializeField]
        public EntityState currentState;
        public List<EntityState> states = new List<EntityState>();
        public Dictionary<string, EntityState> statesDict = new Dictionary<string, EntityState>();

        [HideInInspector]
        public UnityEvent onStateChanged = new UnityEvent();

        public override void Initialize(Entity owner)
        {
            base.Initialize(owner);
            
            CreateRuntimeInstances(states);

            InitializeStates();
            InitializeDictionary();

            SetEventListeners();
        }

        public void Start()
        {
            currentState = states.FirstOrDefault();
            if(currentState == null) return;
            
            ChangeState(currentState);
        }

        public override void Handle()
        {
            currentState?.OnUpdate();
        }

        //Defaults
        const string DAMAGE_TRIGGER_TAG = "Damage";
        public void OnDamageDefault()
        {
            if(animator != null)
            {
                animator.SetTrigger(DAMAGE_TRIGGER_TAG);
            }
        }

        public void OnDeathDefault()
        {
            //Kill Animator
            if(animator != null)
            {
                animator.enabled = false;
            }

            //Disable Rigidbody
            if(owner.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        //State API

        public EntityState ChangeState(string tag)
        {
            EntityState newState = GetState(tag);
            onStateChanged?.Invoke();

            if(newState == null)
            {
                Debug.LogWarning("EnemyStateMachine: State " + tag + " not found!");
                return null;
            }

            //Handle enter/exit
            currentState?.OnExit();
            currentState = newState;
            currentState?.OnEnter();

            return currentState;
        }

        public EntityState ChangeState(EntityState newState)
        {
            return ChangeState(newState.tag);
        }

        public void ClearState()
        {
            currentState?.OnExit();
            currentState = null;
        }

        public EntityState GetState(string tag)
        {
            statesDict.TryGetValue(tag, out EntityState newState);

            if(newState == null)
            {
                Debug.LogWarning("EnemyStateMachine: State " + tag + " not found!");
                return null;
            }

            return newState;
        }

        public void CreateRuntimeInstances(List<EntityState> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && !list[i].IsInstance)
                {
                    EntityState instance = list[i].CreateRuntimeInstance() as EntityState;
                    list[i] = instance;
                }
            }
        }

        private void InitializeStates()
        {
            foreach(var state in states)
            {
                state.Initialize(owner);
            }
        }

        private void InitializeDictionary()
        {
            foreach(var state in states)
            {
                statesDict[state.tag] = state;
            }
        }

        private void SetEventListeners()
        {
            owner.takeDamage.AddListener(OnDamageDefault);
        }
    }
}