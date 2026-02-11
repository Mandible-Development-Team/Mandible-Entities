using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities
{
    public class DynamicHealthbarRenderer : MonoBehaviour
    {
        public Healthbar healthBarPrefab;
        Dictionary<Healthbar, HealthbarInfo> activeHealthbars = new Dictionary<Healthbar, HealthbarInfo>();
        Dictionary<Healthbar, HealthbarInfo> allHealthbars = new Dictionary<Healthbar, HealthbarInfo>();

        [Header("Healthbar")]
        public float heightOffset = 1f;
        public float maxDistance = 25f;
        public float appearSpeed = 5f;
        public bool prioritizeClosest = true;
        Entity targetEntity;
        
        [Header("Raycast Cone")]
        public bool useCone = false;
        public Camera cameraOrigin;
        public Vector3 coneDirection = Vector3.forward;
        public float coneAngle = 30f;
        public float coneLength = 5f;
        public LayerMask hitMask;
        public LayerMask occlusionMask;

        [Header("Debug")]
        public bool debug = false;
        [SerializeField] List<Collider> hits = new List<Collider>();
        HashSet<Entity> hitEntities = new HashSet<Entity>();
        Dictionary<Healthbar, Coroutine> removeCoroutines = new Dictionary<Healthbar, Coroutine>();
        
        void Update()
        {
            RenderHealthbars();
        }

        void OnDisable()
        {
            CleanUpData();
        }

        void RenderHealthbars()
        {
            //Healthbar spawning
            if (useCone)
            {
                hits = RaycastConeAll();
            }
            else
            {
                hits = Physics.RaycastAll(cameraOrigin.transform.position, cameraOrigin.transform.forward, coneLength, hitMask).Select(h => h.collider).ToList();
            }
            
            //Create healthbars for new entities
            hitEntities = new HashSet<Entity>();
            foreach (Collider hit in hits)
            {
                Entity entity = hit.GetComponent<Entity>();
                
                if (hit.TryGetComponent<CriticalPoint>(out var crit)) entity = crit.target as Entity;
                if (entity != null) hitEntities.Add(entity);
                
                if (entity == null || entity.IsDead) continue;
                if (allHealthbars.Any(h => h.Value.target == entity)) continue;
                if (prioritizeClosest && entity != GetClosestEntityToScreenCenter()) continue;

                CreateHealthbarForEntity(entity);
            }            
            
            //Update all healthbars
            foreach (Healthbar bar in allHealthbars.Keys.ToArray())
            {
                var info = allHealthbars[bar];
                UpdateHealthbar(bar, info);
            }

            //Remove healthbars that should be removed
            foreach (Healthbar hb in activeHealthbars.Keys.ToArray())
            {
                var info = allHealthbars[hb];
                if (ShouldRemoveHealthbar(info))
                    StageDestroyHealthbar(hb);
            }
        }

        public void CreateHealthbarForEntity(Entity entity)
        {
            if(entity == null) return;

            Healthbar newBar = Instantiate(healthBarPrefab, transform);
            newBar.Initialize(entity.GetHealthPercentage());
            HealthbarInfo info = new HealthbarInfo
            {
                target = entity,
                distanceToCamera = Vector3.Distance(cameraOrigin.transform.position, ((MonoBehaviour)entity).transform.position)
            };

            activeHealthbars.Add(newBar, info);
            allHealthbars.Add(newBar, info);
        }

        public void UpdateHealthbar(Healthbar hb, HealthbarInfo info = null)
        {
            //Health Data
            float healthPercent = info.target.GetHealthPercentage();
            hb.SetHealthPercentage(healthPercent);

            //Distance Data
            info.distanceToCamera = Vector3.Distance(cameraOrigin.transform.position, ((MonoBehaviour)info.target).transform.position);

            //Positioning
            Vector3 worldPos = ((MonoBehaviour)info.target).transform.position + Vector3.up * heightOffset;
            Vector3 screenPos = cameraOrigin.WorldToScreenPoint(worldPos);

            //Original
            hb.transform.position = screenPos;

            //Experimental (camera space)
            /*
            RectTransform canvasRect = (RectTransform)hb.transform.parent;

            RectTransform rect = hb.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                cameraOrigin,
                out Vector2 localPos
            );

            rect.localPosition = localPos;
            */

            //Hiding
            bool shouldShow = ShouldShowHealthbar(info);

            if(shouldShow){
                ShowHealthbar(hb);
            }
            else 
                HideHealthbar(hb);

            //Culling
            bool shouldCull = IsOffScreen(info);
            hb.gameObject.SetActive(!shouldCull);
        }

        void ShowHealthbar(Healthbar hb)
        {
            hb.Show(appearSpeed);

            //Cancel removal coroutine
            removeCoroutines.TryGetValue(hb, out Coroutine c);
            if(c != null) StopCoroutine(c);
            removeCoroutines.Remove(hb);
        }

        void HideHealthbar(Healthbar hb)
        {
            hb.Hide(appearSpeed);
        }
        
        public void StageDestroyHealthbar(Healthbar hb)
        {
            if (hb == null) return;
            activeHealthbars.Remove(hb);

            if(removeCoroutines.ContainsKey(hb)) StopCoroutine(removeCoroutines[hb]);
            removeCoroutines[hb] = StartCoroutine(DestroyHealthbarAfterFade(hb));
        }
        
        public IEnumerator DestroyHealthbarAfterFade(Healthbar hb)
        {
            if (hb == null) yield break;

            while (hb != null && hb.IsVisible()) yield return null;

            allHealthbars.Remove(hb);
            removeCoroutines.Remove(hb);

            if (hb != null) Destroy(hb.gameObject);
        }

        //Helpers

        bool ShouldRemoveHealthbar(HealthbarInfo info)
        {
            if(info.distanceToCamera > maxDistance) return true;
            if(info.target.IsDead) return true;

            return false;
        }

        bool ShouldShowHealthbar(HealthbarInfo info)
        {
            if(info.distanceToCamera > maxDistance) return false;
            if(info.target.IsDead) return false;

            if(IsOffScreen(info)) return false;
            if(!hitEntities.Contains(info.target)) return false;

            if(prioritizeClosest && info.target != GetClosestEntityToScreenCenter()) return false;

            return true;
        }

        bool IsOffScreen(HealthbarInfo info)
        {
            Vector3 worldPos = ((MonoBehaviour)info.target).transform.position + Vector3.up * heightOffset;
            Vector3 screenPos = cameraOrigin.WorldToScreenPoint(worldPos);

            return screenPos.z < 0f || screenPos.x < 0f || screenPos.x > Screen.width || screenPos.y < 0f || screenPos.y > Screen.height;
        }
        
        Entity GetClosestEntityToScreenCenter()
        {
            Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Entity closest = null;
            float minDist = float.MaxValue;

            foreach (Entity entity in hitEntities)
            {
                Vector3 sp = cameraOrigin.WorldToScreenPoint(((MonoBehaviour)entity).transform.position + Vector3.up * heightOffset);
                if (sp.z < 0f) continue;

                float d = (sp.x - center.x) * (sp.x - center.x) + (sp.y - center.y) * (sp.y - center.y);

                if (d < minDist) { 
                    minDist = d; 
                    closest = entity; 
                }
            }

            return closest;
        }

        //Data

        void CleanUpData()
        {
            //Cleanup
            foreach (Coroutine co in removeCoroutines.Values)
            {
                if (co != null)
                    StopCoroutine(co);
            }

            foreach (Healthbar hb in activeHealthbars.Keys)
            {
                if (hb == null) continue;
                Destroy(hb.gameObject);
            }

            activeHealthbars.Clear();
            allHealthbars.Clear();
        }

        //Raycast

        List<Collider> RaycastConeAll()
        {
            if (cameraOrigin == null)
            {
                Debug.LogError("DynamicHealthbarRenderer: Cone Origin is not assigned.");
                return null;
            }

            Transform origin = cameraOrigin.transform;
            Vector3 originPos = origin.position;
            Vector3 forward = origin.TransformDirection(coneDirection.normalized);

            float cosLimit = Mathf.Cos(coneAngle * Mathf.Deg2Rad);

            HashSet<Collider> seen = new HashSet<Collider>();
            List<Collider> hits = new List<Collider>();

            // Broad phase: volume query
            Collider[] candidates = Physics.OverlapSphere(
                originPos,
                coneLength,
                hitMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (var col in candidates)
            {
                if (!seen.Add(col)) continue;

                Vector3 toTarget = col.bounds.center - originPos;
                float dist = toTarget.magnitude;
                if (dist > coneLength) continue;

                Vector3 dir = toTarget / dist;

                // Cone angle test
                if (Vector3.Dot(forward, dir) < cosLimit)
                    continue;

                // Occlusion test
                if (Physics.Raycast(originPos, dir, out RaycastHit hit, dist, occlusionMask))
                {
                    if (hit.collider != col)
                        continue;
                }

                hits.Add(col);

                if (debug)
                    Debug.DrawLine(originPos, col.bounds.center, Color.red);
            }

            return hits;
        }
    }

    [System.Serializable]
    public class HealthbarInfo
    {
        public Entity target;
        public float distanceToCamera;
    }
}
