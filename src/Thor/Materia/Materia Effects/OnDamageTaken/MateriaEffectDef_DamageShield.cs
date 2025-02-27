using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_DamageShield : MateriaEffectDef
    {
        public float shieldMaxCapacity = 100f;
        public float shieldRechargeRate = 0f;
        public float rechargeCooldownTime = 0f;
        public DamageDef damageType;
        public bool isModifier = false;
        public float absorbPercentage = 0.5f;
        public bool showShieldBar = true;
        public bool playAbsorbSound = true;
        public SoundDef absorbSound;

        public MateriaEffectDef_DamageShield()
        {
            workerClass = typeof(MateriaEffect_DamageShield);
        }
    }

    public class MateriaEffect_DamageShield : MateriaWorker
    {
        public float currentShieldHP;
        public int lastDamageAbsorbedTick = -9999;

       public  MateriaEffectDef_DamageShield Def => (MateriaEffectDef_DamageShield)def;

        public override void Notify_MateriaEquipped()
        {
            base.Notify_MateriaEquipped();

            currentShieldHP = Def.shieldMaxCapacity;
        }

        public override void Notify_MateriaUnequipped()
        {
            base.Notify_MateriaUnequipped();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentShieldHP, "shieldHP", Def.shieldMaxCapacity);
            Scribe_Values.Look(ref lastDamageAbsorbedTick, "lastDamageTick", -9999);
        }

        public override bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            bool wasAbsorbed = base.Notify_PostPreApplyDamage(ref dinfo);

            if (currentShieldHP > 0f && (Def.damageType == null || Def.damageType == dinfo.Def))
            {
                if (CalculateAbsorbption(ref dinfo))
                {
                    wasAbsorbed = true;
                    if (this.EquippingPawn != null)
                    {
                        FleckMaker.Static(EquippingPawn.DrawPos, EquippingPawn.Map, FleckDefOf.ExplosionFlash, 1f);
                    }                   
                }

                if (currentShieldHP <= 0)
                {
                    OnShieldDelepted();
                }
            }

            return wasAbsorbed;
        }

        private bool CalculateAbsorbption(ref DamageInfo originalDamage)
        {
            float amountToAbsorb;

            if (Def.isModifier)
            {
                amountToAbsorb = originalDamage.Amount * Def.absorbPercentage;
            }
            else
            {
                amountToAbsorb = Mathf.Min(originalDamage.Amount, currentShieldHP);
            }

            amountToAbsorb = Mathf.Min(amountToAbsorb, currentShieldHP);

            currentShieldHP -= amountToAbsorb;
            MoteMaker.ThrowText(EquippingPawn.DrawPos, EquippingPawn.Map, $"{amountToAbsorb} absorbed.");

            float remainingDamage = originalDamage.Amount - amountToAbsorb;
            originalDamage.SetAmount(remainingDamage);

            lastDamageAbsorbedTick = Find.TickManager.TicksGame;

            return remainingDamage <= 0;
        }

        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);

            //if (Def.shieldRechargeRate > 0f && currentShieldHP < Def.shieldMaxCapacity)
            //{
            //    if (Find.TickManager.TicksGame >= lastDamageAbsorbedTick + Def.rechargeCooldownTime)
            //    {
            //        currentShieldHP = Mathf.Min(
            //            currentShieldHP + Def.shieldRechargeRate,
            //            Def.shieldMaxCapacity
            //        );
            //    }
            //}
        }


        private void OnShieldDelepted()
        {
            Messages.Message($"{this.MateriaSlot.SlottedMateria.def.GetColouredLabel()} has shattered after expending its energy!", MessageTypeDefOf.NegativeEvent);
            DestroyParentMateria();
        }

        public string GetShieldStatus()
        {
            float percentage = currentShieldHP / Def.shieldMaxCapacity * 100f;
            return $"Shield: {currentShieldHP:F1}/{Def.shieldMaxCapacity:F0} ({percentage:F1}%)";
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (Def.showShieldBar && currentShieldHP > 0)
            {
                yield return new Gizmo_MateriaShieldStatus
                {
                    shield = this
                };
            }
        }
    }
}