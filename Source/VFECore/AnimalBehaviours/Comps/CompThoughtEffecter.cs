﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Collections;

namespace AnimalBehaviours
{
    public class CompThoughtEffecter : ThingComp
    {
        public int tickCounter = 0;
        public List<Pawn> pawnList = new List<Pawn>();
        public Pawn thisPawn;

        public CompProperties_ThoughtEffecter Props
        {
            get
            {
                return (CompProperties_ThoughtEffecter)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            //Only do anything every tickInterval
            if (tickCounter > Props.tickInterval)
            {
                thisPawn = this.parent as Pawn;
                //Null map check. Also will only work if pawn is not dead or downed
                if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed)
                {
                    foreach (Thing thing in GenRadial.RadialDistinctThingsAround(thisPawn.Position, thisPawn.Map, Props.radius, true))
                    {
                        Pawn pawn = thing as Pawn;
                        //It won't affect animals, cause they don't have Thoughts, or mechanoids, or itself
                        if (pawn != null && !pawn.AnimalOrWildMan() && pawn.RaceProps.IsFlesh && pawn != this.parent)
                        {
                            //Only work on not dead, not downed, not psychically immune pawns
                            if (!pawn.Dead && !pawn.Downed && pawn.GetStatValue(StatDefOf.PsychicSensitivity, true) > 0f)
                            {
                                //Only show an effect if the user wants it to, or it gets obnoxious
                                if (Props.showEffect)
                                {
                                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                                    MoteMaker.MakeAttachedOverlay(this.parent, ThingDef.Named("Mote_PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                                }
                                //Apply thought
                                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named(Props.thoughtDef), null);
                            }
                        }
                    }
                }
                tickCounter = 0;
            }
        }
    }
}


