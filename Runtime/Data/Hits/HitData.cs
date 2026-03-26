using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Mandible.Entities
{
    public struct HitData
    {
        public IDamageable hitTarget;
        public HitType hitType;
        public float hitAmount;
        public RaycastHit hitInfo;
        public Vector3 hitDirection;
    }
}