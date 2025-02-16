using RimWorld;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_OnProjectileHitNeed : MateriaEffectDef
    {
        public float needOffset = 0.1f;
        public NeedDef needDef;
        public bool affectAttacker = false;

        public MateriaEffectDef_OnProjectileHitNeed()
        {
            workerClass = typeof(MateriaEffect_OnProjectileHitNeed);
        }

        public override string EffectDescription =>
            $"{(affectAttacker ? "User gains" : "Target loses")} {needOffset} {needDef.LabelCap} on hit.";
    }

    public class MateriaEffect_OnProjectileHitNeed : MateriaWorker
    {
        MateriaEffectDef_OnProjectileHitNeed Def => (MateriaEffectDef_OnProjectileHitNeed)def;

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

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
            return damage;
        }
    }
}