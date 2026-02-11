using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Mandible.Entities.StatusEffects;
using Mandible.Core.Data;

using UnityEditor;

namespace Mandible.Entities
{
    public class Entity : MonoBehaviour, IDamageable
    {
        public EntityMovement movement;
        public EntityAI ai;
        public EntityStateMachine stateMachine;
        protected List<EntityDependency> dependencies = new List<EntityDependency>();

        [Header("General")]
        [SerializeField] protected float health = 100f;
        [SerializeField] protected float currentHealth = 0;
        [SerializeField] bool isDead = false;
        [Header("Extensions")]
        [SerializeField] protected List<EntityExtension> extensions = new List<EntityExtension>();
        private List<EntityExtension> preProcess = new List<EntityExtension>();
        private List<EntityExtension> postProcess = new List<EntityExtension>();
        [Header("Debug")]
        [SerializeField] public bool debug = false;

        //Events
        [HideInInspector] public UnityEvent takeDamage = new UnityEvent();

        //Editor
        [HideInInspector] public bool usedSetupTool = false;

        public virtual void Awake()
        {
            //Dependencies
            GetDependencies();
            InitializeDependencies();

            //Extensions
            GetExtensions();
            InitializeExtensions();

            //Initialize
            currentHealth = health;
        }

        public virtual void Start()
        {
            
        }
        public virtual void Update()
        {
            //PreProcess
            HandlePreProcessExtensions();

            //Process
            HandleDependencies();
            HandleExtensions();

            //PostProcess
            HandlePostProcessExtensions();
        }

        private void Reset()
        {
            EditorApplication.delayCall += () => 
            {
                if(!usedSetupTool) 
                    Debug.Log("Entity: Component added manually. It is recommended you use the Entity Setup Tool to create entities.");
            };
        }

        //API

        public virtual void TakeDamage(float amount)
        {
            currentHealth -= amount;
            takeDamage?.Invoke();
            if (ShouldDie()) Kill();
        }

        public void Kill()
        {
            currentHealth = 0f;
            isDead = true;
        }

        public void Revive()
        {
            currentHealth = health;
            isDead = false;
        }

        public bool ShouldDie()
        {
            return currentHealth <= 0f && !isDead;
        }

        public virtual HitType GetHitType()
        {
            return HitType.Normal;
        }

        //Dependencies

        public void GetDependencies()
        {
            if(!TryGetComponent<EntityMovement>(out this.movement))
            {
                movement = gameObject.AddComponent<EntityMovement>();
            }

            if(!TryGetComponent<EntityAI>(out this.ai))
            {
                ai = gameObject.AddComponent<EntityAI>();
            }

            if(!TryGetComponent<EntityStateMachine>(out this.stateMachine))
            {
                stateMachine = gameObject.AddComponent<EntityStateMachine>();
            }
        }

        public virtual void SetDependencyOrder()
        {
            dependencies = new List<EntityDependency>()
            {
                movement,
                ai,
                stateMachine
            };
        }

        public void InitializeDependencies()
        {
            SetDependencyOrder();

            foreach (EntityDependency dependency in dependencies)
            {
                dependency.Initialize(this);
            }
        }

        public void HandleDependencies()
        {
            foreach (EntityDependency dependency in dependencies)
            {
                dependency?.Handle();
            }
        }

        //Extensions

        public void GetExtensions()
        {
            extensions = EntityExtensionRegistry.CreateExtensions();
        }

        public T GetExtension<T>() where T : EntityExtension
        {
            return extensions.OfType<T>().FirstOrDefault();
        }

        private List<EntityExtension> stageForRemoval = new List<EntityExtension>();
        public void InitializeExtensions()
        {
            foreach (EntityExtension extension in extensions)
            {
                extension.Initialize(this);

                if(extension.updateOrder == EntityExtension.UpdateOrder.PreProcess)
                {
                    preProcess.Add(extension);
                }
                else if(extension.updateOrder == EntityExtension.UpdateOrder.PostProcess)
                {
                    postProcess.Add(extension);
                }
                else continue;

                stageForRemoval.Add(extension);
            }
            
            foreach (EntityExtension extension in stageForRemoval)
            {
                extensions.Remove(extension);
            }

        }

        public void HandleExtensions()
        {
            foreach (EntityExtension extension in extensions)
            {
                extension.Handle();
            }
        }

        public void HandlePreProcessExtensions()
        {
            foreach (EntityExtension extension in preProcess)
            {
                extension.Handle();
            }
        }

        public void HandlePostProcessExtensions()
        {
            foreach (EntityExtension extension in postProcess)
            {
                extension.Handle();
            }
        }

        //Extension Contract

        #if STATUS_EFFECTS
        public void AddStatusEffectContribution(StatusEffectContribution contribution)
        {
            StatusEffectHandler handler = GetExtension<StatusEffectHandler>();
            if (handler != null)
            {
                handler.AddEffectContribution(contribution.name, contribution.value);
            }
        }
        #endif

        //Getters / Setters

        public float GetHealth()
        {
            return currentHealth;
        }

        public float GetHealthPercentage()
        {
            return currentHealth / health;
        }

        public virtual bool IsDead
        {
            get { return currentHealth <= 0f; }
        }
    }
}
