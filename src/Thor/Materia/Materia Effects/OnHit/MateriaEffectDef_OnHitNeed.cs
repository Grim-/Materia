using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnHitNeed : MateriaEffectDef_OnHitBase
    {
        public float needOffset = 0.1f;
        public NeedDef needDef;
        public bool affectAttacker = false;

        public MateriaEffectDef_OnHitNeed()
        {
            workerClass = typeof(MateriaEffect_OnHitNeed);
        }

        public override string EffectDescription =>
            $"{(affectAttacker ? "User gains" : "Target loses")} {needOffset} {needDef.LabelCap} on hit.";
    }

    public class MateriaEffect_OnHitNeed : MateriaWorker
    {
        MateriaEffectDef_OnHitNeed Def => (MateriaEffectDef_OnHitNeed)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, DamageWorker.DamageResult damageResult)
        {
            Pawn targetPawn = Def.affectAttacker ? attacker : target.Pawn;


            if (Def.hitMode == OnHitMode.Melee)
            {
                if (targetPawn != null)
                {
                    Need need = targetPawn.needs.TryGetNeed(Def.needDef);
                    if (need != null)
                    {
                        need.CurLevel += Def.needOffset;

                        string effectText = $"{(Def.needOffset >= 0 ? "+" : "")}{Def.needOffset} {Def.needDef.LabelCap}";
                        Color textColor = Def.needOffset >= 0 ? Color.green : Color.red;
                        MoteMaker.ThrowText(targetPawn.DrawPos, targetPawn.Map, effectText, textColor);
                    }
                }
            }
            return damageResult;
        }

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Def.hitMode == OnHitMode.Range)
            {
                if (Attacker != null && Target != null && Target is Pawn TargetPawn)
                {
                    Pawn targetPawn = Def.affectAttacker ? Attacker : TargetPawn;

                    if (targetPawn != null)
                    {
                        Need need = targetPawn.needs.TryGetNeed(Def.needDef);
                        if (need != null)
                        {
                            need.CurLevel += Def.needOffset;

                            string effectText = $"{(Def.needOffset >= 0 ? "+" : "")}{Def.needOffset} {Def.needDef.LabelCap}";
                            Color textColor = Def.needOffset >= 0 ? Color.green : Color.red;
                            MoteMaker.ThrowText(targetPawn.DrawPos, targetPawn.Map, effectText, textColor);
                        }
                    }
                }
            }
            return damage;
        }
    }


}