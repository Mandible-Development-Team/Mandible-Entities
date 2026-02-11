using UnityEngine;
using Mandible.Entities;

namespace Mandible.Entities.StatusEffects
{
    public abstract class StatusEffectData : ScriptableObject
    {
        public string key;
        public string description = "No description provided.";

        [Header("General")]
        public string effectName;
        public float duration;
        public float threshold;

        [Header("Visuals")]
        public GameObject vfxPrefab;

        public abstract StatusEffect CreateRuntimeEffect(Entity owner = default);
    }
}
