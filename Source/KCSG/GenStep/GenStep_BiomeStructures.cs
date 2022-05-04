﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    public class GenStep_BiomeStructures : GenStep
    {
        public override int SeedPart => 919504193;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (map.Biome.GetModExtension<BiomeStructGenExtension>() is BiomeStructGenExtension ext)
            {
                if (ext.onlyOnPlayerMap && map.ParentFaction != Faction.OfPlayer)
                    return;

                int spawnCount = ext.countScaleHiliness ? ext.scalingOptions.GetScalingFor(map, ext.spawnCount) : ext.spawnCount;
                for (int i = 0; i < spawnCount; i++)
                {
                    StructureLayoutDef layout = ext.structures.RandomElementByWeight(l => l.commonality).layout;
                    int height = layout.height;
                    int width = layout.width;

                    IntVec3 spawnPos = CellFinderLoose.RandomCellWith((c) =>
                    {
                        CellRect rect = CellRect.CenteredOn(c, width + ext.clearCellRadiusAround, height + ext.clearCellRadiusAround);

                        if (!rect.InBounds(map))
                            return false;

                        if (!ext.canSpawnInMontains)
                            foreach (IntVec3 cell in rect.Cells)
                            {
                                if (!cell.Walkable(map))
                                    return false;
                            }

                        return true;
                    }, map);


                    CellRect spawnRect = CellRect.CenteredOn(spawnPos, width, height);
                    for (int o = 0; o < layout.layouts.Count; o++)
                    {
                        GenUtils.GenerateRoomFromLayout(layout, o, spawnRect, map);
                    }
                    GenUtils.GenerateRoofGrid(layout, spawnRect, map);
                }

                if (ext.postGenerateOre)
                {
                    GenStep_ScatterLumpsMineable gen = new GenStep_ScatterLumpsMineable
                    {
                        maxValue = ext.maxMineableValue
                    };

                    float count = 0f;
                    switch (Find.WorldGrid[map.Tile].hilliness)
                    {
                        case Hilliness.Flat:
                            count = 4f;
                            break;
                        case Hilliness.SmallHills:
                            count = 8f;
                            break;
                        case Hilliness.LargeHills:
                            count = 11f;
                            break;
                        case Hilliness.Mountainous:
                            count = 15f;
                            break;
                    }
                    gen.countPer10kCellsRange = new FloatRange(count, count);
                    gen.Generate(map, parms);
                }
            }
        }
    }
}