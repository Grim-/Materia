using RimWorld;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_OnMeleeHitNeed : MateriaEffectDef
    {
        public float needOffset = 0.1f;
        public NeedDef needDef;
        public bool affectAttacker = false;

        public MateriaEffectDef_OnMeleeHitNeed()
        {
            workerClass = typeof(MateriaEffect_OnMeleeHitNeed);
        }

        public override string EffectDescription =>
            $"{(affectAttacker ? "User gains" : "Target loses")} {needOffset} {needDef.LabelCap} on hit.";
    }

    public class MateriaEffect_OnMeleeHitNeed : MateriaWorker
    {
        MateriaEffectDef_OnMeleeHitNeed Def => (MateriaEffectDef_OnMeleeHitNeed)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, DamageWorker.DamageResult damageResult)
        {
            Pawn targetPawn = Def.affectAttacker ? attacker : target.Pawn;

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

            return damageResult;
        }
    }
}