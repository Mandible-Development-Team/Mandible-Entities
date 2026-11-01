using UnityEngine;
using System.Collections;

using Mandible.Entities.StatusEffects;

namespace Mandible.Entities
{
    public class EntityTransform : MonoBehaviour, IDamageable
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

        void Reset()
        {
            target = transform.root.GetComponentInChildren<IDamageable>();
            if (target != null) targetObject = ((MonoBehaviour)target).gameObject;
        }

        public virtual void TakeDamage(float amount, HitData data = default)
        {
            data.hitTarget = target;
            data.hitType = GetHitType();
            data.hitAmount = amount;

            target?.TakeDamage(amount, data);
        }

        public virtual HitType GetHitType()
        {
            return HitType.Normal;
        }

        public void AddStatusEffectContribution(StatusEffectContribution contribution)
        {
            target?.AddStatusEffectContribution(contribution);
        }

        public bool IsDead
        {
            get { return target?.IsDead ?? false; }
        }
    }
}