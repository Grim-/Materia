using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaSetBonus : IExposable
    {
        public string label;
        public string description;
        public List<MateriaSetBonusRequirement> requirements = new List<MateriaSetBonusRequirement>();
        public List<MateriaEffectDef> bonusEffects = new List<MateriaEffectDef>();
        public bool isActive = false;

        // Cached active effects
        private List<MateriaWorker> activeEffects = new List<MateriaWorker>();

        public List<MateriaWorker> ActiveEffects => activeEffects;

        public MateriaSetBonus()
        {
        }

        public void InitializeEffects(ThingWithComps parent, Comp_MateriaSetBonus comp)
        {
            activeEffects.Clear();

            foreach (var effectDef in bonusEffects)
            {
                var worker = effectDef.CreateWorker(parent, null, null);
                activeEffects.Add(worker);
            }
        }

        public bool CheckRequirements(Pawn pawn)
        {
            if (pawn == null)
                return false;

            // Get all Comp_Materia components on the pawn's equipment and apparel
            List<Comp_Materia> materiaComps = new List<Comp_Materia>();

            if (pawn.equipment?.AllEquipmentListForReading != null)
            {
                foreach (var eq in pawn.equipment.AllEquipmentListForReading)
                {
                    var comp = eq.GetComp<Comp_Materia>();
                    if (comp != null)
                        materiaComps.Add(comp);
                }
            }

            if (pawn.apparel?.WornApparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    var comp = apparel.GetComp<Comp_Materia>();
                    if (comp != null)
                        materiaComps.Add(comp);
                }
            }

            // Count all equipped materia by type
            Dictionary<MateriaTypeDef, int> materiaTypeCounts = new Dictionary<MateriaTypeDef, int>();
            Dictionary<MateriaDef, int> specificMateriaCounts = new Dictionary<MateriaDef, int>();

            foreach (var comp in materiaComps)
            {
                foreach (var slot in comp.MateriaSlots)
                {
                    if (slot.IsOccupied && slot.SlottedMateria != null)
                    {
                        // Count by type
                        var materiaType = slot.SlottedMateria.def.SlotType;
                        if (materiaType != null)
                        {
                            if (!materiaTypeCounts.ContainsKey(materiaType))
                                materiaTypeCounts[materiaType] = 0;
                            materiaTypeCounts[materiaType]++;
                        }

                        // Count specific materia
                        var materiaDef = slot.SlottedMateria.def;
                        if (!specificMateriaCounts.ContainsKey(materiaDef))
                            specificMateriaCounts[materiaDef] = 0;
                        specificMateriaCounts[materiaDef]++;
                    }
                }
            }

            // Check all requirements
            foreach (var req in requirements)
            {
                // Type-based requirement
                if (req.materiaType != null)
                {
                    if (!materiaTypeCounts.ContainsKey(req.materiaType) ||
                        materiaTypeCounts[req.materiaType] < req.requiredCount)
                        return false;
                }

                // Specific materia requirement
                if (req.specificMateria != null && req.specificMateria.Any())
                {
                    int foundCount = 0;
                    foreach (var materia in req.specificMateria)
                    {
                        if (specificMateriaCounts.ContainsKey(materia))
                            foundCount += specificMateriaCounts[materia];
                    }

                    if (foundCount < req.requiredCount)
                        return false;
                }
            }

            return true;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref isActive, "isActive", false);
            Scribe_Collections.Look(ref requirements, "requirements", LookMode.Deep);
            Scribe_Collections.Look(ref bonusEffects, "bonusEffects", LookMode.Def);
            Scribe_Collections.Look(ref activeEffects, "activeEffects", LookMode.Deep);
        }
    }

    public class MateriaSetBonusRequirement
    {
        public MateriaTypeDef materiaType;
        public int requiredCount;
        public List<MateriaDef> specificMateria; // Optional: for requiring specific materia instead of just types

        public MateriaSetBonusRequirement()
        {
        }

        public MateriaSetBonusRequirement(MateriaTypeDef type, int count)
        {
            materiaType = type;
            requiredCount = count;
        }
    }
}
