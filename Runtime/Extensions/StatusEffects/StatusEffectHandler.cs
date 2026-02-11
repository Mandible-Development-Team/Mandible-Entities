using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Mandible.Core.Data;

namespace Mandible.Entities.StatusEffects
{
    public class StatusEffectHandler : EntityExtension
    {
        [Header("Status Effects")]
        [SerializedDictionary] public SerializedDictionary<StatusEffect, StatusEffectInfo> activeEffects = new SerializedDictionary<StatusEffect, StatusEffectInfo>();
        [SerializedDictionary] public SerializedDictionary<string, float> effectContributions = new SerializedDictionary<string, float>();

        const string path = "StatusEffects/";

        protected override void Start()
        {

        }

        public override void Handle()
        {
            HandleEffectStates();
        }

        void HandleEffectStates()
        {
            List<StatusEffect> effects = activeEffects.Keys.ToList();
            foreach(var effect in effects)
            {
                StatusEffectInfo info = activeEffects[effect];

                //Handle Update
                if (info.currentState == StatusEffectInfo.State.Pending)
                {
                    StartEffectUpdate(effect);
                }

                //Handle Destruction
                else if (info.currentState == StatusEffectInfo.State.Expired)
                {
                    RemoveEffect(effect);
                }
            }
        }

        //Effects

        public void AddEffect<T>() where T : StatusEffect
        {
            StatusEffect effect = GetStatusEffect<T>();

            AddEffect(effect);
        }

        public void AddEffect(StatusEffect effect)
        {
            StatusEffectInfo info = new StatusEffectInfo
            {
                duration = effect.data.duration,
                currentTime = 0f,
                currentState = StatusEffectInfo.State.Pending
            };

            if (activeEffects.ContainsKey(effect))
            {
                activeEffects[effect].currentTime = 0f;
            }
            else
            {
                activeEffects.Add(effect, info);
            }

            GetStatusEffectKey(effect).OnApply();
        }

        public void RemoveEffect(StatusEffect effect)
        {
            //Info
            StopEffectUpdate(effect);

            //Effect
            effect.OnRemove();
            activeEffects.Remove(effect);
        }

        //Effect API

        public void AddDamageOverTime()
        {
            
        }

        //Effect Management

        private IEnumerator ManageStatusEffect(StatusEffect effect, StatusEffectInfo info)
        {
            info.currentState = StatusEffectInfo.State.Active;

            while (info.currentTime < info.duration)
            {
                effect.OnTick(Time.deltaTime);
                info.currentTime += Time.deltaTime;
                yield return null;
            }

            info.currentState = StatusEffectInfo.State.Expired;
        }

        void StartEffectUpdate(StatusEffect effect)
        {
            StatusEffectInfo info = activeEffects[effect];
            if (info != null)
            {
                info.update = entity.StartCoroutine(ManageStatusEffect(effect, info));
            }
        }

        void StopEffectUpdate(StatusEffect effect)
        {
            if (activeEffects.ContainsKey(effect))
            {
                StatusEffectInfo info = activeEffects[effect];

                if (info.update != null)
                {
                    entity.StopCoroutine(info.update);
                    info.update = null;
                }
            }
        }

        //Effect Contributions

        public void AddEffectContribution(StatusEffect effect, float contribution)
        {
            if (effect == null){
                Debug.LogWarning("StatusEffectHandler: Attempted to add contribution to null effect.");
                return;
            }
            string effectName = effect.data.name;

            //Add Contribution
            bool contributionExists = effectContributions.ContainsKey(effectName);
            if (contributionExists)
            {
                effectContributions[effectName] += contribution;
            }
            else
            {
                effectContributions.Add(effectName, contribution);
            }

            //Check Threshold
            if(effectContributions[effectName] > effect.data.threshold)
            {
                effectContributions[effectName] = 0f;
                AddEffect(effect);
            }
        }
        
        public void AddEffectContribution<T>(float contribution) where T : StatusEffect
        {
            StatusEffect effect = GetStatusEffect<T>();
            AddEffectContribution(effect, contribution);
        }

        public void AddEffectContribution(string name, float contribution)
        {
            StatusEffect effect = StatusEffectRegistry.GetStatusEffectByName(name);

            if(effect == null)
            {
                Debug.LogWarning($"StatusEffectHandler: No status effect found with name {name}.");
                return;
            }
            effect.SetOwner(entity);

            AddEffectContribution(effect, contribution);
        }

        public void RemoveEffectContribution(StatusEffect effect)
        {
            string effectName = effect.data.name;

            if (effectContributions.ContainsKey(effectName))
            {
                effectContributions.Remove(effectName);
            }
        }

        //Helpers

        public T GetStatusEffect<T>() where T : StatusEffect
        {
            StatusEffect effect = StatusEffectRegistry.GetStatusEffect<T>(owner: entity); 
            effect?.SetOwner(entity);

            return effect as T; 
        }

        //Used due to referencing issues with StatusEffect keys
        StatusEffect GetStatusEffectKey(StatusEffect keyCandidate)
        {
            foreach (var key in activeEffects.Keys)
            {
                if (ReferenceEquals(key, keyCandidate) || key.Equals(keyCandidate))
                    return key;
            }
            return null;
        }

    }
}