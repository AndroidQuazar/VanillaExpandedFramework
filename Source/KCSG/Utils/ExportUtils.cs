﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public class ExportUtils
    {
        /// <summary>
        /// Return a struct def coresponding to area exported
        /// </summary>
        public static StructureLayoutDef CreateStructureDef(Map map, Area area)
        {
            var sld = new StructureLayoutDef();

            sld.defName = Dialog_ExportWindow.defName;
            sld.isStorage = Dialog_ExportWindow.isStorage;
            sld.spawnConduits = Dialog_ExportWindow.spawnConduits;
            sld.forceGenerateRoof = Dialog_ExportWindow.forceGenerateRoof;
            sld.needRoofClearance = Dialog_ExportWindow.needRoofClearance;
            sld.tags = Dialog_ExportWindow.tags.ToList();
            sld.terrainGrid = CreateTerrainlayout(area, map);
            sld.roofGrid = CreateRoofGrid(area, map);
            sld.modRequirements = GetNeededMods();

            int numOfLayout = GetMaxThings();
            for (int i = 0; i < numOfLayout; i++)
            {
                sld.layouts.Add(CreateIndexLayout(area, i));
            }

            sld.spawnAtPos = new List<Pos>();
            sld.spawnAt = new List<string>();
            sld.symbolsLists = new List<List<SymbolDef>>();
            sld.terrainGridResolved = new List<TerrainDef>();
            sld.roofGridResolved = new List<string>();
            sld.ResolveLayouts();

            return sld;
        }

        /// <summary>
        /// Create layout for things
        /// </summary>
        private static List<string> CreateIndexLayout(Area area, int index)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;

            List<Thing> addedThings = new List<Thing>();

            IntVec3 cell = cellExport.First();
            // For each row
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                // For each cell of the row
                for (int x = 0; x < hw.x; x++)
                {
                    // Get the thing on the cell
                    List<Thing> things = pairsCellThingList.TryGetValue(cell);
                    // Remove filth if needed
                    if (!Dialog_ExportWindow.exportFilth)
                        things.RemoveAll(t => t.def.category == ThingCategory.Filth);
                    // Remove plant if needed
                    if (!Dialog_ExportWindow.exportPlant)
                        things.RemoveAll(t => t.def.category == ThingCategory.Plant);

                    // Shouldn't be exported
                    if (things.Count < index + 1 || (area != null && !active.Contains(cell)))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else
                    {
                        Thing thing = things[index];
                        // It's a pawn
                        if (thing is Pawn pawn && pawn != null)
                        {
                            SymbolDef symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.pawnKindDefNS == pawn.kindDef);
                            if (symbolDef == null)
                            {
                                AddToString(ref temp, pawn.kindDef.defName, x, hw.x);
                            }
                            else
                            {
                                AddToString(ref temp, symbolDef.defName, x, hw.x);
                            }
                        }
                        // It's an item
                        else if (thing.def.category == ThingCategory.Item)
                        {
                            SymbolDef symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == things.First().def && s.thingDef.category == ThingCategory.Item);
                            if (symbolDef == null)
                            {
                                AddToString(ref temp, thing.def.defName, x, hw.x);
                            }
                            else
                            {
                                AddToString(ref temp, symbolDef.defName, x, hw.x);
                            }
                        }
                        // It's something else
                        // Make sure we don't add big building multiple times/add them on the right cell
                        else if (!addedThings.Contains(thing) && thing.Position == cell)
                        {
                            SymbolDef symbolDef;
                            if (thing.Stuff != null)
                            {
                                symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def
                                                                                                   && s.stuffDef == thing.Stuff
                                                                                                   && (thing.def.rotatable == false || s.rotation == thing.Rotation));
                            }
                            else
                            {
                                symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def
                                                                                                   && (thing.def.rotatable == false || s.rotation == thing.Rotation));
                            }

                            if (symbolDef == null)
                            {
                                string symbolString = thing.def.defName;
                                if (thing.Stuff != null) symbolString += "_" + thing.Stuff.defName;
                                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant) symbolString += "_" + StartupActions.Rot4ToStringEnglish(thing.Rotation);

                                AddToString(ref temp, symbolString, x, hw.x);
                            }
                            else
                            {
                                AddToString(ref temp, symbolDef.defName, x, hw.x);
                            }
                            // Add to treated things
                            addedThings.Add(thing);
                        }
                        // We added it, skip
                        else
                        {
                            AddToString(ref temp, ".", x, hw.x);
                        }
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            return ll;
        }

        /// <summary>
        /// Add a string to a string, add comma if necessary
        /// </summary>
        private static void AddToString(ref string str, string add, int x, int rx)
        {
            if (x + 1 == rx)
            {
                str += add;
            }
            else
            {
                str += add;
                str += ",";
            }
        }

        /// <summary>
        /// Create layout for terrains
        /// </summary>
        private static List<string> CreateTerrainlayout(Area area, Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;
            var add = false;

            IntVec3 cell = cellExport.First();
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                for (int x = 0; x < hw.x; x++)
                {
                    TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                    // Shouldn't be exported
                    if (area != null && !active.Contains(cell))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else if (!terrain.BuildableByPlayer && !terrain.HasTag("Road") && !Dialog_ExportWindow.exportNatural)
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else
                    {
                        AddToString(ref temp, terrain.defName, x, hw.x);
                        add = true;
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            if (add)
                return ll;

            return new List<string>();
        }

        /// <summary>
        /// Create roof grid
        /// </summary>
        private static List<string> CreateRoofGrid(Area area, Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;

            IntVec3 cell = cellExport.First();
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                for (int x = 0; x < hw.x; x++)
                {
                    if (area != null && !active.Contains(cell))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else if (cell.GetRoof(map) is RoofDef roofDef && roofDef != null)
                    {
                        var roofType = roofDef == RoofDefOf.RoofRockThick ? "3" : (roofDef == RoofDefOf.RoofRockThin ? "2" : "1");
                        AddToString(ref temp, roofType, x, hw.x);
                    }
                    else
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            return ll;
        }

        /// <summary>
        /// Create cache dic for things on area cells
        /// </summary>
        public static Dictionary<IntVec3, List<Thing>> FillCellThingsList(Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var list = new Dictionary<IntVec3, List<Thing>>();
            for (int i = 0; i < cellExport.Count; i++)
            {
                var cell = cellExport[i];
                list.Add(cell, cell.GetThingList(map).ToList());
            }
            return list;
        }

        /// <summary>
        /// Make an Area into a square
        /// </summary>
        public static List<IntVec3> AreaToSquare(Area a)
        {
            List<IntVec3> list = a.ActiveCells.ToList();
            MinMaxXZ(list, out int zMin, out int zMax, out int xMin, out int xMax);

            List<IntVec3> listOut = new List<IntVec3>();

            for (int zI = zMin; zI <= zMax; zI++)
            {
                for (int xI = xMin; xI <= xMax; xI++)
                {
                    listOut.Add(new IntVec3(xI, 0, zI));
                }
            }
            listOut.Sort((x, y) => x.z.CompareTo(y.z));
            return listOut;
        }

        /// <summary>
        /// Get smallest X and Z value out of a list
        /// </summary>
        private static void MinMaxXZ(List<IntVec3> list, out int zMin, out int zMax, out int xMin, out int xMax)
        {
            zMin = list[0].z;
            zMax = 0;
            xMin = list[0].x;
            xMax = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c.z < zMin) zMin = c.z;
                if (c.z > zMax) zMax = c.z;
                if (c.x < xMin) xMin = c.x;
                if (c.x > xMax) xMax = c.x;
            }
        }

        /// <summary>
        /// Get height/width from list
        /// </summary>
        private static IntVec2 EdgeFromList(List<IntVec3> cellExport)
        {
            var vec = new IntVec2();
            IntVec3 first = cellExport[0];

            for (int i = 0; i < cellExport.Count; i++)
            {
                var cell = cellExport[i];
                if (first.z == cell.z) vec.x++;
                if (first.x == cell.x) vec.z++;
            }

            return vec;
        }

        /// <summary>
        /// Get the maximum amount of things on one cell of the list
        /// </summary>
        private static int GetMaxThings()
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            int max = 1;
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                var count = 0;

                for (int o = 0; o < things.Count; o++)
                {
                    var thing = things[o];
                    if (!Dialog_ExportWindow.exportFilth && thing.def.category == ThingCategory.Filth)
                        continue;
                    if (!Dialog_ExportWindow.exportPlant && thing.def.category == ThingCategory.Plant)
                        continue;

                    count++;
                }

                if (count > max) max = count;
            }

            return max;
        }

        /// <summary>
        /// Get the needed mods for an export
        /// </summary>
        private static List<string> GetNeededMods()
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            var modsId = new HashSet<string>();
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                for (int o = 0; o < things.Count; o++)
                {
                    var packageId = things[o].def.modContentPack.PackageId;
                    if (packageId != "ludeon.rimworld")
                        modsId.Add(packageId);
                }
            }

            return modsId.ToList();
        }

        /// <summary>
        /// Create needed symbols
        /// </summary>
        public static List<SymbolDef> CreateSymbolIfNeeded(Area area)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            var symbols = new List<SymbolDef>();
            var activeCells = area?.ActiveCells;

            foreach (IntVec3 c in cellExport)
            {
                if (activeCells == null || activeCells.Contains(c))
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(c);
                    foreach (Thing t in things)
                    {
                        if (t.def.category == ThingCategory.Item)
                        {
                            var sym = CreateItemSymbol(t);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                        else if (t.def.category == ThingCategory.Pawn)
                        {
                            var sym = CreatePawnSymbol(t as Pawn);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                        else if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant)
                        {
                            var sym = CreateThingSymbol(t);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                    }
                }
            }

            return symbols;
        }

        /// <summary>
        /// Create symbol from item
        /// </summary>
        private static SymbolDef CreateItemSymbol(Thing thing)
        {
            var defName = thing.def.defName;

            if (!Dialog_ExportWindow.exportedSymbolsName.Contains(defName)
                && !DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defName).Any())
            {
                var symbol = new SymbolDef
                {
                    defName = defName,
                    thing = defName
                };

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }

        /// <summary>
        /// Create symbol from pawn
        /// </summary>
        private static SymbolDef CreatePawnSymbol(Pawn pawn)
        {
            var defName = pawn.kindDef.defName;

            if (!Dialog_ExportWindow.exportedSymbolsName.Contains(defName)
                && !DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defName).Any())
            {
                var symbol = new SymbolDef
                {
                    defName = defName,
                    pawnKindDef = defName
                };

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }

        /// <summary>
        /// Create symbol from thing
        /// </summary>
        private static SymbolDef CreateThingSymbol(Thing thing)
        {
            var defName = thing.def.defName;

            if (thing.Stuff != null)
                defName += "_" + thing.Stuff.defName;

            if (thing.def.rotatable && thing.def.category != ThingCategory.Plant && !thing.def.IsFilth)
                defName += "_" + StartupActions.Rot4ToStringEnglish(thing.Rotation);

            if (!Dialog_ExportWindow.exportedSymbolsName.Contains(defName)
                && !DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defName).Any())
            {
                var symbol = new SymbolDef
                {
                    defName = defName,
                    thing = thing.def.defName
                };

                if (thing.Stuff != null)
                    symbol.stuff = thing.Stuff.defName;

                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant)
                    symbol.rotation = thing.Rotation;

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }
    }
}