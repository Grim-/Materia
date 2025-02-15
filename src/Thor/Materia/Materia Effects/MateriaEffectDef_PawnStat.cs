using RimWorld;

namespace Thor
{
    public class MateriaEffectDef_PawnStat : MateriaEffectDef
    {
        public StatDef statToAffect;
        public float statFactor = 1f;
        public float statOffset = 0f; 
    }



    public class MateriaEffect_PawnStat : MateriaWorker
    {
        MateriaEffectDef_PawnStat StatDef => (MateriaEffectDef_PawnStat)def;

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