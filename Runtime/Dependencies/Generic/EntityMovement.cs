using UnityEngine;

namespace Mandible.Entities
{
    public class EntityMovement : EntityDependency
    {
        [SerializeField] public Rigidbody rigidBody;

        [Header("General")]
        public float maxSpeed = 12f;

        [Header("Avoidance (Separation)")]
        public bool avoidsEntities = true;
        public float avoidanceRadius = 3f;
        public float avoidanceStrength = 6f;
        public LayerMask avoidanceMask;

        private Vector3 accumulatedForce;

        public Vector3 Velocity => rigidBody != null ? rigidBody.linearVelocity : Vector3.zero;

        public void Start()
        {
            rigidBody = owner.GetComponent<Rigidbody>();
        }

        public override void Handle()
        {
            if (owner.IsDead) return;

            if (avoidsEntities) ApplyAvoidance();
            ApplyMovement();
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            if (rigidBody != null)
                rigidBody.AddForce(force, mode);
            else
                accumulatedForce += force;
        }

        public void MoveRotation(Quaternion rotation, float lerpSpeed = 0f)
        {
            if (rigidBody != null)
            {
                if(lerpSpeed > 0f)
                {
                    rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, rotation, Time.fixedDeltaTime * lerpSpeed));
                }
                else
                {
                    rigidBody.MoveRotation(rotation);
                }
            }
            else
                owner.transform.rotation = rotation;
        }

        private void ApplyMovement()
        {
            if (accumulatedForce.sqrMagnitude > maxSpeed * maxSpeed)
                accumulatedForce = accumulatedForce.normalized * maxSpeed;

            if (rigidBody == null)
                owner.transform.position += accumulatedForce;

            accumulatedForce = Vector3.zero;
        }

        private void ApplyAvoidance()
        {
            Vector3 f = ComputeSeparation(owner.transform.position, avoidanceRadius, avoidanceStrength, avoidanceMask, owner.transform);
            AddForce(f, ForceMode.Force);
        }

        private const float COMPUTE_SEPARATION_DIST_EPSILON = 0.0001f;
        private Vector3 ComputeSeparation(Vector3 position, float radius, float strength, LayerMask mask, Transform self)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, mask);
            Vector3 force = Vector3.zero;

            foreach (var hit in hits)
            {
                if (hit.transform == self || hit.transform.IsChildOf(self) || self.IsChildOf(hit.transform)) continue;
                Vector3 dir = position - hit.transform.position;
                float dist = dir.magnitude;

                if (dist <= COMPUTE_SEPARATION_DIST_EPSILON) continue;
                force += dir.normalized * (1f - dist / radius);
            }

            return force.normalized * strength;
        }
    }
}
