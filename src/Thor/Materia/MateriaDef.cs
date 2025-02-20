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
    public class MateriaCategoryDef : Def
    {
      
    }

    public class MateriaLinkData
    {
        public MateriaTypeDef type;
        public MateriaDef specificMateria;
    }
}
