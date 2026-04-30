using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities
{
    [DefaultExecutionOrder(-100)]
    public class DynamicDamageRenderer : MonoBehaviour
    {
        public Camera cameraOrigin;
        public DamageNumber damageNumberPrefab;

        [Header("Settings")]
        public float renderRadius = 50f;
        public LayerMask entityLayer;
        public List<Entity> entitiesToRender = new List<Entity>();

        void Start()
        {
            
        }

        // Update is called once per frame
        void LateUpdate()
        {
            GetEntities();
            DrawDamageNumbers();
        }

        void DrawDamageNumbers()
        {
            foreach(Entity entity in entitiesToRender)
            {
                entity.GetHitData().ForEach(data =>
                {
                    DrawDamageNumber(data);
                });
            }
        }

        void DrawDamageNumber(HitData data)
        {
            Entity target = data.hitTarget as Entity;
            Vector3 worldPosition = target.transform.position;
            Vector3 screenPosition = cameraOrigin.WorldToScreenPoint(worldPosition);

            if (screenPosition.z <= 0) return; // Behind Camera
            bool onScreen =
                screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                screenPosition.y >= 0 && screenPosition.y <= Screen.height;
            if (!onScreen) return;
            
            DamageNumber dmg = Instantiate(damageNumberPrefab, transform);
            dmg.SetCamera(cameraOrigin);
            dmg.damage = data.hitAmount;
            dmg.transform.position = screenPosition;
        }

        void GetEntities()
        {
            entitiesToRender.Clear();
            Collider[] hits = Physics.OverlapSphere(cameraOrigin.transform.position, renderRadius);
            foreach (Collider col in hits)
            {
                Entity entity = col.GetComponent<Entity>();
                if (entity != null)
                    entitiesToRender.Add(entity);
            }
        }
    }
}
