using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaDef : ThingDef
    {
        public float commonality = 1f;
        public string texPath;
        //the slot type
        public MateriaTypeDef SlotType;
        //effects that equipping this materia to a weapon applies
        public List<MateriaEffectDef> effects;
        //list of valid materia that can be linked to this one
        public List<MateriaLinkBonus> links;

        public Color textColor = Color.white;
        public List<MateriaBreakdownMaterials> breakdownMaterials;

        public string GetColouredLabel()
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(SlotType.materiaTextColor)}>{label}</color>";
        }

        public bool CanLinkWith(MateriaDef other)
        {
            return other != this &&
                   this.links.Any(x => x.materia == other);
        }
    }


}
