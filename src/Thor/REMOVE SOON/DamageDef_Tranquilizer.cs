using Verse;

namespace Materia
{
    public class DamageDef_Tranquilizer : DamageDef
    {
        public float damageToSeverityRatio = 0.05f;
        public HediffDef tranqHediff;

        public DamageDef_Tranquilizer()
        {
            workerClass = typeof(DamageWorker_Tranquilizer);
        }
    }
}
