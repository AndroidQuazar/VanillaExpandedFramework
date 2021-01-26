using System.Collections.Generic;
using System.Linq;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Comps
{
    public class Comp_VerbGiver : ThingComp, IVerbOwner
    {
        private VerbTracker verbTracker;
        private bool worn;

        public Comp_VerbGiver()
        {
            verbTracker = new VerbTracker(this);
        }

        public CompProperties_VerbGiver Props => (CompProperties_VerbGiver) props;

        public VerbTracker VerbTracker => verbTracker;

        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        public List<Tool> Tools => parent.def.tools;

        Thing IVerbOwner.ConstantCaster => null;

        ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

        string IVerbOwner.UniqueVerbOwnerID()
        {
            return parent.GetUniqueLoadID() + "_" + parent.AllComps.IndexOf(this);
        }

        bool IVerbOwner.VerbsStillUsableBy(Pawn p)
        {
            return p.equipment.Contains(parent) || p.apparel.Contains(parent) || p.inventory.Contains(parent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref verbTracker, "verbTracker", (object) this);
            Scribe_Values.Look(ref worn, "worn");
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
                return;
            if (verbTracker == null)
                verbTracker = new VerbTracker(this);
            if (!(parent?.holdingOwner?.Owner is Pawn_ApparelTracker tracker)) return;
            foreach (var verb in verbTracker.AllVerbs) verb.caster = tracker.pawn;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (worn)
                verbTracker.VerbsTick();
        }

        public void Notify_Worn(Pawn pawn)
        {
            worn = true;
            foreach (var verb in verbTracker.AllVerbs)
            {
                verb.caster = pawn;
                verb.Notify_PickedUp();
            }
        }

        public void Notify_Unworn()
        {
            worn = false;
            foreach (var verb in verbTracker.AllVerbs)
            {
                verb.Notify_EquipmentLost();
                verb.caster = null;
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var gizmo in base.CompGetWornGizmosExtra())
                yield return gizmo;
            var man = (parent as Apparel)?.Wearer?.Manager();
            if (man == null) yield break;
            foreach (var gizmo in from verb in verbTracker.AllVerbs
                from gizmo in verb.GetGizmosForVerb(man.GetManagedVerbForVerb(verb))
                select gizmo) yield return gizmo;
        }

        public AdditionalVerbProps PropsFor(Verb verb)
        {
            var label = verb.verbProps.label;
            return string.IsNullOrEmpty(label) ? null : Props.verbProps?.FirstOrDefault(prop => prop.label == label);
        }
    }
}