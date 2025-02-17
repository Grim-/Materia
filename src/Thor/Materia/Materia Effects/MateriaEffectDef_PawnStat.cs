using RimWorld;

namespace Thor
{
    public abstract class MateriaEffectDef_PawnStat : MateriaEffectDef
    {
        public StatDef statToAffect;
        public abstract string GetExplanationString();
    }

}