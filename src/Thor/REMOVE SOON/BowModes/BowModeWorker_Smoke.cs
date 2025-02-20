using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public class BowModeWorker_Smoke : BowModeWorker
    {
        private int cooldownTicks = 0;
        private const int MaxCooldown = 2400;

        public bool IsOnCooldown => cooldownTicks > 0;

        private Texture2D _GizmoIcon;
        private Texture2D GizmoIcon
        {
            get
            {
                if (_GizmoIcon == null)
                {
                    _GizmoIcon = ContentFinder<Texture2D>.Get("Things/Item/Equipment/WeaponRanged/Grenades");
                }
                return _GizmoIcon;
            }
        }

        public override void Tick()
        {
            if (cooldownTicks > 0)
                cooldownTicks--;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref cooldownTicks, "smokeCooldownTicks", 0);
        }

        public override void OnGUI(Gizmo_BowModeSelector parentGizmo, float parentWidth, Rect slotRect)
        {
            //base.OnGUI(parentGizmo, parentWidth, slotRect);
            if (!IsOnCooldown)
            {
                if (Widgets.ButtonImage(slotRect.LeftPartPixels(15f), GizmoIcon))
                {
                    TryDeploySmoke();
                }
            }
            else
            {
                Widgets.FillableBar(slotRect.ContractedBy(2), cooldownTicks / MaxCooldown);
            }
        }

        private void TryDeploySmoke()
        {
            if (cooldownTicks > 0) 
                return;

            Pawn wielder = parentComp.EquippedPawn;
            if (wielder == null) 
                return;

            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetLocations = true,
                canTargetSelf = false,
                canTargetPawns = false,
                validator = (TargetInfo targ) => targ.Cell.InBounds(parentComp.EquippedPawn.Map) &&
                            (parentComp.parent.Position - targ.Cell).LengthHorizontal <= 30f
            },
            (LocalTargetInfo target) =>
            {
                DoDeploySmoke(target.Cell, parentComp.EquippedPawn.Map);
                cooldownTicks = MaxCooldown;
            }, null, null, null);
        }

        private void DoDeploySmoke(IntVec3 cell, Map map)
        {
            GenExplosion.DoExplosion(cell, map, 3f, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, new GasType?(GasType.BlindSmoke), false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f, null, null);
        }
    }
}
