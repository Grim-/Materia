using Verse;

namespace Materia
{
    public class SpecialThingFilterWorker_MateriaWeapons : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return t.TryGetComp<Comp_MateriaComp>() != null;
        }

        public override bool AlwaysMatches(ThingDef def)
        {
            return def.HasComp(typeof(Comp_MateriaComp));
        }
    }
}
