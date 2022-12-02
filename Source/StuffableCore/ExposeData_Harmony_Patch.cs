using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;
using Verse;

namespace StuffableCore
{
    [HarmonyPatch(typeof(HediffWithComps), "ExposeData")]
    internal static class ExposeData_Harmony_Patch
    {

        public static void Prefix(HediffWithComps __instance)
        {
            HediffCompStuffable hcs = __instance.TryGetComp<HediffCompStuffable>();
            if(hcs != null)
            {
                Log.Message("Prefix Found");
            }
        }

        public static void Postfix(HediffWithComps __instance)
        {
            if (!__instance.comps.NullOrEmpty() && __instance.TryGetComp<HediffCompStuffable>() != null)
                Scribe_Collections.Look(ref __instance.comps, "comps", LookMode.Value);
        }
    }
}
