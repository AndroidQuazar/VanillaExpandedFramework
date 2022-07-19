using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace Outposts
{
    public class IncidentWorker_OutpostAttacked : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms) =>
            Find.WorldObjects.AllWorldObjects.Any(wo => wo is Outpost {Faction: {IsPlayer: true}}) && OutpostsMod.Settings.DoRaids;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!Find.WorldObjects.AllWorldObjects.OfType<Outpost>().TryRandomElement(out var target)) return false;
            LongEventHandler.QueueLongEvent(() =>
            {
                parms.target = GetOrGenerateMapUtility.GetOrGenerateMap(target.Tile, new IntVec3(150, 1, 150), target.def);
                target.Debug(parms,.35f,.35f);
                parms.points = target.ResolveRaidPoints(parms);
                TryGenerateRaidInfo(parms, out var pawns);
                target.raidFaction = parms.faction;
                target.raidPoints = parms.points;
                TaggedString baseLetterLabel = GetLetterLabel(parms);
                TaggedString baseLetterText = GetLetterText(parms, pawns);
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref baseLetterLabel, ref baseLetterText, GetRelatedPawnsInfoLetterText(parms), true);
                SendStandardLetter(baseLetterLabel, baseLetterText, GetLetterDef(), parms, SplitIntoGroups(parms, pawns), Array.Empty<NamedArgument>());
                if (parms.controllerPawn == null || parms.controllerPawn.Faction != Faction.OfPlayer) parms.raidStrategy.Worker.MakeLords(parms, pawns);
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            }, "GeneratingMapForNewEncounter", false, null);
            return true;
        }

        private List<TargetInfo> SplitIntoGroups(IncidentParms parms, List<Pawn> pawns)
        {
            var result = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                var groups = IncidentParmsUtility.SplitIntoGroups(pawns, parms.pawnGroups);
                var biggest = groups.MaxBy(x => x.Count);
                if (biggest.Any()) result.Add(biggest[0]);

                result.AddRange(groups.Where(group => group != biggest && group.Any()).Select(group => (TargetInfo) group[0]));
            }
            else if (pawns.Any()) result.AddRange(pawns.Select(t => (TargetInfo) t));

            return result;
        }
        protected override void GenerateRaidLoot(IncidentParms parms, float raidLootPoints, List<Pawn> pawns)
        {
            //Dont generate loot because we will pass it out later
            return;
        }
        //Below is me working out what I wanted to do for points. Leaving for a commit in case its at all helpful
        //These are from QuestNode_Root_Mission_AncientComplex because I know that does a raid under similar parameters.        
        //It works on DefaultThreatPointsNow (X), Colony Wealth Total (Y), Pawns (Z)
        //Then * the number by the random range
        //(b(x) * c(y) * d(z)) * A
        //Whats done in that quest is going to be way too tough for outposts. Going to use a combination of what Hack_AncientComplex and that one
        //Time Detection raids does flat local wealth *2.5 but those raids also feel pointless so I want it harder then that. Since local wealth is always really low
        //Doing (b(x) * d(z))* A
        //A is reduced heavily compared to quest
        //This will be something I can only get a real feel for once I play with it
        /*   private static readonly FloatRange RandomRaidPointsFactorRange = new FloatRange(0.15f, 0.25f);//A

           protected static readonly SimpleCurve ThreatPointsOverPointsCurve = new SimpleCurve//B
           {
               {
                   new CurvePoint(35f, 38.5f),
                   true
               },
               {
                   new CurvePoint(400f, 165f),
                   true
               },
               {
                   new CurvePoint(10000f, 4125f),
                   true
               }
           };
           private static SimpleCurve ThreatPointsFactorOverPawnCountCurve = new SimpleCurve//D
           {
               {
                   new CurvePoint(1f, 0.5f),
                   true
               },
               {
                   new CurvePoint(2f, 0.55f),
                   true
               },
               {
                   new CurvePoint(5f, 0.75f),
                   true
               },
               {
                   new CurvePoint(20f, 2f),
                   true
               }
           };
           private static SimpleCurve ThreatPointsFactorOverColonyWealthCurve = new SimpleCurve//C
           {
               {
                   new CurvePoint(10000f, 0.5f),
                   true
               },
               {
                   new CurvePoint(100000f, 1f),
                   true
               },
               {
                   new CurvePoint(1000000f, 1.5f),
                   true
               }
           };*/

    }
}