﻿using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using Verse;

namespace Outposts
{
    public partial class Outpost
    {
        public override MapGeneratorDef MapGeneratorDef
        {
            get
            {
                if (def.GetModExtension<CustomGenOption>() is { } cGen && (cGen.chooseFromlayouts.Count > 0 || cGen.chooseFromSettlements.Count > 0))
                    return DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                return MapGeneratorDefOf.Base_Faction;
            }
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

            foreach (var pawn in Map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike).ToList()) pawn.Destroy();

            foreach (var occupant in occupants) GenPlace.TryPlaceThing(occupant, Map.Center, Map, ThingPlaceMode.Near);
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (!Map.mapPawns.FreeColonists.Any())
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.Lost.Label".Translate(), "Outposts.Letters.Lost.Text".Translate(Name),
                    LetterDefOf.NegativeEvent);
                alsoRemoveWorldObject = true;
                return true;
            }
            List<Pawn> pawns = Map.mapPawns.AllPawns.ToList();
            if (!pawns.Any(p => p.HostileTo(Faction.OfPlayer) && p.RaceProps.Humanlike))
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.BattleWon.Label".Translate(), "Outposts.Letters.BattleWon.Text".Translate(Name),
                    LetterDefOf.PositiveEvent,
                    new LookTargets(Gen.YieldSingle(this)));
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];
                    if (pawn.Faction is { IsPlayer: true } || pawn.HostFaction is { IsPlayer: true })
                    {
                        pawn.DeSpawn();
                        occupants.Add(pawn);
                    }
                }
                RecachePawnTraits();
                alsoRemoveWorldObject = false;
                return true;
            }

            alsoRemoveWorldObject = false;
            return false;
        }
    }
}