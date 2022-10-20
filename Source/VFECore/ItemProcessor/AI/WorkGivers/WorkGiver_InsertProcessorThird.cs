using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace ItemProcessor
{
    public class WorkGiver_InsertProcessorThird : WorkGiver_Scanner
    {
        private static string NoIngredientFound;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {

            return pawn.Map.GetComponent<ItemProcessor_MapComponent>().itemProcessors_InMap;


        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public static void ResetStaticData()
        {

            WorkGiver_InsertProcessorThird.NoIngredientFound = "IP_NoIngredientFound".Translate();
        }


        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_ItemProcessor building_processor = t as Building_ItemProcessor;
            if (building_processor == null || building_processor.GetComp<CompItemProcessor>().Props.isCompletelyAutoMachine || building_processor.processorStage != ProcessorStage.ExpectingIngredients || building_processor.thirdIngredientComplete || building_processor.thirdItem == "" )
            {
                return false;
            }

            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, 1, null, forced))
                {
                    if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                    {
                        return false;
                    }
                    if (this.FindIngredient(pawn, building_processor.thirdItem, building_processor) == null)
                    {
                        JobFailReason.Is(WorkGiver_InsertProcessorThird.NoIngredientFound, null);
                        return false;
                    }
                    return !t.IsBurning();
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_ItemProcessor building_processor = (Building_ItemProcessor)t;
            Thing t2 = this.FindIngredient(pawn, building_processor.thirdItem, building_processor);
            return new Job(DefDatabase<JobDef>.GetNamed("IP_InsertThirdIngredient", true), t, t2);
        }

        private Thing FindIngredient(Pawn pawn, string thirdItem, Building_ItemProcessor building_processor)
        {
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, 1, null, false);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            PathEndMode peMode = PathEndMode.ClosestTouch;
            
            if (building_processor.compItemProcessor.Props.isCategoryBuilding)
            {
                List<Thing> searchSet = new List<Thing>();
                foreach (ThingDef thingDef in ThingCategoryDef.Named(thirdItem).childThingDefs)
                {
                    if (!(DefDatabase<CombinationDef>.GetNamed(building_processor.thisRecipe).disallowedThingDefs != null &&
                        DefDatabase<CombinationDef>.GetNamed(building_processor.thisRecipe).disallowedThingDefs.Contains(thingDef.defName)))
                    {
                        searchSet.AddRange(map.listerThings.ThingsOfDef(thingDef));
                    }

                }
                
                return GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, null);
            }
            else
            {
                ThingRequest thingReq = ThingRequest.ForDef(ThingDef.Named(thirdItem));
                return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
            }
        }
    }
}



