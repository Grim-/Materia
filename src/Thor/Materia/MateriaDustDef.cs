using System.Collections.Generic;
using Verse;

namespace Materia
{
    public class MateriaDustDef : ThingDef
    {
        public MateriaSlotTypeDef createsSlotType;

        public List<MateriaMinableData> mineableData;
    }
}
