using System.Linq;
using System.Collections.Generic;
using KCSG;
using RimWorld;
using RimWorld.Planet;
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
        public virtual void AddLoot()
        {
            //looking at this if a colonist gets downed and dropped their weapon would they not lose their weapon?
            //Also can get raider's weapon
            foreach (Thing thing in Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon))
            {
                if (!containedItems.Contains(thing) && !thing.Position.Fogged(Map)) //In case ancient dangers are possible in these maps
                {
                    containedItems.Add(thing);
                }
            }
            //I wanted loot
            if (raidFaction.def.raidLootMaker != null)
            {
                float raidLootPoints = raidPoints * Find.Storyteller.difficulty.EffectiveRaidLootPointsFactor;
                float num = raidFaction.def.raidLootValueFromPointsCurve.Evaluate(raidLootPoints);
                ThingSetMakerParams parms2 = default(ThingSetMakerParams);
                parms2.totalMarketValueRange = new FloatRange(num, num);
                parms2.makingFaction = raidFaction;
                List<Thing> loot = raidFaction.def.raidLootMaker.root.Generate(parms2);
                foreach (Thing thing in loot)
                {
                    AddItem(thing);
                }
            }
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

            var pawns = Map.mapPawns.AllPawns.ListFullCopy();
            if (!pawns.Any(p => p.Faction == raidFaction && !p.Downed)) //Tweak so that random ancients that spawn dont prevent it. (I suspect this is also why raids dont end sometimes some invisible or fogged enemy)
            {
                occupants.Clear();
                Find.LetterStack.ReceiveLetter("Outposts.Letters.BattleWon.Label".Translate(), "Outposts.Letters.BattleWon.Text".Translate(Name),
                    LetterDefOf.PositiveEvent,
                    new LookTargets(Gen.YieldSingle(this)));
                foreach (var pawn in pawns)
                {
                    if (pawn.Faction is { IsPlayer: true } || pawn.HostFaction is { IsPlayer: true })
                    {
                        pawn.DeSpawn();
                        occupants.Add(pawn);
                    }
                    if (pawn.Faction == raidFaction)//Random chance to capture
                    {
                        if (Rand.Chance(0.33f))
                        {
                            pawn.DeSpawn();
                            occupants.Add(pawn);
                        }
                    }
                }
                AddLoot();
                RecachePawnTraits();
                raidFaction = null;
                raidPoints = 0;
                alsoRemoveWorldObject = false;
                return true;
            }

            alsoRemoveWorldObject = false;
            return false;
        }
    }
}