﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{

    public class DamageWorker_ExtraInfecter : DamageWorker_Cut
    {

        //A damage class that causes additional infections when causing damage. The percentage chance
        //is passed by adding a comp class, CompInfecter, to the animal using this damage class

        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            Random random = new Random();

            if (random.NextDouble() > ((float)(100 - dinfo.Instigator.TryGetComp<CompInfecter>().GetChance) / 100))
            {
                pawn.health.AddHediff(HediffDefOf.WoundInfection, dinfo.HitPart, null, null);
            }

        }
    }
}

