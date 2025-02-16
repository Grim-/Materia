using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace Thor
{
    public class PawnFlyerDelegate : PawnFlyer
    {
        public Action<PawnFlyer> OnRespawn;

        protected override void RespawnPawn()
        {
            OnRespawn?.Invoke(this);

            base.RespawnPawn();
        }
    }
}
