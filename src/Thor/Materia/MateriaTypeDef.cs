using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaTypeDef : Def
    {
        public Color materiaTextColor;

        //slot gen stuff
        public float baseWeight = 1f;
        public float rangedWeaponMultiplier = 1f;
        public float meleeWeaponMultiplier = 1f;


        public List<TechLevelWeight> techLevelWeights;
        public List<WeaponTagWeight> weaponTagWeights;

        public List<SlotLimit> slotLimits;
        public List<SlotLevelRarity> slotLevelRarities;


        //red materia type
        ///Weapon
        //2 slots
        //armour
        //0 slots
        //utility
        //0 slots

        //green
        ///Weapon
        //0 slots
        //armour
        //2 slots
        //utility
        //2 slots

        //blue materia type
        ///Weapon
        //1 slots
        //armour
        //2 slots
        //utility
        //2 slots


        //purple materia type
        ///Weapon
         //
        //1 slots
        //armour
        //2 slots
        //utility
        //3 slots
  
    }
}
