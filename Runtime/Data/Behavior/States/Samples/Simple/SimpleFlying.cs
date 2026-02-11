using UnityEngine;

namespace Mandible.Entities.Actions
{
    [CreateAssetMenu(fileName = "SimpleFlying", menuName = "Mandible/Entities/Entity States/Simple/SimpleFlying", order = 1)]
    public class SimpleFlying : EntityState
    {
        [Header("General")]
        public GameObject projectilePrefab;
        public string projectileSpawnPointName = "ProjectileOwner";
        //public string projectileSpawnPointLocation = "ProjectileOwner";
        public float approachSpeed = 8f;
        public float rotateSpeed = 5f;

        [Header("Attack")]
        public float attackInterval = 2f;

        [Header("Orbiting")]
        public float orbitRadius = 10f;
        public float orbitSpeedDegrees = 90f;
        public float heightOffset = 6f;
        public float orbitForce = 5f;
        public OrbitDirection orbitDirection = OrbitDirection.Clockwise;
        public enum OrbitDirection { Clockwise, CounterClockwise, Random, Deterministic }

        [Header("Procedural Motion")]
        public float bobAmplitude = 0.5f;
        public float bobSpeed = 2f;
        public float jitterUpdateInterval = 0.2f;
        public Vector3 jitterRange = new Vector3(0.3f, 0.2f, 0.3f);
        private float bobOffset = 0f;

        private Transform projectileOwner;
        private float attackT = 0f;
        private Vector3 wanderLocation = Vector3.zero;

        private float angle;
        private float randomDir = 1f;
        private float dir = 1f;

        private float bobTimer = 0f;
        private float randTimer = 0f;
        private Vector3 randomOffset = Vector3.zero;

        public override void OnEnter()
        {
            //projectileOwner = owner.transform.Find(projectileSpawnPointLocation);

            //Idle
            wanderLocation = owner.transform.position;

            //Movement
            angle = Random.Range(0f, 360f);
            randomDir = Random.value < 0.5f ? -1f : 1f;
            bobOffset = Random.Range(0f, 2f * Mathf.PI);

            //Attack
            attackT = Random.Range(0f, attackInterval/2f);
        }

        public override void OnUpdate()
        {
            if (owner.ai.Target == null)
            {
                HandleIdle();
            }
            else
            {
                HandleMovement();
                HandleAttack();                
            }
        }

        public void HandleIdle()
        {
            Vector3 wander = ComputeWanderForce();
            //Vector3 procedural = ComputeProceduralEffects();
            Vector3 finalVector = wander;

            owner.movement.AddForce(finalVector, ForceMode.Force);
        }

        public void HandleMovement()
        {
            Transform target = owner.ai.Target.transform;

            Vector3 toTarget = target.position - owner.transform.position;
            float dist = toTarget.magnitude;
            Vector3 dir = toTarget.normalized;

            Vector3 orbitForceVec = ComputeOrbitForce(target.position);
            Vector3 procedural = ComputeProceduralEffects();
            Vector3 finalVector = orbitForceVec + procedural;

            owner.movement.AddForce(finalVector, ForceMode.Force);

            // Rotation toward target
            Vector3 look = target.position - owner.transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0f) owner.movement.MoveRotation(Quaternion.LookRotation(look.normalized), rotateSpeed);
        }

        public void HandleAttack()
        {
            attackT += Time.deltaTime;
            if (attackT >= attackInterval)
            {
                attackT = 0f;
                FireProjectile();
            }
        }

        public void FireProjectile()
        {
            Transform projectileOwner = owner.transform.Find(projectileSpawnPointName);
            if(projectileOwner == null) projectileOwner = owner.transform;

            Vector3 dirToTarget = (owner.ai.Target.transform.position - projectileOwner.position).normalized;
            Vector3 dirError = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            Vector3 dir = (dirToTarget + dirError).normalized;

            Projectile projectile = Instantiate(projectilePrefab, projectileOwner.position + dir, owner.transform.rotation).GetComponent<Projectile>();
            projectile.GetComponent<Rigidbody>().linearVelocity = owner.transform.forward * projectile.speed;
        }

        private Vector3 ComputeWanderForce()
        {
            //Compute wander
            Vector3 wander = wanderLocation;
            wander.y = 0f;

            Vector3 current = owner.transform.position;
            current.y = 0f;

            Vector3 toWander = wander - current;
            float distToWanderPoint = toWander.magnitude;

            //Rotate towards wander
            if(toWander != Vector3.zero) owner.movement.MoveRotation(Quaternion.LookRotation(toWander.normalized), rotateSpeed);

            //Approach wander
            if (distToWanderPoint < 0.1f)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-10f, 10f), owner.transform.position.y, Random.Range(-10f, 10f));
                wanderLocation = owner.transform.position + randomOffset;
            }

            return toWander.normalized * approachSpeed * Time.deltaTime;
        }

        // Orbit

        private Vector3 ComputeOrbitForce(Vector3 center)
        {
            Vector3 pos = owner.transform.position;
            Vector3 vel = owner.movement.Velocity;

            // Determine orbit direction
            if (orbitDirection == OrbitDirection.Clockwise) dir = -1f;
            else if (orbitDirection == OrbitDirection.CounterClockwise) dir = 1f;
            else if (orbitDirection == OrbitDirection.Random) dir = randomDir;
            else if (orbitDirection == OrbitDirection.Deterministic) dir = DetermineOrbitDirection(center);

            angle += orbitSpeedDegrees * dir * Time.deltaTime;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 desiredFlatPos = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;

            Vector3 flatError = new Vector3(desiredFlatPos.x - pos.x, 0f, desiredFlatPos.z - pos.z);
            Vector3 flatVel = new Vector3(vel.x, 0f, vel.z);

            float Kp = orbitForce;
            float Kd = 2f * Mathf.Sqrt(Kp);

            Vector3 forceXZ = (flatError * Kp) - (flatVel * Kd);

            if (forceXZ.sqrMagnitude > orbitForce * orbitForce)
                forceXZ = forceXZ.normalized * orbitForce;

            return forceXZ;
        }

        private float DetermineOrbitDirection(Vector3 center)
        {
            Vector3 toCenter = owner.transform.position - center;
            Vector3 flatToCenter = new Vector3(toCenter.x, 0f, toCenter.z);
            float distance = flatToCenter.magnitude;

            if (distance > orbitRadius)
            {
                float relativeAngle = Mathf.Atan2(flatToCenter.z, flatToCenter.x) * Mathf.Rad2Deg;
                float normalizedAngle = (angle + 360f) % 360f;
                float diff = (relativeAngle - normalizedAngle + 360f) % 360f;
                return diff > 180f ? -1f : 1f;
            }
            else
                return dir;
        }

        //Procedural
        private Vector3 ComputeProceduralEffects()
        {
            //Bob
            bobTimer += Time.deltaTime;
            float bobAmount = Mathf.Sin(bobTimer * bobSpeed + bobOffset) * bobAmplitude;

            //Jitter
            randTimer += Time.deltaTime;
            if (randTimer > jitterUpdateInterval)
            {
                randomOffset = new Vector3(
                    Random.Range(-jitterRange.x, jitterRange.x),
                    Random.Range(-jitterRange.y, jitterRange.y),
                    Random.Range(-jitterRange.z, jitterRange.z)
                );
                randTimer = 0f;
            }

            //Correction
            float tempY = bobAmount + randomOffset.y;
            float targetY = 0f;
            if(owner.ai.Target != null) targetY = owner.ai.Target.transform.position.y + heightOffset;
            
            float yVel = owner.movement.Velocity.y;
            float Kp = orbitForce;
            float Kd = 2f * Mathf.Sqrt(Kp);
            float yForce = (targetY - (owner.transform.position.y + tempY)) * Kp - yVel * Kd;

            return new Vector3(randomOffset.x, tempY + yForce, randomOffset.z);
        }

    }
}
