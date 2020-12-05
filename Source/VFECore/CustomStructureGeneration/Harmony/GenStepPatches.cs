﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.BaseGen;
using RimWorld.Planet;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Settlement))]
    [HarmonyPatch("ScatterAt", MethodType.Normal)]
    public class GenStepPatches
    {
        [HarmonyPrefix]
        public static bool Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            //KENT TEMP CODE
            if (VFECore.VFEGlobal.settings.enableLog)
            {
                Log.Message("Testing defs");
                foreach (SymbolDef def in DefDatabase<SymbolDef>.AllDefs)
                {
                    if (def.isTerrain && def.terrainDef == null)
                        Log.Error("Found invalid terrain def: " + def.defName);
                    else if (def.isPawn && def.pawnKindDefNS == null)
                        Log.Error("Found invalid pawn def: " + def.defName);
                    else if (!def.isTerrain && !def.isPawn && def.thingDef == null)
                        Log.Error("Found invalid thing def: " + def.defName);
                }
            }

            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement sf = map.ParentFaction.def.GetModExtension<FactionSettlement>();
                SettlementLayoutDef sld;
                if (ModLister.RoyaltyInstalled) sld = sf.chooseFrom.RandomElement();
                else sld = sf.chooseFrom.ToList().FindAll(sfl => !sfl.requireRoyalty).RandomElement();

                FactionSettlement.temp = sld;

                // Get faction
                Faction faction;
                if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
                {
                    faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
                }
                else faction = map.ParentFaction;

                // Get settlement size
                int width = sld.settlementSize.x;
                int height = sld.settlementSize.z;
                CellRect rect = new CellRect(c.x - width / 2, c.z - height / 2, width, height);
                rect.ClipInsideMap(map);

                ResolveParams rp = default(ResolveParams);
                rp.faction = faction;
                rp.rect = rect;


                BaseGen.globalSettings.map = map;
                BaseGen.symbolStack.Push("kcsg_settlement", rp, null);
                BaseGen.Generate();
                return false;
            }
            else if (Find.World.worldObjects.AllWorldObjects.Find(o=>o.Tile == map.Tile && o.def.HasModExtension<KCSG.FactionSettlement>()) is WorldObject worldObject)
            {
                FactionSettlement sf = worldObject.def.GetModExtension<FactionSettlement>();
                SettlementLayoutDef sld;
                if (ModLister.RoyaltyInstalled) sld = sf.chooseFrom.RandomElement();
                else sld = sf.chooseFrom.ToList().FindAll(sfl => !sfl.requireRoyalty).RandomElement();

                FactionSettlement.temp = sld;

                // Get faction
                Faction faction;
                if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
                {
                    Log.Error("Faction was null or player.... throwing random ennemy faction");
                    faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
                }
                else faction = map.ParentFaction;

                // Get settlement size
                int width = sld.settlementSize.x;
                int height = sld.settlementSize.z;
                CellRect rect = new CellRect(c.x - width / 2, c.z - height / 2, width, height);
                rect.ClipInsideMap(map);

                ResolveParams rp = default(ResolveParams);
                rp.faction = faction;
                rp.rect = rect;


                BaseGen.globalSettings.map = map;
                if (sf.symbolResolver == null) BaseGen.symbolStack.Push("kcsg_settlement", rp, null);
                else BaseGen.symbolStack.Push(sf.symbolResolver, rp, null);
                BaseGen.Generate();
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
