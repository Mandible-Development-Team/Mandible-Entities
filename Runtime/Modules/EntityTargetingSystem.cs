using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities
{
    [System.Serializable]
    public class EntityTargetingSystem
    {
        public Entity owner;

        [Header("Detection")]
        [SerializeField] LayerMask layerMask = ~0;
        [SerializeField] float visionRadius = 10f;
        [SerializeField] float heightOffset = 0f;
        [SerializeField] float forgetTime = 5f;
        [SerializeField] Entity target;
        [SerializeField] List<TargetInfo> targets = new List<TargetInfo>();

        public EntityTargetingSystem(Entity owner)
        {
            this.owner = owner;
        }
        
        public void Initialize(Entity owner)
        {
            this.owner = owner;
        }

        public void UpdateTargets()
        {
            Vector3 origin = owner.transform.position + Vector3.up * heightOffset;
            var colliders = Physics.OverlapSphere(origin, visionRadius, layerMask);

            foreach (var col in colliders) ProcessCollider(col);

            ForgetOldTargets();
            SelectHighestWeightTarget();
        }

        private void ProcessCollider(Collider col)
        {
            Entity entity = GetValidEntity(col);
            if (entity == null) return;

            bool visible = CheckVisibility(entity);

            TargetInfo info = ValidateTargetInfo(entity, visible);
            UpdateTargetInfo(info, entity, visible);
        }

        protected virtual float ComputeTargetWeight(Entity entity, bool visible)
        {
            float weight = 1f;

            float distance = Vector3.Distance(owner.transform.position, entity.transform.position);
            weight *= Mathf.Clamp01(1f - (distance / visionRadius));

            float healthPercentage = entity.GetHealthPercentage();
            weight *= (1f - healthPercentage);

            if (!visible)
            {
                weight *= 0.5f;
            }

            return weight;
        }

        //Targeting Helpers

        private TargetInfo CreateTargetInfo(Entity entity, bool visible)
        {
            return new TargetInfo(entity, ComputeTargetWeight(entity, visible));
        }

        private TargetInfo ValidateTargetInfo(Entity entity, bool visible)
        {
            var info = targets.FirstOrDefault(t => t.entity == entity);
            if (info == null)
            {
                info = CreateTargetInfo(entity, visible);
                targets.Add(info);
            }
            return info;
        }

        private void UpdateTargetInfo(TargetInfo info, Entity entity, bool visible)
        {
            if (visible) UpdateLastKnown(info, entity);

            info.visible = visible;
            info.weight = ComputeTargetWeight(entity, visible);
        }

        private void UpdateLastKnown(TargetInfo info, Entity entity)
        {
            info.lastKnownPosition = entity.transform.position;
            info.lastSeenTime = Time.time;
        }

        private void ForgetOldTargets()
        {
            targets.RemoveAll(t => Time.time - t.lastSeenTime > forgetTime);
        }

        private void SelectHighestWeightTarget()
        {
            target = targets.OrderByDescending(t => t.weight).Select(t => t.entity).FirstOrDefault();
        }

        private Entity GetValidEntity(Collider col)
        {
            if (!col.TryGetComponent(out Entity entity)) return null;
            if (entity == owner || entity.IsDead) return null;

            return entity;
        }

        private bool CheckVisibility(Entity entity)
        {
            return !IsBehindWall(entity);
        }

        //Targeting Getters
        protected virtual bool IsValidTarget(Entity entity)
        {
            return !entity.IsDead && !IsBehindWall(entity);
        }

        protected bool IsBehindWall(Entity entity)
        {
            Vector3 origin = owner.transform.position + Vector3.up * heightOffset;
            Vector3 targetPos = entity.transform.position + Vector3.up * heightOffset;
            Vector3 direction = (targetPos - origin).normalized;
            float distance = Vector3.Distance(origin, targetPos);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, layerMask))
            {
                if (hit.collider.gameObject != entity.gameObject)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual Entity GetTarget()
        {
            return target;
        }
    }

    public class TargetInfo
    {
        public Entity entity;
        public float weight;

        [Header("Info")]
        public bool visible;
        public Vector3 lastKnownPosition;
        public float lastSeenTime;
        
        public TargetInfo(Entity entity, float initialWeight = 1f)
        {
            this.entity = entity;
            this.weight = initialWeight;

            visible = true;
            lastKnownPosition = entity.transform.position;
            lastSeenTime = Time.time;
        }

        public void SetWeight(float newWeight)
        {
            weight = Mathf.Clamp01(newWeight);
        }
    }
}