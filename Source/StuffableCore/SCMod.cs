using HarmonyLib;
using RimWorld;
using StuffableCore.SCCaching;
using StuffableCore.SCComps;
using StuffableCore.SCPatches;
using StuffableCore.Settings;
using StuffableCore.Settings.BulkEditor;
using StuffableCore.Settings.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Configuration;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace StuffableCore
{
    public class SCMod : Mod
    {
        public static Harmony harmony;
        public static SCSettings settings;

        public static int currentTabIndex = 0;
        public static ISettings RootWindow = new LandingSettingsPage();

        public static List<TabRecord> settingsTabs = new List<TabRecord>() {
            new TabRecord("Core", ()=>{
                currentTabIndex = 0;
                UpdateTab(currentTabIndex);
                RootWindow = settings.CoreSettings;
            }, false),
            new TabRecord("Implants/Prosthetics", ()=>{
                currentTabIndex = 1;
                UpdateTab(currentTabIndex);
                RootWindow = BulkEditorModule.GetDefaultEditor(new List<StuffableCategorySettings>()
                {
                    settings.ImplantProstheticSettings
                });
            }, false),
            new TabRecord("Bulk Weapons", ()=>{
                currentTabIndex = 2;
                UpdateTab(currentTabIndex);
                RootWindow = BulkEditorModule.GetDefaultEditor(new List<StuffableCategorySettings>()
                {
                    settings.RangedSettings,
                    settings.MeleeSettings,
                    settings.WeaponSettings
                });
            }, false),
            new TabRecord("Bulk Armor/Clothing", ()=>{
                currentTabIndex = 3;
                UpdateTab(currentTabIndex);
                RootWindow = BulkEditorModule.GetDefaultEditor(new List<StuffableCategorySettings>()
                { 
                    settings.ClothingSettings,
                    settings.ArmorSettings,
                    settings.ClothingAndArmorSettings
                });
            }, false),
             new TabRecord("Specific Item Editor", ()=>{
                currentTabIndex = 4;
                UpdateTab(currentTabIndex);
                RootWindow = null;
                RootWindow = EditorModule.GetDefaultEditor(settings.EditorSettings);
            }, false),
        };

        public SCMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony(Content.Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = GetSettings<SCSettings>();
            
            if (settings.CoreSettings.modEnabled)
            {
                if (settings.ImplantProstheticSettings.enabled)
                {
                    foreach (Type type in typeof(RecipeWorker).AllSubclassesNonAbstract())
                    {
                        try
                        {
                            harmony.Patch(type.GetMethod("ApplyOnPawn"), null, new HarmonyMethod(typeof(RecipeWorker_Harmony_Patch).GetMethod("ApplyOnPawnPostfix")));
                        }
                        catch (Exception) { }
                    }
                }

                harmony.Patch(AccessTools.Method(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve"), prefix: new HarmonyMethod(GetType(), "Main"));
                harmony.Patch(AccessTools.Method(typeof(DefGenerator), "GenerateImpliedDefs_PostResolve"), postfix: new HarmonyMethod(GetType(), "LoadSettings"));
            }
        }

        public static void Main()
        {
            if (settings.CoreSettings.needsUpdate)
            {
                settings.GenNewSettings();
                settings.CoreSettings.needsUpdate = false;
            }

            IEnumerable<StuffCategoryDef> cacheStuffCategoryDef = DefDatabase<StuffCategoryDef>.AllDefs.Where(i => IsRawEnabled(i));
            foreach (StuffCategoryDef stuffCategoryDef in cacheStuffCategoryDef)
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(i => i.stuffProps != null && i.stuffProps.categories.NotNullAndContains(stuffCategoryDef)))
                    CacheUtil.AddToCache(stuffCategoryDef.defName, thingDef, IngredientSelectionCache.IngredientCache);

            IEnumerable<ThingDef> stuffableStuff = DefDatabase<ThingDef>.AllDefs.Where(i =>
            {
                bool? flag = i.apparel?.tags.NotNullAndContains(SCConstants.stuffableTag);
                bool flagVal = false;
                if (flag.HasValue)
                    flagVal = flag.Value;

                return (i.techHediffsTags.NotNullAndContains(SCConstants.stuffableTag)
                    || i.weaponTags.NotNullAndContains(SCConstants.stuffableTag)
                    || flagVal) && i.recipeMaker != null;
            });

            if (settings.CoreSettings.enableRawFoodStuffs)
            {
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(i =>
                {
                    bool flag = i.thingCategories != null && (i.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw)
                        || i.thingCategories.Contains(ThingCategoryDefOf.MeatRaw));

                    return i.thingCategories != null && flag;
                }))
                {
                    if (thingDef.stuffCategories == null)
                        thingDef.stuffCategories = new List<StuffCategoryDef>();

                    thingDef.thingCategories.Add(ThingCategoryDefOf.ResourcesRaw);

                    if (thingDef.stuffProps == null)
                        thingDef.stuffProps = new StuffProperties();

                    if (thingDef.stuffProps.categories == null)
                        thingDef.stuffProps.categories = new List<StuffCategoryDef>();
                    thingDef.stuffProps.categories.Add(SCDefOf.StuffableCore_RawStuff);
                    thingDef.stuffProps.canSuggestUseDefaultStuff = true;
                    thingDef.stuffProps.color = thingDef.GetColorForStuff(thingDef);
                    CacheUtil.AddToCache(SCDefOf.StuffableCore_RawStuff.defName, thingDef, IngredientSelectionCache.IngredientCache);
                }
            }

            if (!stuffableStuff.EnumerableNullOrEmpty())
            {
                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.ImplantProstheticSettings);

                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.WeaponSettings);
                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.MeleeSettings);
                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.RangedSettings);

                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.ClothingAndArmorSettings);
                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.ClothingSettings);
                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.ArmorSettings);

                RunUpdate(cacheStuffCategoryDef, stuffableStuff, settings.EditorSettings);
            }
        }

        private static void RunUpdate(IEnumerable<StuffCategoryDef> stuffCategoryDefs, IEnumerable<ThingDef> stuffableStuff, StuffableCategorySettings categorySetting)
        {

            List<ThingDef> newStuffList = new List<ThingDef>();
            foreach (ThingDef thingDef in stuffableStuff.Where(i => categorySetting.ApplySearch(i)))
                newStuffList.AddDistinct(thingDef);

            if (categorySetting.altSearch)
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(i => categorySetting.ApplyAltSearch(i)))
                    newStuffList.AddDistinct(thingDef);

            if (categorySetting.enabled)
            {
                categorySetting.Initialize();
                foreach (ThingDef thingdef in newStuffList)
                    categorySetting.ApplyStuffCategoryValues(stuffCategoryDefs, thingdef);
            }
        }

        public static void LoadSettings()
        {
            IEnumerable<StuffCategoryDef> stuffs = DefDatabase<StuffCategoryDef>.AllDefs.Where(i => IsRawEnabled(i));
            foreach (StuffableCategorySettings setting in settings.GetAllStuffableCategorySettings())
                setting.SetStuffDefaults(stuffs);
        }

        private static bool IsRawEnabled(StuffCategoryDef i)
        {
            bool flag = true;
            if (!settings.CoreSettings.enableRawFoodStuffs && SCDefOf.StuffableCore_RawStuff.Equals(i))
                flag = false;
            return flag;
        }

        public static void ResetSettingsCache()
        {

        }

        private static void UpdateTab(int currentTabIndex)
        {
            ResetSelectedTab();
            settingsTabs[currentTabIndex].selected = true;
        }

        private static void ResetSelectedTab()
        {
            settingsTabs.ForEach(i => i.selected = false);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect interiorRect = inRect;
            interiorRect.y += 50;
            TabDrawer.DrawTabs(interiorRect, settingsTabs);
            listingStandard.Begin(interiorRect);
            listingStandard.Gap();
            if (settings.CoreSettings.clearCache)
            {
                ResetSettingsCache();
                settings.CoreSettings.clearCache = false;
            }
            if (settings.CoreSettings.toggleAll)
                settings.ToggleAll(true);
            if (settings.CoreSettings.toggleAllOff)
                settings.ToggleAll(false);
            if (RootWindow != null)
                RootWindow.GetSettings(listingStandard);
            listingStandard.Gap();
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
            settings.Write();
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

    }
}
