using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Thor
{
    public class BowModeWorker_Beam : BowModeWorker
    {
        private int beamTicks = 0;
        private const int BeamDuration = 400; // Beam lasts for 2 seconds (assuming 60 ticks per second)
        private Pawn wielder;
        private IntVec3 targetCell;
        private Map map;

        //beamMoteDef Mote_GraserBeamBase
        //beamEndEffecterDef GraserBeam_End
        //beamLineFleckDef Fleck_BeamSpark
        private MoteDualAttached mote;
        public bool IsBeamActive => beamTicks > 0;

        public override void Tick()
        {
            if (IsBeamActive)
            {
                beamTicks--;
                if (this.mote != null)
                {

                    this.mote.UpdateTargets(new TargetInfo(parentComp.EquippedPawn.Position, parentComp.EquippedPawn.Map, false),
                        new TargetInfo(targetCell, parentComp.EquippedPawn.Map, false), Vector3.zero, Vector3.zero);
                    this.mote.Maintain();
                }

                if (beamTicks % 10 == 0) // Apply damage every 10 ticks
                {

                    FleckMaker.Static(targetCell.ToVector3Shifted(), parentComp.EquippedPawn.Map, DefDatabase<FleckDef>.GetNamed("Fleck_BeamSpark"), 1f);
                    ApplyBeamDamage();
                }

                if (beamTicks <= 0)
                {
                    CleanupBeam();
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref beamTicks, "beamTicks", 0);
        }

        public override void OnGUI(Gizmo_BowModeSelector parentGizmo, float parentWidth, Rect slotRect)
        {
            if (!IsBeamActive)
            {
                if (Widgets.ButtonText(slotRect, "Fire Beam"))
                {
                    TryFireBeam();
                }
            }
            else
            {
                Widgets.FillableBar(slotRect.ContractedBy(2), (float)beamTicks / (float)BeamDuration);
            }
        }

        private void TryFireBeam()
        {
            if (IsBeamActive)
                return;

            wielder = parentComp.EquippedPawn;
            if (wielder == null)
                return;

            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetLocations = true,
                canTargetPawns = true,
                validator = (TargetInfo targ) => targ.Cell.InBounds(wielder.Map) &&
                            (wielder.Position - targ.Cell).LengthHorizontal <= 20f
            },
            (LocalTargetInfo target) =>
            {
                StartBeam(target.Cell, wielder.Map);
            });
        }

        private void StartBeam(IntVec3 cell, Map currentMap)
        {
            targetCell = cell;
            map = currentMap;
            beamTicks = BeamDuration;

            this.mote = MoteMaker.MakeInteractionOverlay(DefDatabase<ThingDef>.GetNamed("Mote_GraserBeamBase"), 
                parentComp.EquippedPawn, 
                new TargetInfo(targetCell, parentComp.EquippedPawn.Map, false));


            parentComp.EquippedPawn.stances.stunner.StunFor(beamTicks, parentComp.EquippedPawn, false, false, false);
        }

        private void ApplyBeamDamage()
        {
            if (map == null || wielder == null)
                return;

            List<Thing> things = map.thingGrid.ThingsListAt(targetCell);
            foreach (Thing thing in things)
            {
                if (thing is Pawn targetPawn)
                {
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Burn, 10f, 0f, -1f, wielder);
                    targetPawn.TakeDamage(damageInfo);
                }
            }
        }

        private void CleanupBeam()
        {
            beamTicks = 0;
            targetCell = IntVec3.Invalid;
            map = null;

            if (mote != null)
            {
                mote.DeSpawn();
                mote = null;
            }
        }


    }
}
