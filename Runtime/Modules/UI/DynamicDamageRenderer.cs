using UnityEngine;
using System.Collections.Generic;

namespace Mandible.Entities
{
    [DefaultExecutionOrder(100)]
    public class DynamicDamageRenderer : MonoBehaviour
    {
        public Camera cameraOrigin;
        public DamageNumber damageNumberPrefab;

        [Header("Settings")]
        public float renderRadius = 50f;

        private HashSet<Entity> subscribedEntities = new HashSet<Entity>();

        void Update()
        {
            foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
            {
                if (!subscribedEntities.Contains(entity))
                {
                    entity.OnDamageReceived += HandleDamageEvent;
                    subscribedEntities.Add(entity);
                }
            }
        }

        void HandleDamageEvent(HitData data)
        {
            Entity target = data.hitTarget as Entity;
            if (target == null) return;

            float dist = Vector3.Distance(cameraOrigin.transform.position, target.transform.position);
            if (dist > renderRadius) return;

            Vector3 worldPos = target.transform.position;
            Collider col = target.GetComponent<Collider>();

            if (data.hitPosition != default)
            {
                worldPos = data.hitPosition;
            }
            else if (col != null)
            {
                worldPos = col.bounds.center;
            }
    
            Vector3 screenPosition = cameraOrigin.WorldToScreenPoint(worldPos);
            if (screenPosition.z <= 0) return;
            
            bool onScreen = screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                            screenPosition.y >= 0 && screenPosition.y <= Screen.height;
            if (!onScreen) return;
            
            DrawDamageNumber(data, screenPosition);
        }

        void DrawDamageNumber(HitData data, Vector3 screenPos)
        {
            DamageNumber dmg = Instantiate(damageNumberPrefab, transform);
            dmg.SetCamera(cameraOrigin);
            dmg.damage = data.hitAmount;
            dmg.transform.position = screenPos;
        }

        void OnDisable()
        {
            foreach (var entity in subscribedEntities)
            {
                if (entity != null) entity.OnDamageReceived -= HandleDamageEvent;
            }
        }
    }
}