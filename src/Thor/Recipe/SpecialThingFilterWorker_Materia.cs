using Verse;

namespace Thor
{
    public class SpecialThingFilterWorker_Materia : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return t.def != null && t.def is MateriaDef;
        }

    }
}
