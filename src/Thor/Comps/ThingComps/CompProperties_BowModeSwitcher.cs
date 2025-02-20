using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    public class CompProperties_BowModeSwitcher : CompProperties
    {
        public BowModeDef defaultModeDef;
        public List<BowModeDef> allowedModeDefs = new List<BowModeDef>();

        public CompProperties_BowModeSwitcher()
        {
            compClass = typeof(CompEquippable_BowModeSwitcher);
        }
    }

    public class CompEquippable_BowModeSwitcher : CompEquippable, IDrawEquippedGizmos
    {
        private BowModeWorker currentWorker;
        private BowModeDef currentModeDef;
        private Verb currentModeVerb;

        public BowModeWorker CurrentWorker => currentWorker;


        private List<BowModeWorker> modeWorkers = new List<BowModeWorker>();

        public BowModeDef CurrentModeDef => currentModeDef;
        public CompProperties_BowModeSwitcher Props => (CompProperties_BowModeSwitcher)props;

        public Pawn EquippedPawn => this.ParentHolder.ParentHolder as Pawn;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                InitModeWorkers();

                if (currentModeDef == null)
                {
                    currentModeDef = Props.defaultModeDef;
                }

                ApplyBowMode(currentModeDef);
            }
        }


        private void InitModeWorkers()
        {
            modeWorkers.Clear();
            foreach (var modeDef in Props.allowedModeDefs)
            {
                var worker = modeDef.GetModeWorker(this);
                modeWorkers.Add(worker);
            }
        }

        public void ApplyBowMode(BowModeDef modeDef)
        {
            if (Props.allowedModeDefs.Contains(modeDef))
            {
                currentModeDef = modeDef;
                currentModeVerb = this.AllVerbs.Find(
                    x => x.GetType() == currentModeDef.verbClass);
                currentWorker = modeWorkers.Find(x => x.modeDef == currentModeDef);

                UpdateVerbProps();
            }
        }

        private void UpdateVerbProps()
        {
            foreach (Verb verb in this.AllVerbs)
            {
                if (verb.GetType() == currentModeDef.verbClass)
                {
                    if (currentModeDef.OverridesVerbAmmo())
                    {
                        verb.verbProps.defaultProjectile = currentModeDef.GetProjectileDef();
                    }
          
                    if (currentModeDef.warmupTime > 0)
                        verb.verbProps.warmupTime = currentModeDef.warmupTime;
                    if (currentModeDef.range > 0)
                        verb.verbProps.range = currentModeDef.range;
                    verb.verbProps.requireLineOfSight = currentModeDef.requireLineOfSight;
                }
            }
        }

        public IEnumerable<Gizmo> GetEquippedGizmos()
        {
            if (this.currentModeVerb != null)
            {
                yield return new Gizmo_BowModeSelector(this, currentModeVerb);

                if (currentWorker != null)
                {
                    foreach (var gizmo in currentWorker.GetGizmos())
                    {
                        yield return gizmo;
                    }
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();


            foreach (var worker in modeWorkers)
            {
                worker.OnDraw();
            }

        }


        public virtual void EquipTick()
        {
            foreach (var worker in modeWorkers)
            {
                worker.Tick();
            }
        }
        

        public override void CompTick()
        {
            base.CompTick();

    
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Defs.Look(ref currentModeDef, "currentModeDef");
            Scribe_Collections.Look(ref modeWorkers, "modeWorkers", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (currentModeDef == null && Props != null)
                {
                    currentModeDef = Props.defaultModeDef;
                }

                if (modeWorkers.Count == 0)
                {
                    InitModeWorkers();
                } 
            }
        }
    }
}

