using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Materia
{
    public class CompProperties_BoundPawn : CompProperties
    {
        public PawnKindDef summonedPawnKind;
        public float maxCharge = 100f;
        public float chargeUsagePerTick = 0.1f;
        public float chargeRegainPerTick = 0.05f;

        public CompProperties_BoundPawn()
        {
            compClass = typeof(Comp_BoundPawn);
        }
    }

    public class Comp_BoundPawn : ThingComp
    {
        private Pawn summonedPawn;
        private float currentCharge = 100f;
        private bool isSummoned;

        public CompProperties_BoundPawn Props => (CompProperties_BoundPawn)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentCharge = Props.maxCharge;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (isSummoned && summonedPawn != null && !summonedPawn.Dead)
            {
                currentCharge -= Props.chargeUsagePerTick;
                if (currentCharge <= 0f)
                {
                    UnsummonPawn();
                }
            }
            else if (!isSummoned && currentCharge < Props.maxCharge)
            {
                currentCharge = Mathf.Min(Props.maxCharge, currentCharge + Props.chargeRegainPerTick);
            }
        }

        private void SummonPawn()
        {
            if (summonedPawn != null && !summonedPawn.Dead || Props.summonedPawnKind == null)
                return;

            PawnGenerationRequest request = new PawnGenerationRequest(
                Props.summonedPawnKind,
                Faction.OfPlayer,
                PawnGenerationContext.NonPlayer,
                -1,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: true,
                colonistRelationChanceFactor: 0f,
                forceAddFreeWarmLayerIfNeeded: false);

            summonedPawn = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(summonedPawn, parent.Position, parent.Map);
            isSummoned = true;
        }

        private void UnsummonPawn()
        {
            if (summonedPawn != null && !summonedPawn.Dead)
            {
                summonedPawn.Destroy();
                summonedPawn = null;
            }
            isSummoned = false;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Summon/Unsummon toggle button
            Command_Action summonGizmo = new Command_Action
            {
                defaultLabel = isSummoned ? "Unsummon" : "Summon",
                defaultDesc = isSummoned ? "Dismiss the summoned pawn." : "Summon a pawn using charge energy.",
                icon = TexButton.Banish,
                action = delegate
                {
                    if (!isSummoned && currentCharge > 0f)
                    {
                        SummonPawn();
                    }
                    else if (isSummoned)
                    {
                        UnsummonPawn();
                    }
                }
            };

            if (!isSummoned && currentCharge <= 0f)
            {
                summonGizmo.Disable("Insufficient charge");
            }

            yield return summonGizmo;

            // Charge meter gizmo
            yield return new Gizmo_ChargeStatus(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentCharge, "currentCharge", 0f);
            Scribe_Values.Look(ref isSummoned, "isSummoned", false);
            Scribe_References.Look(ref summonedPawn, "summonedPawn");
        }


        public float CurrentCharge => currentCharge;
        public float MaxCharge => Props.maxCharge;
    }




}
