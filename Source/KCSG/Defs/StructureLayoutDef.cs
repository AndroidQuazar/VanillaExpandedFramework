﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    public struct Pos
    {
        public int x;
        public int y;

        public Pos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Pos FromString(string str)
        {
            string[] array = str.Split(',');

            if (array.Length == 2)
            {
                return new Pos(int.Parse(array[0]), int.Parse(array[1]));
            }

            return new Pos(0, 0);
        }
    }

    public class StructureLayoutDef : Def
    {
        public bool isStorage = false;
        public bool spawnConduits = true;
        public List<List<string>> layouts = new List<List<string>>();
        public List<string> roofGrid = new List<string>();
        public List<string> terrainGrid = new List<string>();
        public bool forceGenerateRoof = false;
        public bool needRoofClearance = false;

        // Settings for SettlementDef
        public List<string> tags = new List<string>();

        // Mod requirements
        public List<string> modRequirements = new List<string>();

        // Spawn position
        public List<Pos> spawnAtPos = new List<Pos>();
        public List<string> spawnAt = new List<string>();

        // Values used regularly in gen:
        internal int width;
        internal int height;
        internal List<List<SymbolDef>> symbolsLists = new List<List<SymbolDef>>();
        internal List<TerrainDef> terrainGridResolved = new List<TerrainDef>();
        internal List<string> roofGridResolved = new List<string>();

        internal bool RequiredModLoaded { get; private set; }

        internal bool IsForSlaves { get; private set; }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            foreach (string sPos in spawnAt)
            {
                spawnAtPos.Add(Pos.FromString(sPos));
            }

            // Get height and width
            height = layouts[0].Count;
            width = layouts[0][0].Split(',').Count();
        }

        /// <summary>
        /// Populate symbol list and roofgrid. Prevent the need of doing it at each gen
        /// </summary>
        public void ResolveLayouts()
        {
            var modName = modContentPack.Name;
            // Populate symbolsLists and setup IsForSlaves
            for (int i = 0; i < layouts.Count; i++)
            {
                var layout = layouts[i];
                symbolsLists.Add(new List<SymbolDef>());
                for (int o = 0; o < layout.Count; o++)
                {
                    var symbols = layout[o].Split(',');
                    for (int p = 0; p < symbols.Length; p++)
                    {
                        string symbol = symbols[p];
                        if (symbol == ".")
                        {
                            symbolsLists[i].Add(null);
                        }
                        else
                        {
                            SymbolDef def = DefDatabase<SymbolDef>.GetNamedSilentFail(symbol);
                            symbolsLists[i].Add(def);

                            if (def == null)
                                StartupActions.AddToMissing($"{modName} {symbol}");
                            else if (def.isSlave)
                                IsForSlaves = true;
                        }
                    }
                }
            }
            // Populate roofgrid
            for (int i = 0; i < roofGrid.Count; i++)
            {
                roofGridResolved.AddRange(roofGrid[i].Split(','));
            }
            // Populate terrainGrid
            for (int i = 0; i < terrainGrid.Count; i++)
            {
                var tList = terrainGrid[i].Split(',');
                for (int o = 0; o < tList.Length; o++)
                {
                    var terrain = DefDatabase<TerrainDef>.GetNamedSilentFail(tList[o]);
                    terrainGridResolved.Add(terrain);
                }
            }
            // Resolve mod requirement(s)
            RequiredModLoaded = true;
            for (int o = 0; o < modRequirements.Count; o++)
            {
                if (!ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId == modRequirements[o].ToLower()))
                {
                    RequiredModLoaded = false;
                    break;
                }
            }
        }
    }
}
