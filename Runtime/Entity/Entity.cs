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
        [HideInInspector] public EntityMovement movement;
        [HideInInspector] public EntityAI ai;
        [HideInInspector] public EntityStateMachine stateMachine;
        protected List<EntityDependency> dependencies = new List<EntityDependency>();

        [Header("General")]
        [SerializeField] protected float health = 100f;
        [SerializeField] protected float currentHealth = 0;
        [SerializeField] bool isDead = false;

        [Header("Data")]
        [HideInInspector] public List<HitData> hitData = new List<HitData>();
        
        [Header("Extensions")]
        [SerializeField] protected List<EntityExtension> extensions = new List<EntityExtension>();
        private List<EntityExtension> preProcess = new List<EntityExtension>();
        private List<EntityExtension> postProcess = new List<EntityExtension>();

        [Header("Debug")]
        [SerializeField] public bool debug = false;

        //Events
        [HideInInspector] public UnityEvent onDamage = new UnityEvent();

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

        public virtual void LateUpdate()
        {
            //ReadHitData();
            ClearHitData();
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

        public virtual void TakeDamage(float amount, HitData data = default)
        {
            if(isDead) return;
            
            currentHealth -= amount;
            if(data.hitTarget != null){
                 hitData.Add(data);
            }
            else{
                Debug.LogError("Entity: TakeDamage called with HitData missing hitTarget reference.");
            }
            onDamage?.Invoke();
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

        //Data

        public void ReadHitData()
        {
            if(hitData.Count <= 0) return;

            foreach (HitData data in hitData)
            {
                Debug.Log($"Hit Type: {data.hitType}, Hit Amount: {data.hitAmount}");
            }
        }

        public void ClearHitData()
        {
            hitData.Clear();
        }

        public List<HitData> GetHitData()
        {
            return hitData;
        }

        //Dependencies

        public void GetDependencies()
        {
            movement = gameObject.GetComponent<EntityMovement>();
            ai = gameObject.GetComponent<EntityAI>();
            stateMachine = gameObject.GetComponent<EntityStateMachine>();
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
                if(dependency != null) dependency.Initialize(this);
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
            //extensions = EntityExtensionRegistry.CreateExtensions();

            extensions = new List<EntityExtension>()
            {
                new StatusEffectHandler(), // can have Status Effects
            };
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

        public void AddStatusEffectContribution(StatusEffectContribution contribution)
        {
            StatusEffectHandler handler = GetExtension<StatusEffectHandler>();
            if (handler != null)
            {
                handler.AddEffectContribution(contribution.name, contribution.value);
            }
        }

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
