﻿using RimWorld.Planet;
using Verse;

namespace KCSG
{
    internal class GenStep_WorldObject : RimWorld.GenStep_Settlement
    {
        public override int SeedPart => 1969308471;

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            WorldObject worldO = Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == map.Tile && o.def.HasModExtension<CustomGenOption>());
            GenOption.ext = worldO.def.GetModExtension<CustomGenOption>();

            GenStepUtils.Generate(map, loc, GenOption.ext, GenOption.ext.symbolResolver ?? "kcsg_settlement");
        }
    }
}