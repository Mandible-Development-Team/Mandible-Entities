using UnityEngine;
using System.Collections.Generic;

namespace Mandible.Entities {
    public class DamageNumberProfile : ScriptableObject
    {
        public Dictionary<HitType, DamageNumber> profiles = new Dictionary<HitType, DamageNumber>();

        public DamageNumber Evaluate(HitData data)
        {
            return profiles[data.hitType];
        }
    }
}