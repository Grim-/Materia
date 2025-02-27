using RimWorld;

namespace Materia
{
    public class MateriaEffectDef_PawnStatFactor : MateriaEffectDef_PawnStat
    {
        public float statFactor = 1f;
        public MateriaEffectDef_PawnStatFactor()
        {
            workerClass = typeof(MateriaEffect_PawnStatFactor);
        }
        public override string EffectDescription
        {
            get
            {
                string statChangeString = statFactor != 1 ? $"x{statFactor}" : "";
                return $"(Pawn)Increases {statToAffect.LabelCap} by {statChangeString}";
            }
        }

        public override string GetExplanationString()
        {
            return $"x{statFactor}";
        }
    }

    public class MateriaEffect_PawnStatFactor : MateriaWorker
    {
        MateriaEffectDef_PawnStatFactor StatDef => (MateriaEffectDef_PawnStatFactor)def;

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statFactor;
            }
            return 1f;
        }
    }
}