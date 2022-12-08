using HarmonyLib;
using RimWorld;
using StuffableCore.Settings;
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
    public class StuffableCoreMod : Mod
    {
        public static Harmony harmony;
        public static StuffableCoreSettings settings;

        public static int currentTabIndex = 0;
        public static ISettings RootWindow = new LandingSettingsPage();

        public Vector2 scrollPosition = new Vector2(0, 0);

        public static List<TabRecord> settingsTabs = new List<TabRecord>() {
            new TabRecord("Core", ()=>{
                currentTabIndex = 0;
                UpdateTab(currentTabIndex);
                RootWindow = settings.CoreSettings;
            }, false),
            new TabRecord("Implants/Prosthetics", ()=>{
                currentTabIndex = 1;
                UpdateTab(currentTabIndex);
                RootWindow = settings.ImplantProstheticSettings;
            }, false),
            new TabRecord("Weapons", ()=>{
                currentTabIndex = 2;
                UpdateTab(currentTabIndex);
                RootWindow = settings.WeaponSettings;
            }, false),
            new TabRecord("Clothing", ()=>{
                currentTabIndex = 3;
                UpdateTab(currentTabIndex);
                RootWindow = settings.ClothingAndArmorSettings;
            }, false),
        };

        public StuffableCoreMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony(Content.Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = GetSettings<StuffableCoreSettings>();
            
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

            IEnumerable<StuffCategoryDef> cacheStuffCategoryDef = DefDatabase<StuffCategoryDef>.AllDefs.Where(i => IsRawEnabled(i));

            IEnumerable<ThingDef> stuffableStuff = DefDatabase<ThingDef>.AllDefs.Where(i =>
            {
                bool? flag = i.apparel?.tags.NotNullAndContains(StuffableCoreConstants.stuffableTag);
                bool flagVal = false;
                if (flag.HasValue)
                    flagVal = flag.Value;

                return i.techHediffsTags.NotNullAndContains(StuffableCoreConstants.stuffableTag)
                    || i.weaponTags.NotNullAndContains(StuffableCoreConstants.stuffableTag)
                    || flagVal;
            });

            if (settings.CoreSettings.enableRawFoodStuffs)
            {
                foreach (ThingDef thingdef in DefDatabase<ThingDef>.AllDefs.Where(i =>
                {
                    bool flag = i.thingCategories != null && (i.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw)
                        || i.thingCategories.Contains(ThingCategoryDefOf.MeatRaw));

                    return i.thingCategories != null && flag;
                }))
                {
                    if (thingdef.stuffCategories == null)
                        thingdef.stuffCategories = new List<StuffCategoryDef>();

                    thingdef.thingCategories.Add(ThingCategoryDefOf.ResourcesRaw);

                    if (thingdef.stuffProps == null)
                        thingdef.stuffProps = new StuffProperties();

                    if (thingdef.stuffProps.categories == null)
                        thingdef.stuffProps.categories = new List<StuffCategoryDef>();
                    thingdef.stuffProps.categories.Add(StuffableCoreDefOf.StuffableCore_RawStuff);
                    thingdef.stuffProps.canSuggestUseDefaultStuff = true;
                    thingdef.stuffProps.color = thingdef.GetColorForStuff(thingdef);

                }
            }

            if (!stuffableStuff.EnumerableNullOrEmpty())
            {
                if (settings.ImplantProstheticSettings.enabled)
                {
                    foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefsListForReading.Where(i => i.tags != null && i.tags.Contains(StuffableCoreConstants.stuffableHediff)))
                    {
                        if (hediffDef.comps == null)
                            hediffDef.comps = new List<HediffCompProperties>();
                        hediffDef.comps.Add(new HediffCompProperties_Stuffable());
                    }
                        
                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.stuffableBodyPartTag, settings.ImplantProstheticSettings);
                }

                if (settings.WeaponSettings.enabled)
                {
                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.StuffableWeapon, settings.WeaponSettings);
                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.StuffableWeaponMelee, settings.MeleeSettings);
                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.StuffableWeaponRanged, settings.RangedSettings);
                }

                if (settings.ClothingAndArmorSettings.enabled)
                {

                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.StuffableWeapon, settings.ClothingAndArmorSettings);
                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.StuffableClothing, settings.ClothingSettings);
                    RunManager(cacheStuffCategoryDef, stuffableStuff, StuffableCoreConstants.StuffableArmor, settings.ArmorSettings);
                }

            }
        }

        private static void RunManager(IEnumerable<StuffCategoryDef> cacheStuffCategoryDef, IEnumerable<ThingDef> stuffableStuff, string tag, StuffableCategorySettings stuffableCategorySettings)
        {
            IEnumerable<ThingDef> newStuffableStuff = stuffableStuff;

            if (stuffableCategorySettings.enabled)
            {
                RunUpdate(cacheStuffCategoryDef, newStuffableStuff, tag, stuffableCategorySettings);
            }
        }

        public static void LoadSettings()
        {
            IEnumerable<StuffCategoryDef> stuffs = DefDatabase<StuffCategoryDef>.AllDefs.Where(i => IsRawEnabled(i));

            if (settings.CoreSettings.resetToDefaults)
            {
                settings.ClearAllSettings();
                foreach (StuffCategoryDef stuff in stuffs)
                {
                    string key = stuff.defName;
                    bool enabled = false;
                    string label = stuff.label;
                    string modName = stuff.modContentPack.Name;
                    if ("Metallic".Equals(stuff.defName))
                        enabled = true;
                    settings.SetAllStuffableSettings(key, enabled, label, modName);
                }
                settings.CoreSettings.resetToDefaults = false;
                return;
            }
            else
            {
                DoSettings(stuffs);
            }
        }

        private static void DoSettings(IEnumerable<StuffCategoryDef> stuffs)
        {
            List<StuffableCategorySettings> allSettings = settings.GetAllStuffableCategorySettings();
            foreach (StuffableCategorySettings setting in allSettings)
            {
                SetStuffSettings(stuffs, setting);
            }
        }

        private static void SetStuffSettings(IEnumerable<StuffCategoryDef> stuffs, StuffableCategorySettings setting)
        {
            setting.stuffCategoriesSetting = setting.stuffCategoriesSetting.Where(i => stuffs.Any(j => j.defName.Equals(i.Key))).ToDictionary(i => i.Key, i => i.Value);
            foreach (StuffCategoryDef stuff in stuffs)
            {
                if (!setting.stuffCategoriesSetting.ContainsKey(stuff.defName))
                {
                    setting.SetSettings(stuff.defName, false, stuff.label, stuff.modContentPack.Name);
                }
            }
        }

        private static bool IsRawEnabled(StuffCategoryDef i)
        {
            bool flag = true;
            if (!settings.CoreSettings.enableRawFoodStuffs && StuffableCoreDefOf.StuffableCore_RawStuff.Equals(i))
                flag = false;
            return flag;
        }

        public static void ResetSettingsCache()
        {
            DoSettings(DefDatabase<StuffCategoryDef>.AllDefs);
        }

        private static void RunUpdate(IEnumerable<StuffCategoryDef> stuffCategoryDefs, IEnumerable<ThingDef> stuffableStuff, string tag, StuffableCategorySettings categorySetting)
        {
            IEnumerable<ThingDef> altStuffList = DefDatabase<ThingDef>.AllDefs.Where(i => categorySetting.ApplyAltSearch(i));

            foreach (ThingDef thingDef in stuffableStuff)
            {
                if (categorySetting.ApplyAltSearch(thingDef))
                    altStuffList.ToList().AddDistinct(thingDef);
            }

            if (!altStuffList.EnumerableNullOrEmpty())
            {
                foreach (ThingDef thingdef in altStuffList)
                {
                    UpdateThingDef(stuffCategoryDefs, thingdef, categorySetting);
                }
            }
        }

        private static void UpdateThingDef(IEnumerable<StuffCategoryDef> cache, ThingDef thingdef, StuffableCategorySettings categorySettings)
        {
            SetDefaultsForCategories(thingdef);
            cache.ToList().ForEach(i => {
                categorySettings.stuffCategoriesSetting.TryGetValue(i.defName, out bool flag);
                if (flag)
                    thingdef.stuffCategories.Add(i);
            });
            categorySettings.UpdateThingDef(thingdef);
            UpdateCost(thingdef, categorySettings.DefaultStuffCost);
            UpdateScenarioItem(thingdef, categorySettings);
        }

        private static void UpdateScenarioItem(ThingDef thingdef, StuffableCategorySettings categorySettings)
        {
            StuffCategoryDef cacheStuffCategoryDef = DefDatabase<StuffCategoryDef>.AllDefs.Where(i => 
            { 
                categorySettings.stuffCategoriesSetting.TryGetValue(i.defName, out bool val);
                return val;
            }).RandomElement();

            ThingDef defaultThindDef = DefDatabase<ThingDef>.AllDefs.Where(i => i.stuffProps != null && i.stuffProps.categories != null && i.stuffProps.categories.Contains(cacheStuffCategoryDef)).RandomElement();

            foreach (ScenarioDef sceneDef in DefDatabase<ScenarioDef>.AllDefs)
            {
                IEnumerable<ScenPart> parts = sceneDef.scenario.AllParts.Where(part =>
                {
                    return part as ScenPart_ThingCount != null && thingdef.Equals((ThingDef)Traverse.Create(part).Field("thingDef").GetValue());
                });
                foreach (ScenPart_ThingCount part in parts.Cast<ScenPart_ThingCount>())
                {
                    Traverse.Create(part).Field("stuff").SetValue(defaultThindDef);
                }
            }
        }

        private static void SetDefaultsForCategories(ThingDef thingdef)
        {
            if (thingdef.stuffCategories == null)
                thingdef.stuffCategories = new List<StuffCategoryDef>();
            else
                thingdef.stuffCategories.Clear();
        }

        private static void UpdateCost(ThingDef thingdef, int defalutStuffCost)
        {
            if (thingdef.costList != null && !thingdef.costList.NullOrEmpty())
            {
                List<ThingDef> filter = new List<ThingDef>(){ ThingDefOf.Steel };
                if (thingdef.IsRangedWeapon)
                    filter.Add(ThingDefOf.WoodLog);
                FindCostItem(thingdef, filter, defalutStuffCost);
            }
        }

        private static void FindCostItem(ThingDef thingdef, List<ThingDef> filter, int defaultStuffCost)
        {
            int newStuffCostCount = 0;
            int currentCost = thingdef.costStuffCount;
            int filterSize = filter.Count;
            for (int i = 0; i < filterSize; i++)
            {
                ThingDef filterThingDef = filter[i];
                int index = thingdef.costList.FindIndex(j => filterThingDef.Equals(j.thingDef));
                newStuffCostCount = UpdateCostList(thingdef, newStuffCostCount, index);
                newStuffCostCount += currentCost;
            }
            thingdef.costStuffCount = newStuffCostCount > 0 ? newStuffCostCount : defaultStuffCost;
        }

        private static int UpdateCostList(ThingDef thingdef, int newStuffCostCount, int index)
        {
            if (index >= 0)
            {
                ThingDefCountClass itemCost = thingdef.costList[index];
                newStuffCostCount += itemCost.count;
                thingdef.costList.RemoveAt(index);
            }
            return newStuffCostCount;
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
