using UnityEngine;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities.Actions
{
    [CreateAssetMenu(fileName = "DroneDeath", menuName = "Mandible/Entities/Entity States/DroneDeath", order = 1)]
    public class DroneDeath : EntityState
    {
        public override string description =>
            "Death state for Drones and simple flying machines.";

        [Header("General")]
        public GameObject deathEffectPrefab;

        // Runs when state is entered
        public override void OnEnter()
        {
            if(deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, owner.transform.position, Quaternion.identity);
                AddConstraint(effect);
                effect.GetComponent<ParticleSystem>().Play();
            }

            owner.stateMachine.EnablePhysics(true);
            owner.movement.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            owner.movement.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        public void AddConstraint(GameObject effect)
        {
            ParentConstraint constraint = effect.AddComponent<ParentConstraint>();
            constraint.AddSource(new ConstraintSource { sourceTransform = owner.transform, weight = 1 });
            constraint.constraintActive = true;
            constraint.locked = true;
        }
    }
}
