using System.Collections.Generic;
using Verse;

namespace Thor
{
    public static class MateriaUtil
    {
        public static List<Thing> GenerateBreakDownMaterials(this MateriaDef MateriaDef, IntVec3 Position, Map map)
        {
            List<Thing> newThings = new List<Thing>();

            if (MateriaDef.breakdownMaterials == null)
            {
                return newThings;
            }
            foreach (var item in MateriaDef.breakdownMaterials)
            {
                for (int i = 0; i < item.amount.RandomInRange; i++)
                {
                    Thing thing = ThingMaker.MakeThing(item.thing);
                    GenSpawn.Spawn(thing, Position, map);
                    newThings.Add(thing);
                }
            }
            return newThings;
        }
    }


}
