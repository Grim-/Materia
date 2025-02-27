using Verse;

namespace Materia
{
    public class SpecialThingFilterWorker_MateriaWeapons : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            if (t.TryGetComp(out Comp_Materia Comp_MateriaComp))
            {                
               return Comp_MateriaComp.MateriaSlots.Count < MateriaSlotGenerator.GetMaxSlotsAllowedFor(t.def);
            }

            return false;
        }

        //public override bool AlwaysMatches(ThingDef def)
        //{
        //    return def.HasComp(typeof(Comp_MateriaComp));
        //}
    }
}
