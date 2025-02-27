using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaDef : ThingDef
    {
        public float commonality = 1f;

        public string texPath;

        public int slotLevel = 1;

        public MateriaTypeDef SlotType;
        public List<MateriaTypeDef> slotSecondaryTypes;

        public List<MateriaEffectDef> effects;

        public List<MateriaLinkData> links;


        public List<MateriaCategoryDef> materiaCategories;

        public Color textColor = Color.white;
        public List<MateriaBreakdownMaterials> breakdownMaterials;

        private Texture2D _IconTex;
        public Texture2D IconTex
        {
            get
            {
                if (_IconTex == null)
                {
                    _IconTex = ContentFinder<Texture2D>.Get(texPath);
                }

                return _IconTex;
            }
        }

        public string GetColouredLabel()
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(SlotType.materiaTextColor)}>{label}</color>";
        }

        public bool CanLinkWith(MateriaDef other)
        {
            if (other == this)
            {
                return false;
            }

            if (this.links == null || this.links.Count == 0)
            {
                return true;
            }

            var specificMatch = this.links.Any(x => x.specificMateria != null && x.specificMateria == other);
            if (specificMatch)
            {
                return true;
            }

            return this.links.Any(x => x.specificMateria == null &&
                                      x.type != null &&
                                      x.type == other.SlotType);
        }
    }


    public class MateriaSlotTypeDef : Def
    {
        public List<MateriaTypeDef> acceptableMateriaTypes;
        public List<SlotLimit> slotLimits;


        public bool IsValidForSlotCreation(ThingWithComps parent, Comp_Materia comp)
        {
            if (slotLimits == null)
                return true;

            foreach (var limit in slotLimits)
            {
                bool matchesCategory = HasThingCategoryOrParent(parent, limit.ThingCategory);
                if (matchesCategory)
                {
                    if (limit.maxSlots == 0)
                    {
                        return false;
                    }

                    if (comp.OccupiedSlotsCount >= limit.maxSlots)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool HasThingCategoryOrParent(Thing thing, ThingCategoryDef categoryToCheck)
        {
            if (thing.HasThingCategory(categoryToCheck))
                return true;

            foreach (ThingCategoryDef parentCategory in categoryToCheck.Parents)
            {
                if (thing.HasThingCategory(parentCategory))
                    return true;
            }

            foreach (ThingCategoryDef childCategory in categoryToCheck.ThisAndChildCategoryDefs)
            {
                if (thing.HasThingCategory(childCategory))
                    return true;
            }

            return false;
        }

        public bool IsValidForSlot(MateriaDef MateriaDef)
        {
            if (acceptableMateriaTypes.Any(x => x == MateriaDef.SlotType))
            {
                return true;
            }

            if (MateriaDef.slotSecondaryTypes != null && MateriaDef.slotSecondaryTypes.Any())
            {
                return MateriaDef.slotSecondaryTypes.Any(secondaryType =>
                    acceptableMateriaTypes.Any(acceptableType => acceptableType == secondaryType));
            }

            return false;
        }
    }

    public class MateriaLinkData
    {
        public MateriaTypeDef type;
        public MateriaDef specificMateria;
    }
}
