using RimWorld;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_Revival : MateriaEffectDef
    {
        public float reviveHealthPercentage = 0.3f;
        public SoundDef reviveSound;

        public MateriaEffectDef_Revival()
        {
            workerClass = typeof(MateriaEffect_Revival);
        }
    }

    public class MateriaEffect_Revival : MateriaWorker
    {
        MateriaEffectDef_Revival Def => (MateriaEffectDef_Revival)def;

        public override void Notify_OwnerKilled()
        {
            if (this.EquippingPawn != null && this.EquippingPawn.Corpse != null && this.EquippingPawn.Corpse.InnerPawn != null)
            {
                //Log.Message("Notify_OwnerKilled");
                if (ResurrectionUtility.TryResurrect(this.EquippingPawn.Corpse.InnerPawn))
                {
                    Messages.Message($"{this.Def.LabelCap} Revived {this.EquippingPawn.Label} and shattered.", MessageTypeDefOf.PositiveEvent);
                    DestroyParentMateria();
                }
            }


        }
    }
}