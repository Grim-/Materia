using RimWorld;

namespace Thor
{
    public class MateriaEffectDef_PawnStatOffset : MateriaEffectDef_PawnStat
    {
        public float statOffset = 0f;

        public MateriaEffectDef_PawnStatOffset()
        {
            workerClass = typeof(MateriaEffect_PawnStatOffset);
        }

        public override string EffectDescription
        {
            get
            {
                string statChangeString = statOffset != 0 ? $"+{statOffset}" : "";
                return $"(Pawn)Increases {statToAffect.LabelCap} by {statChangeString}";
            }
        }

        public override string GetExplanationString()
        {
            return $"+{statOffset}";
        }
    }

    public class MateriaEffect_PawnStatOffset : MateriaWorker
    {
        MateriaEffectDef_PawnStatOffset StatDef => (MateriaEffectDef_PawnStatOffset)def;

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