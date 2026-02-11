using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mandible.Entities.StatusEffects;

namespace Mandible.Entities
{
    public class Projectile : MonoBehaviour
    {
        [Header ("General Settings")]
        public Rigidbody rb;
        public GameObject sender;
        [HideInInspector] public AudioSource audioSource;

        [Header("Projectile Settings")]
        public float speed = 20f;
        public Vector3 launchDirection = Vector3.zero;
        public float lifetime = 5f;
        public float damage = 10f;
        public Vector3 facingDirection;
        public bool destroyOnHit = true;
        public float timeUntilDestroy = 0f;
        [SerializeField] private bool isHalted;
        public bool connectOnHit;
        public bool hasHit;
        public bool isConnected;
        public GameObject connectObject;
        public bool isHoming;
        public bool isParabolic;
        public bool isExplosive;
        public GameObject objectToHome;
        public float homingSmoothness;
        public LayerMask hitLayers;
        public bool forwardIsUp;
        
        #if STATUS_EFFECTS
        [Header("Status Effect")]
        public StatusEffectContribution contribution;
        #endif

        [Header("Physics")]
        [SerializeField] private bool hasImpulse;

        [Header("Effects")]
        public ParticleSystem impactEffect;
        public ParticleSystem connectEffect;
        public AudioClip impactSFX;
        public LineRenderer ropeRenderer;
        public Transform attachedTarget; 

        private float timer = 0f;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            Vector3 launchDir = transform.forward.normalized;
            /*
            rb.linearVelocity = launchDir*speed;
            
            if(hasImpulse){
                rb.AddForce(launchDir * impulseSpeed, ForceMode.Impulse);
            }
            */
        }

        void LateUpdate(){
            RenderRope();
        }


        void FixedUpdate()
        {
            if (isHoming && !isConnected)
            {
                FollowTarget();
            }

            float currentSpeed = speed;

            if (isConnected || isHalted)
            {
                rb.linearVelocity = Vector3.zero;
            }

            //Track Lifecycle
            timer += Time.deltaTime;

            if (!isConnected && timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        void SpawnImpactEffect(Collision collision)
        {
            if(impactEffect == null) return;
            ContactPoint contact = collision.contacts[0];

            Quaternion hitRotation = Quaternion.LookRotation(contact.normal);
            Instantiate(impactEffect, contact.point, hitRotation);
        }

        void OnCollisionEnter(Collision other)
        {
            SpawnImpactEffect(other);

            if (isExplosive)
            {
                HandleExplosion();
                return;
            }
            else
            {
                HandleHit(other.collider);
            }

            if (connectOnHit)
            {
                isConnected = true;
                isHalted = true;
                connectObject = other.transform.gameObject;


                rb.linearVelocity = Vector3.zero;
                connectEffect?.Play();
                audioSource.PlayOneShot(impactSFX);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject, timeUntilDestroy);
            }

        }

        public void HandleHit(Collider hitCollider)
        {
            //Damage
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                #if STATUS_EFFECTS
                damageable.AddStatusEffectContribution(contribution);
                #endif

                if(!damageable.IsDead) damageable.TakeDamage(damage);
            }
        }

        public void HandleExplosion()
        {
            float explosionRadius = 7.5f;
            float explosionForce = 200f;
            Vector3 explosionPosition = transform.position;

            Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius, hitLayers);

            foreach (Collider nearbyObject in colliders)
            {
                HandleHit(nearbyObject);

                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

                if (rb != null)
                    rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
            }

            Destroy(gameObject);
        }

        public void SetSender(GameObject sender)
        {
            this.sender = sender;
        }

        public void SetTarget(GameObject target){
            objectToHome = target;
        }

        public void ReturnToSender(float time, bool deleteOnReturn){
            StartCoroutine(ReturnToSenderCoroutine(time, deleteOnReturn));
        }

        public IEnumerator ReturnToSenderCoroutine(float totalTime, bool deleteOnReturn)
        {
            float currentTime = 0f;
            Vector3 startPosition = transform.position;
            Vector3 endPosition = sender.transform.position;

            while (currentTime < totalTime)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / totalTime;
                transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }

            rb.MovePosition(endPosition);

            if (deleteOnReturn)
            {
                Destroy(gameObject);
            }
        }

        public void FollowTarget(){
            if(objectToHome == null || isConnected) return;

            Vector3 directionToTarget = (objectToHome.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            if(homingSmoothness >= 1){
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, homingSmoothness * Time.deltaTime);
            }
            else{
                transform.rotation = targetRotation;
            }

            float distance = Vector3.Distance(transform.position, objectToHome.transform.position);
            if (distance < 0.2f)
            {
                isConnected = true;
                isHalted = true;
                connectObject = objectToHome;
                transform.SetParent(objectToHome.transform);
                GetComponent<Collider>().enabled = false;
            }
        }

        void RenderRope()
        {
            //Rope
            if (ropeRenderer != null)
            {
                Vector3 originPoint = sender.transform.position;
                Vector3 anchorPoint = transform.position; 

                ropeRenderer.SetPosition(0, originPoint); 
                ropeRenderer.SetPosition(1, anchorPoint);
            }
        }
    }
}


