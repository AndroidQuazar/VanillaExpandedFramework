﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using Verse;

namespace MVCF.Features
{
    public class Feature_InventoryVerbs : Feature_Humanoid
    {
        public override string Name => "InventoryVerbs";

        public override IEnumerable<Patch> GetPatches()
        {
            foreach (var patch in base.GetPatches()) yield return patch;
            yield return Patch.Postfix(AccessTools.Method(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.Notify_ItemRemoved)),
                AccessTools.Method(GetType(), nameof(Notify_ItemRemoved)));
            yield return Patch.Postfix(AccessTools.Method(typeof(ThingOwner), "NotifyAdded"), AccessTools.Method(GetType(), nameof(Notify_Added)));
        }

        public void Notify_ItemRemoved(Pawn_InventoryTracker __instance, Thing item)
        {
            if (Base.IsIgnoredMod(item?.def?.modContentPack?.Name)) return;
            var man = __instance?.pawn?.Manager(false);
            if (man == null) return;
            var comp = item.TryGetComp<CompVerbsFromInventory>();
            if (comp?.VerbTracker?.AllVerbs is null) return;
            comp.Notify_Dropped();
            foreach (var verb in comp.VerbTracker.AllVerbs.Concat(man.ExtraVerbsFor(item))) man.RemoveVerb(verb);
        }

        public void Notify_Added(ThingOwner __instance, Thing item)
        {
            if (__instance.Owner is not Pawn_InventoryTracker {pawn: var pawn}) return;
            pawn?.Manager(false)?.AddVerbs(item);
        }
    }
}