using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public static class MateriaDisplayFactory
    {
        private static Dictionary<Type, MateriaDisplayWorker> availableDisplays = new Dictionary<Type, MateriaDisplayWorker>();

        public static void RegisterDisplays()
        {
            foreach (var displayDef in DefDatabase<MateriaDisplayDef>.AllDefs)
            {
                try
                {
                    var worker = displayDef.CreateWorker();
                    if (worker != null)
                    {
                        availableDisplays[displayDef.displayClass.GetType()] = worker;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error registering materia display {displayDef.defName}: {ex}");
                }
            }

            if (!availableDisplays.Any())
            {
                availableDisplays[typeof(GridMateriaDisplay)] = new GridMateriaDisplay();
            }
        }

        public static MateriaDisplayWorker GetBestDisplayForSlots(List<MateriaSlot> slots)
        {
            if (availableDisplays.Count == 0)
            {
                RegisterDisplays();
            }

            MateriaDisplayWorker bestDisplay = null;
            float bestSuitability = -1f;

            foreach (var display in availableDisplays.Values)
            {
                float suitability = display.GetLayoutSuitability(slots);
                if (suitability > bestSuitability)
                {
                    bestSuitability = suitability;
                    bestDisplay = display;
                }
            }

            MateriaDisplayWorker chosen = bestDisplay ?? new GridMateriaDisplay(); ;

            Log.Message($"{chosen} was chosen as display type");
            return chosen;
        }

        public static MateriaDisplayWorker GetDisplay(Type displayName)
        {
            if (availableDisplays.Count == 0)
            {
                RegisterDisplays();
            }

            if (availableDisplays.TryGetValue(displayName, out var display))
            {
                return display;
            }

            Log.Warning($"Materia display '{displayName}' not found, using default");
            return new GridMateriaDisplay();
        }
    }
}
