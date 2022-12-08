using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore
{

    internal static class RecipeWorker_Harmony_Patch
    {

        public static void ApplyOnPawnPostfix(RecipeWorker __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Log.Message("Adding apply on pawn.");
            if (__instance.recipe?.addsHediff == null)
                return;

            Log.Message("Check complete.");

            HediffWithComps hediff = (HediffWithComps)pawn.health.hediffSet.hediffs.FindLast(x => x.def == __instance.recipe.addsHediff);

            if (hediff != null && hediff.TryGetComp<HediffCompStuffable>() != null)
            {
                Log.Message("Needs Hediff comp.");
                ThingDef stuff = null;
                bool needDefault = false;
                if (!ingredients.NullOrEmpty())
                {
                    stuff = ingredients.Find(i => i.def.techHediffsTags != null && !i.def.techHediffsTags.NullOrEmpty() && i.def.techHediffsTags.Contains(StuffableCoreConstants.stuffableBodyPartTag))?.Stuff;
                    if (stuff == null)
                        needDefault = true;
                }
                else
                {
                    needDefault = true;
                }

                if (needDefault)
                    stuff = ThingDefOf.Steel;

                hediff.TryGetComp<HediffCompStuffable>().stuff = stuff;

            }
        }
    }
}
