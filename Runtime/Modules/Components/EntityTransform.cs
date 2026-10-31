using UnityEngine;
using System.Collections;

using Mandible.Entities.StatusEffects;

namespace Mandible.Entities
{
    public abstract class EntityTransform : MonoBehaviour, IDamageable
    {
        [SerializeField] public GameObject targetObject;
        [HideInInspector] public IDamageable target;
        public virtual void Awake()
        {
            if(targetObject == null)
            {
                Debug.LogError(GetType()+": "+gameObject.name+" has no targetObject assigned.");
            }

            target = targetObject.GetComponent<IDamageable>();

            if(target == null)
            {
                Debug.LogError(GetType()+": "+gameObject.name+" targetObject has no IDamageable component.");
            }
        }

        public abstract void TakeDamage(float amount, HitData data = default);
        public abstract HitType GetHitType();
        public abstract void AddStatusEffectContribution(StatusEffectContribution contribution);

        public virtual bool IsDead
        {
            get { return target?.IsDead ?? false; }
        }
    }
}