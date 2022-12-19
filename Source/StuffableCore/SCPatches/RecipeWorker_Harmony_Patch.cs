using HarmonyLib;
using RimWorld;
using StuffableCore.SCComps;
using StuffableCore.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCPatches
{

    internal static class RecipeWorker_Harmony_Patch
    {

        public static void ApplyOnPawnPostfix(RecipeWorker __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (__instance.recipe?.addsHediff == null)
                return;

            HediffWithComps hediff = (HediffWithComps)pawn.health.hediffSet.hediffs.FindLast(x => x.def == __instance.recipe.addsHediff);

            if (hediff == null || hediff.TryGetComp<HediffCompStuffable>() == null)
                return;

            ThingDef stuff = ThingDefOf.Steel;
            ThingDef thingDef = hediff.def.spawnThingOnRemoved;
            if (SCMod.settings.ImplantProstheticSettings.enabled)
                stuff = SCMod.settings.ImplantProstheticSettings.GetEnabledIngredientsForEnabledCategories().RandomElement();
            if (SCMod.settings.EditorSettings.ThingDefSettingsCache.TryGetValue(thingDef.defName, out StuffableCategorySettings scs) && scs.enabled)
                stuff = scs.GetEnabledIngredientsForEnabledCategories().RandomElement();

            hediff.TryGetComp<HediffCompStuffable>().stuff = stuff;
        }
    }
}
