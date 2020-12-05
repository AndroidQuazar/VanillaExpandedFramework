﻿using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;


namespace AnimalBehaviours
{
    public class CompAnimalProduct : CompHasGatherableBodyResource
    {

        //CompAnimalProduct builds upon both CompMilkable and CompShearable, with many more configuration options


        System.Random rand = new System.Random();

        protected override int GatherResourcesIntervalDays
        {
            get
            {
                return this.Props.gatheringIntervalDays;
            }
        }



        protected override int ResourceAmount
        {
            get
            {
                return this.Props.resourceAmount;
            }
        }

        protected override ThingDef ResourceDef
        {
            get
            {
               
                //This selects a random output item
                if (Props.isRandom)
                {

                    return ThingDef.Named(Props.randomItems.RandomElement());
                }
                else
                {
                    return this.Props.resourceDef;
                }

            }
        }

        protected override string SaveKey
        {
            get
            {
                return "resourceGrowth";
            }
        }

        public CompProperties_AnimalProduct Props
        {
            get
            {
                return (CompProperties_AnimalProduct)this.props;
            }
        }

        protected override bool Active
        {
            get
            {
                if (!base.Active)
                {
                    return false;
                }
                Pawn pawn = this.parent as Pawn;
                //Using shearable, though this could be changed to allow only the last lifestage to be harvestable.
                //As it is, it needs a patch for adult insectoids, which aren't shearable
                return pawn == null || pawn.ageTracker.CurLifeStage.shearable;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!this.Active)
            {
                return null;
            }
            if (!this.Props.customResourceString.NullOrEmpty())
            {
                return Translator.Translate(this.Props.customResourceString) + ": " + base.Fullness.ToStringPercent();
            }
            else return Translator.Translate("ResourceGrowth") + ": " + base.Fullness.ToStringPercent();
        }

        public void InformGathered(Pawn doer)
        {
            //Mostly a copy from CompShearable
            if (!this.Active)
            {
                Log.Error(doer + " gathered body resources while not Active: " + this.parent, false);
            }
            if (!Rand.Chance(doer.GetStatValue(StatDefOf.AnimalGatherYield, true)))
            {
                MoteMaker.ThrowText((doer.DrawPos + this.parent.DrawPos) / 2f, this.parent.Map, "TextMote_ProductWasted".Translate(), 3.65f);
            }
            else
            {
                int i = GenMath.RoundRandom((float)this.ResourceAmount * this.fullness);
                while (i > 0)
                {
                    int num = Mathf.Clamp(i, 1, this.ResourceDef.stackLimit);
                    i -= num;
                    Thing thing = ThingMaker.MakeThing(this.ResourceDef, null);
                    thing.stackCount = num;
                    GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                }

                //Until here, where it handles additional products

                if (this.Props.hasAditional)
                {

                    if (rand.NextDouble() <= ((float)Props.additionalItemsProb / 100))
                    {
                        Thing thingExtra = ThingMaker.MakeThing(ThingDef.Named(Props.additionalItems.RandomElement()), null);
                        thingExtra.stackCount = Props.additionalItemsNumber;
                        GenPlace.TryPlaceThing(thingExtra, doer.Position, doer.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                    }

                }


            }
            this.fullness = 0f;
        }



    }
}
