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
            // Only search for new entities to subscribe to. 
            // We do this less frequently if needed, or keep it in Update for simplicity.
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

            // 1. DISTANCE CHECK (The Sphere Logic)
            float dist = Vector3.Distance(cameraOrigin.transform.position, target.transform.position);
            if (dist > renderRadius) return; // Ignore if outside sphere

            // 2. FRUSTUM CHECK (The "In Front" Logic)
            Vector3 screenPosition = cameraOrigin.WorldToScreenPoint(target.transform.position);
            if (screenPosition.z <= 0) return; // Behind Camera
            
            bool onScreen = screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                            screenPosition.y >= 0 && screenPosition.y <= Screen.height;
            if (!onScreen) return;
            
            // 3. RENDER
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