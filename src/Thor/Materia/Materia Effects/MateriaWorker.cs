using Verse;

namespace Thor
{
    public class MateriaWorker : IExposable
    {
        public MateriaEffectDef def;
        public Pawn EquippingPawn;
        public ThingWithComps ParentEquipment;

        public MateriaWorker()
        {

        }

        public MateriaWorker(MateriaEffectDef def, Pawn equippingPawn, ThingWithComps parentEquipment)
        {
            this.def = def;
            EquippingPawn = equippingPawn;
            ParentEquipment = parentEquipment;
        }

        public virtual void Notify_Equipped(Pawn pawn)
        {
        
        }
        public virtual void Notify_Unequipped(Pawn pawn)
        {
        
        }

        public virtual void OnTick(Pawn pawn)
        {

        }

        public virtual void Notify_MateriaLinked(MateriaDef sourceMateria, MateriaDef linkedMateria)
        {
        
        }
        public virtual void Notify_MateriaUnlinked(MateriaDef sourceMateria, MateriaDef linkedMateria)
        {
        
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, DamageWorker.DamageResult DamageWorkerResult)
        {
            return DamageWorkerResult;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
        }

    }

}
