using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities.Actions
{
    [CreateAssetMenu(fileName = "SimpleDrone", menuName = "Mandible/Entities/Entity States/SimpleDrone", order = 1)]
    public class SimpleDrone : EntityState
    {
        public override string description =>
            "Intelligent drone with hover, obstacle avoidance, wander, and orbit attack.";

        [Header("Movement")]
        public float moveForce = 8f;             // Horizontal movement force
        public float strafeSpeed = 2f;           // Orbit angular speed
        public float rotationSpeed = 5f;

        [Header("Wander")]
        public float wanderForce = 1f;
        public float wanderRadius = 8f;
        public float wanderArrivalDistance = 1.5f;
        public float maxTraversalTime = 5f;           // Max seconds to reach target
        public float waitBetweenWanders = 1f;          // Pause after reaching a point
        [Range(0f, 1f)] public float waitRandomness = 1f;
        
        [Header("Hover")]
        public float hoverHeight = 5f;          // Desired height above ground/target
        public float hoverForce = 10f;           // Upward force when below desired height

        [Header("Combat")]
        public float attackInterval = 0.5f;
        public float distanceToAttack = 15f;
        public float distanceToEngage = 10f;     // Switch to orbit when closer
        public float orbitRadius = 5f;           // Circle radius around target
        
        [Header("Projectile")]
        public GameObject projectilePrefab;
        public string projectileSpawnPointName = "ProjectileOwner";

        [Header("Obstacle Avoidance")]
        public float avoidanceForce = 12f;        // Force to push away from obstacles
        public float sensorRange = 4f;            // Raycast distance
        public LayerMask obstacleMask = ~0;       // What blocks the drone

        // Sensor directions (simulates real drone proximity sensors)
        private Vector3[] sensorDirections = new Vector3[]
        {
            Vector3.forward, Vector3.back, Vector3.right, Vector3.left,
            Vector3.up, Vector3.down,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };

        private Entity target;
        private float distFromTarget;
        private Vector3 toTarget;
        private Vector3 dirToTarget;
        private Vector3 lookDir;

        //Wander
        private Vector3 wanderTarget;
        private float traversalTimer;
        private float waitTimer;
        private bool isWaiting;

        public override void OnEnter() { }

        public override void OnUpdate()
        {
            GatherData();

            Hover();                // Maintain altitude
            AvoidObstacles();       // Push away from nearby objects
            HandleStates();         // Decide wander or attack

            LookAtTarget();
        }

        void HandleStates()
        {
            if (owner.ai.Target == null)
                Wander();
            else
                Attack();
        }

        // ----------------------------------------------------------------------
        // Data
        // ----------------------------------------------------------------------
        void GatherData()
        {
            target = owner.ai.Target;
            if (target != null)
            {
                toTarget = target.transform.position - owner.transform.position;
                distFromTarget = toTarget.magnitude;
                dirToTarget = toTarget.normalized;
                lookDir = dirToTarget;
            }
        }

        void LookAtTarget()
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            owner.movement.MoveRotation(targetRotation, rotationSpeed);
        }

        // ----------------------------------------------------------------------
        // Hover – simple spring force (no velocity read)
        // ----------------------------------------------------------------------
        void Hover()
        {
            float desiredHeight;
            if (target != null)
                desiredHeight = target.transform.position.y + hoverHeight;
            else
            {
                RaycastHit groundHit;
                if (Physics.Raycast(owner.transform.position, Vector3.down, out groundHit, 100f, obstacleMask))
                    desiredHeight = groundHit.point.y + hoverHeight;
                else
                    desiredHeight = owner.transform.position.y; // fallback
            }

            float error = desiredHeight - owner.transform.position.y;
            if (Mathf.Abs(error) > 0.05f)
            {
                Vector3 upwardForce = Vector3.up * (error * hoverForce);
                owner.movement.AddForce(upwardForce, ForceMode.Force);
            }
        }

        // ----------------------------------------------------------------------
        // Obstacle avoidance – raycast in all directions
        // ----------------------------------------------------------------------
        void AvoidObstacles()
        {
            Vector3 avoidance = Vector3.zero;

            foreach (Vector3 localDir in sensorDirections)
            {
                Vector3 worldDir = owner.transform.TransformDirection(localDir);
                RaycastHit hit;
                if (Physics.Raycast(owner.transform.position, worldDir, out hit, sensorRange, obstacleMask))
                {
                    float strength = 1f - (hit.distance / sensorRange); // closer = stronger
                    Vector3 avoidDir = Vector3.Lerp(worldDir, hit.normal, 0.5f).normalized;
                    avoidance += avoidDir * avoidanceForce * strength;
                    Debug.DrawRay(owner.transform.position, worldDir * hit.distance, Color.red);
                }
                else
                {
                    Debug.DrawRay(owner.transform.position, worldDir * sensorRange, Color.green);
                }
            }

            if (avoidance.magnitude > 0.01f)
                owner.movement.AddForce(avoidance, ForceMode.Force);
        }

        // ----------------------------------------------------------------------
        // Wander – random waypoints with obstacle avoidance
        // ----------------------------------------------------------------------
        void Wander()
        {
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    // Wait finished – pick a new target and start moving
                    isWaiting = false;
                    waitTimer = waitBetweenWanders * (1f - waitRandomness * Random.value);
                    wanderTarget = GetValidWanderTarget();
                    traversalTimer = 0f;
                }
                // While waiting, do nothing else
                return;
            }

            if (wanderTarget == Vector3.zero || IsAtWanderTarget() || traversalTimer > maxTraversalTime)
            {
                isWaiting = true;
                waitTimer = waitBetweenWanders;
                wanderTarget = Vector3.zero;   // Clear target so we stop moving
                return;
            }

            traversalTimer += Time.deltaTime;

            Vector3 toWander = wanderTarget - owner.transform.position;
            toWander.y = 0;

            if (toWander.magnitude > 0.5f)
            {
                Vector3 moveDir = toWander.normalized;
                lookDir = moveDir;
                owner.movement.AddForce(moveDir * wanderForce, ForceMode.Force);
            }
        }

        const int MAX_WANDER_ATTEMPTS = 5;
        Vector3 GetValidWanderTarget()
        {
            for (int attempts = 0; attempts < MAX_WANDER_ATTEMPTS; attempts++) // Try multiple times
            {
                // Generate random direction within a cone for smoother movement
                Vector3 randomDir = Random.insideUnitSphere;
                randomDir.y = 0; // Keep on ground plane
                randomDir.Normalize();
                
                // Add some persistence to avoid constant direction changes
                if (wanderTarget != Vector3.zero)
                {
                    randomDir = Vector3.Lerp((wanderTarget - owner.transform.position).normalized, randomDir, 0.5f);
                }
                
                float distance = Random.Range(wanderRadius * 0.5f, wanderRadius);
                Vector3 candidateTarget = owner.transform.position + randomDir * distance;
                
                // Check if the path to target is clear
                if (IsPathClear(candidateTarget))
                {
                    return candidateTarget;
                }
            }
            
            // Fallback: try to go in a direction away from obstacles
            return GetAvoidanceTarget();
        }

        bool IsAtWanderTarget()
        {
            Vector3 toTarget = wanderTarget - owner.transform.position;
            toTarget.y = 0;
            return toTarget.magnitude <= wanderArrivalDistance;
        }

        bool IsPathClear(Vector3 target)
        {
            Vector3 direction = target - owner.transform.position;
            float distance = direction.magnitude;
            direction.Normalize();
            
            // Raycast from current position to target
            if (Physics.Raycast(owner.transform.position, direction, out RaycastHit hit, distance, obstacleMask))
            {
                // Path is blocked
                Debug.DrawRay(owner.transform.position, direction * hit.distance, Color.yellow);
                return false;
            }
            
            // Also check if the target position itself is valid (not inside an obstacle)
            if (Physics.CheckSphere(target, 0.5f, obstacleMask))
            {
                return false;
            }
            
            Debug.DrawRay(owner.transform.position, direction * distance, Color.green);
            return true;
        }

        Vector3 GetAvoidanceTarget()
        {
            // Cast rays in all directions to find the clearest path
            Vector3 bestDirection = Vector3.zero;
            float furthestClearDistance = 0f;
            
            int numRays = 8;
            for (int i = 0; i < numRays; i++)
            {
                float angle = (i / (float)numRays) * 360f;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                
                if (!Physics.Raycast(owner.transform.position, direction, out RaycastHit hit, wanderRadius, obstacleMask))
                {
                    // No obstacle in this direction
                    return owner.transform.position + direction * wanderRadius;
                }
                
                if (hit.distance > furthestClearDistance)
                {
                    furthestClearDistance = hit.distance;
                    bestDirection = direction;
                }
                
                Debug.DrawRay(owner.transform.position, direction * hit.distance, Color.magenta);
            }
            
            // Return a point in the best direction found
            return owner.transform.position + bestDirection * (furthestClearDistance * 0.8f);
        }

        // ----------------------------------------------------------------------
        // Attack – close in then orbit
        // ----------------------------------------------------------------------
        void Attack()
        {
            if (distFromTarget > distanceToEngage)
                MoveToTarget();
            else
                OrbitTarget();

            if (distFromTarget <= distanceToAttack)
                HandleFire();

            if (target == null) return;

        }

        float attackT = 0f;
        public void HandleFire()
        {
            attackT += Time.deltaTime;
            if (attackT >= attackInterval)
            {
                attackT = 0f;
                FireProjectile();
            }
        }

        private const float DIR_ERROR_SPREAD_SCALAR = 0.1f;
        public void FireProjectile()
        {
            Transform projectileOwner = owner.transform.Find(projectileSpawnPointName);
            if(projectileOwner == null) projectileOwner = owner.transform;

            Vector3 dirToTarget = (owner.ai.Target.transform.position - projectileOwner.position).normalized;
            Vector3 dirError = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * DIR_ERROR_SPREAD_SCALAR;

            Vector3 dir = (dirToTarget + dirError).normalized;
            Quaternion rot = Quaternion.LookRotation(dir);

            Debug.DrawRay(projectileOwner.position + dir, rot * Vector3.forward, Color.red, 5f);

            Projectile projectile = Instantiate(projectilePrefab, projectileOwner.position + dir, rot).GetComponent<Projectile>();
            projectile.GetComponent<Rigidbody>().linearVelocity = dir * projectile.speed;
        }

        void MoveToTarget()
        {
            if (target == null) return;
            owner.movement.AddForce(dirToTarget * moveForce, ForceMode.Force);
        }

        void OrbitTarget()
        {
            float angle = Time.time * strafeSpeed;
            Vector3 orbitOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * orbitRadius;
            Vector3 desiredPos = target.transform.position + orbitOffset;
            desiredPos.y = owner.transform.position.y; // hover handles Y

            Vector3 toDesired = desiredPos - owner.transform.position;
            if (toDesired.magnitude > 0.2f)
            {
                Vector3 orbitDir = toDesired.normalized;
                owner.movement.AddForce(orbitDir * moveForce, ForceMode.Force);
            }
        }
    }
}