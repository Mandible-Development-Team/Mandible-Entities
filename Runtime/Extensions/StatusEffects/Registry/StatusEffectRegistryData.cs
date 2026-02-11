using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Mandible.Registry;

namespace Mandible.Entities.StatusEffects
{
    #if MANDIBLE_DEVELOPER_MODE
    [CreateAssetMenu(fileName = "StatusEffectRegistryData", menuName = "Mandible/Entities/Developer/Status Effect Registry Data", order = 0)]
    #endif
    
    public class StatusEffectRegistryData : ScriptableObject
    {
        public List<StatusEffectData> statusEffects = new List<StatusEffectData>();
    }
}
