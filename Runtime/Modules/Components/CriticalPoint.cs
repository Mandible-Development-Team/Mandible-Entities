using UnityEngine;
using System.Collections;

#if STATUS_EFFECTS
using Mandible.Entities.StatusEffects;
#endif

namespace Mandible.Entities
{
    public class CriticalPoint : MonoBehaviour, IDamageable
    {
        [SerializeField] GameObject targetObject;
        [HideInInspector] public IDamageable target;

        static float damageMultiplier = 1.5f;

        public void Awake()
        {
            if(targetObject == null)
            {
                Debug.LogError("CriticalPoint: "+gameObject.name+" has no targetObject assigned.");
            }

            target = targetObject.GetComponent<IDamageable>();

            if(target == null)
            {
                Debug.LogError("CriticalPoint: "+gameObject.name+" targetObject has no IDamageable component.");
            }
        }

        //API

        public virtual void TakeDamage(float amount)
        {
            target?.TakeDamage(amount * damageMultiplier);
        }

        //Extension Conduct

        #if STATUS_EFFECTS
        public void AddStatusEffectContribution(StatusEffectContribution contribution)
        {
            target?.AddStatusEffectContribution(contribution);
        }
        #endif

        //Getters / Setters
        public virtual HitType GetHitType()
        {
            return HitType.Critical;
        }

        public virtual bool IsDead
        {
            get { return target?.IsDead ?? false; }
        }
    }
}