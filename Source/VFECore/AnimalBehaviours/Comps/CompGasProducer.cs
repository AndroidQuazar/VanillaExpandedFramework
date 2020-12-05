﻿
using RimWorld;
using Verse;


namespace AnimalBehaviours
{
    class CompGasProducer : ThingComp
    {

        private int gasProgress = 0;
        private int gasTickMax = 64;
        private System.Random rand = new System.Random();

        public CompProperties_GasProducer Props
        {
            get
            {
                return (CompProperties_GasProducer)this.props;
            }
        }
        public override void CompTick()
        {

            this.gasProgress += 1;
            //Increasing gasTickMax reduces lag, but it will also look like ass
            if (this.gasProgress > gasTickMax)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn.Map != null)
                {
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(Props.radius);

                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map) && rand.NextDouble() < Props.rate)
                        {
                            Thing thing = ThingMaker.MakeThing(ThingDef.Named(Props.gasType), null);
                            thing.Rotation = Rot4.North;
                            thing.Position = current;
                            //Directly using SpawnSetup instead of GenSpawn.Spawn to further reduce lag
                            thing.SpawnSetup(pawn.Map, false);
                        }
                    }
                    this.gasProgress = 0;
                }
            }
        }
    }
}
