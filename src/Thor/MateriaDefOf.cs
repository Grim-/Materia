using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    [DefOf]
    public static class MateriaDefOf
    {
        public static ThingDef Materia_DelegateProjectile;

        public static EffecterDef WeaponFireEnchant;

        public static HediffDef Materia_BoundWeapon;


        public static ThingDef RedMateriaDust;
        public static ThingDef PurpleMateriaDust;
        public static ThingDef GreenMateriaDust;
        public static ThingDef BlueMateriaDust;

        public static SlotLinkRuleDef PairForward;
        public static MateriaCategoryDef MateriaCategory_Universal;
        public static MateriaCategoryDef MateriaCategory_Melee;
        public static MateriaCategoryDef MateriaCategory_Ranged;
        public static MateriaCategoryDef MateriaCategory_Armor;

        public static MateriaTypeDef RedMateria;
        public static MateriaTypeDef GreenMateria;
        public static MateriaTypeDef BlueMateria;
        public static MateriaTypeDef PurpleMateria;

        public static MateriaGenerationConfigDef MateriaGenerationConfig;
    }


}
