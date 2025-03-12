using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaTypeDef : Def
    {
        public Color materiaTextColor;
        public float baseWeight = 1f;

        public List<ThingDef> breakdownMaterials;
        public List<MateriaSlotTypeDef> disallowedSlotTypes;
    }
}
