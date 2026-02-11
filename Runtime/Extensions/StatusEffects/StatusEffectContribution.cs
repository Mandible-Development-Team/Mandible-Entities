using System.Collections;

namespace Mandible.Entities.StatusEffects
{
    [System.Serializable]
    public class StatusEffectContribution
    {
        public string name;
        public float value;

        public StatusEffectContribution(string name, float value)
        {
            this.name = name;
            this.value = value;
        }

        public StatusEffect GetEffect()
        {
            return StatusEffectRegistry.GetStatusEffectByName(name);
        }
    }
}