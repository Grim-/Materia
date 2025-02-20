using System;
using UnityEngine;
using Verse;

namespace Materia
{
    public class BowModeDef : Def
    {
        public ThingDef projectileDef;

        public float range = 30f;
        public float warmupTime = 0f;
        public bool requireLineOfSight = true;
        public Type verbClass = null;

        public Type modeWorkerClass = typeof(BowModeWorker);

        public bool OverridesVerbAmmo()
        {
            return projectileDef != null;
        }

        public ThingDef GetProjectileDef()
        {
            if (projectileDef != null)
            {
                return projectileDef;
            }

            return null;
        }
        public BowModeWorker GetModeWorker(CompEquippable_BowModeSwitcher bowModeSwitcher)
        {
            var worker = (BowModeWorker)Activator.CreateInstance(modeWorkerClass);
            worker.modeDef = this;
            worker.parentComp = bowModeSwitcher;
            return worker;
        }
    }
}
