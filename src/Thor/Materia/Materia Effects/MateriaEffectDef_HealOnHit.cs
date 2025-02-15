using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_HealOnHit : MateriaEffectDef
    {
        public float healFactor = 0.2f;
        public float minimumDamageToHeal = 5f;
    }

    public class MateriaEffect_HealOnHit : MateriaWorker
    {
        MateriaEffectDef_HealOnHit Def => (MateriaEffectDef_HealOnHit)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt >= Def.minimumDamageToHeal && Attacker != null)
            {
                float healAmount = damageResult.totalDamageDealt * Def.healFactor;
                if (HealthUtility.TryGetWorstHealthCondition(Attacker, out Hediff hediff, out BodyPartRecord part, null))
                {
                    HealthUtility.AdjustSeverity(Attacker, hediff.def, healAmount);
                }

                MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, "+" + healAmount.ToString("F1"), Color.green);
            }
            return damageResult;
        }
    }
    public class MateriaEffectDef_OnHitBane : MateriaEffectDef
    {
        public float damage =1f;
        public DamageDef damageType;
        public ThingDef race;
    }

    public class MateriaEffect_OnHitBane : MateriaWorker
    {
        MateriaEffectDef_OnHitBane Def => (MateriaEffectDef_OnHitBane)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (target.Pawn != null && target.Pawn.def == Def.race)
            {
                float extraDamage = Def.damage;
                target.Thing.TakeDamage(new DamageInfo(Def.damageType, extraDamage));
                MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, $"Bane {Def.race.LabelCap} " + extraDamage.ToString("F1"), Color.red);
            }
            return damageResult;
        }
    }

    public class MateriaEffectDef_DamageTracker : MateriaEffectDef
    {
        public float damageToHealthRatio = 0.001f; 
        public float maxStored = 1000f;      
        public float lossPerDay = 50f;     
    }

    public class MateriaEffect_DamageTracker : MateriaWorker
    {
        private float storedDamage = 0f;
        private const int TicksPerProgressUpdate = 250;

        MateriaEffectDef_DamageTracker Def => (MateriaEffectDef_DamageTracker)def;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storedDamage, "storedDamage", 0f);
        }

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt > 0f)
            {
                float newDamage = Mathf.Min(storedDamage + damageResult.totalDamageDealt, Def.maxStored);
                if (newDamage != storedDamage)
                {
                    storedDamage = newDamage;
                    MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map,
                        "Damage Stored: " + storedDamage.ToString("F1"),
                        Color.yellow);
                }
            }
            return damageResult;
        }

        public override void OnTick(Pawn pawn)
        {
            if (Find.TickManager.TicksGame % TicksPerProgressUpdate == 0)
            {
                float damageDecay = Def.lossPerDay * (TicksPerProgressUpdate / GenDate.TicksPerDay);
                storedDamage = Mathf.Max(0f, storedDamage - damageDecay);
            }
        }

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDefOf.MaxHitPoints)
            {
                return 1f + (storedDamage * Def.damageToHealthRatio);
            }
            return base.GetStatFactor(stat);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (EquippingPawn?.Faction == Faction.OfPlayer)
            {
                yield return new Gizmo_DamageTrackerStatus
                {
                    damageTracker = this,
                    maxDamage = Def.maxStored,
                    currentDamage = storedDamage
                };
            }
        }
    }

    public class Gizmo_DamageTrackerStatus : Gizmo
    {
        public MateriaEffect_DamageTracker damageTracker;
        public float maxDamage;
        public float currentDamage;

        private static readonly Vector2 BarSize = new Vector2(140f, 12f);
        private static readonly Color BarColor = new Color(0.9f, 0.85f, 0.2f);
        private static readonly Color BarBackgroundColor = new Color(0.15f, 0.15f, 0.15f);

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect barRect = new Rect(rect.x, rect.y + 32f, BarSize.x, BarSize.y);

            // Draw the background
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;

            // Draw label
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height),
                "Stored Damage: " + currentDamage.ToString("F1") + " / " + maxDamage.ToString("F0"));

            // Draw the bar
            Widgets.FillableBar(barRect, currentDamage / maxDamage);

            // Draw health bonus
            float healthBonus = ((MateriaEffectDef_DamageTracker)damageTracker.def).damageToHealthRatio * currentDamage * 100f;
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 50f, rect.width - 10f, rect.height),
                "Health Bonus: +" + healthBonus.ToString("F1") + "%");

            return new GizmoResult(GizmoState.Clear);
        }
    }
}