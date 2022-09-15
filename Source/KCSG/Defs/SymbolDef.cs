﻿using System.Collections.Generic;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public class SymbolDef : Def
    {
        public bool spawnPartOfFaction = true;

        // Building & item basic infos
        public ThingDef thingDef = null;
        public string thing = null;
        public ThingDef replacementDef = null;

        public ThingDef stuffDef = null;
        public string stuff = null;
        public int maxStackSize = -1;

        // Building info
        public Rot4 rotation = Rot4.North;

        // Plant
        public float plantGrowth = 0.5f;

        // CryptosleepCasket and CorpseCasket
        public float chanceToContainPawn = 1f;
        public List<PawnKindDef> containPawnKindAnyOf = new List<PawnKindDef>();
        public List<PawnKindDef> containPawnKindForPlayerAnyOf = new List<PawnKindDef>();

        // Crate
        public ThingSetMakerDef thingSetMakerDef = null;
        public ThingSetMakerDef thingSetMakerDefForPlayer = null;
        public float crateStackMultiplier = 1f;

        // Pawn
        public string pawnKindDef = null;
        public PawnKindDef pawnKindDefNS = null;
        public bool isSlave = false;
        public FactionDef faction;
        public int numberToSpawn = 1;
        public bool spawnDead = false;
        public bool spawnRotten = false;
        public bool spawnFilthAround = false;
        public bool defendSpawnPoint = false;

        public override void ResolveReferences()
        {
            if (thing != null)
            {
                thingDef = DefDatabase<ThingDef>.GetNamed(thing, Debug.Enabled);
                if (thingDef == null && replacementDef != null)
                {
                    thingDef = replacementDef;
                    Debug.Message($"Replaced by {replacementDef.defName}");
                }
            }

            if (stuff != null)
                stuffDef = DefDatabase<ThingDef>.GetNamed(stuff, Debug.Enabled);

            if (pawnKindDef != null)
                pawnKindDefNS = DefDatabase<PawnKindDef>.GetNamed(pawnKindDef, Debug.Enabled);

            if (thingSetMakerDef == null)
                thingSetMakerDef = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Default;
        }

        public override string ToString() => defName;

        /// <summary>
        /// Create XML elements
        /// </summary>
        public string ToXMLString()
        {
            XElement layoutDef = new XElement("KCSG.SymbolDef", null);

            layoutDef.Add(new XElement("defName", defName));

            if (thing != null)
                layoutDef.Add(new XElement("thing", thing));

            if (stuff != null)
                layoutDef.Add(new XElement("stuff", stuff));

            if (pawnKindDef != null)
                layoutDef.Add(new XElement("pawnKindDef", pawnKindDef));

            if (isSlave)
                layoutDef.Add(new XElement("isSlave", isSlave));

            if (faction != null)
                layoutDef.Add(new XElement("faction", faction));

            if (spawnDead)
                layoutDef.Add(new XElement("spawnDead", spawnDead));

            if (spawnRotten)
                layoutDef.Add(new XElement("spawnRotten", spawnRotten));

            if (rotation != Rot4.North)
                layoutDef.Add(new XElement("rotation", StartupActions.Rot4ToStringEnglish(rotation)));

            return layoutDef.ToString();
        }
    }
}
