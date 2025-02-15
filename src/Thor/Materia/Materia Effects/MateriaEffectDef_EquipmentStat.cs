using RimWorld;
using System.Collections.Generic;

namespace Thor
{
    public class MateriaEffectDef_EquipmentStat : MateriaEffectDef
    {
        public StatDef statToAffect;
        public float statFactor = 1f;
        public float statOffset = 0f;

        public List<StatModifier> StatOffsets
        {
            get
            {
                return new List<StatModifier>
            {
                new StatModifier { stat = statToAffect, value = statOffset }
            };
            }
        }

        public List<StatModifier> StatFactors
        {
            get
            {
                return new List<StatModifier>
            {
                new StatModifier { stat = statToAffect, value = statFactor }
            };
            }
        }
    }

    public class MateriaEffect_EquipmentStat : MateriaWorker
    {
        MateriaEffectDef_EquipmentStat StatDef => (MateriaEffectDef_EquipmentStat)def;

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statFactor;
            }
            return 1f;
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statOffset;
            }
            return 0f;
        }
    }
}