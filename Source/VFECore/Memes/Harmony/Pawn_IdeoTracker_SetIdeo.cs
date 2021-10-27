﻿using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;



namespace VanillaMemesExpanded
{


    [HarmonyPatch(typeof(Pawn_IdeoTracker))]
    [HarmonyPatch("SetIdeo")]
    public static class VanillaMemesExpanded_Pawn_IdeoTracker_SetIdeo_Patch
    {
        [HarmonyPostfix]
        static void ForceTrait(Ideo ideo, Pawn ___pawn)
        {
            
            foreach(MemeDef meme in ideo?.memes)
            {
                ExtendedMemeProperties extendedMemeProps = meme.GetModExtension<ExtendedMemeProperties>();
                if(extendedMemeProps != null)
                {
                    if (extendedMemeProps.forcedTrait != null)
                    {
                        Trait trait = new Trait(extendedMemeProps.forcedTrait, 0, true);
                        ___pawn.story.traits.GainTrait(trait);
                    }
                    if (extendedMemeProps.abilitiesGiven != null)
                    {
                       foreach(AbilityDef ability in extendedMemeProps.abilitiesGiven)
                        {
                            ___pawn.abilities.GainAbility(ability);
                        }
                    }
                }
                

            }
            
            
            







        }
    }








}