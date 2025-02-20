using Verse;

namespace Materia
{
    public class DamageWorker_Tranquilizer : DamageWorker_AddInjury
    {
        DamageDef_Tranquilizer Def => (DamageDef_Tranquilizer)def;

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            DamageResult result = base.Apply(dinfo, thing);
            if (thing != null && thing is Pawn pawn && !pawn.Dead && pawn.health != null)
            {
                float severityToAdd = dinfo.Amount * 0.05f;

                HediffDef tranqDef = Def.tranqHediff;
                if (tranqDef != null)
                {
                    Hediff hediff = pawn.health.GetOrAddHediff(tranqDef);
                    if (hediff != null)
                    {
                        hediff.Severity += severityToAdd;
                    }

                }
            }

            return result;
        }
    }
}
