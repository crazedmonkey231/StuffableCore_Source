using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{

    internal class MeleeSettings : StuffableCategorySettings
    {
        public MeleeSettings()
        {
            settingsLabel = "Melee Settings";
            enabled = true;
            altSearch = true;
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Melee settings dropdown {0}".Formatted(meleeSettingsToggle ? "▲" : "▼"), ref meleeSettingsToggle, "Melee settings dropdown.");
            if (meleeSettingsToggle)
                DropDown(listingStandard);
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag1 = item.weaponTags.NotNullAndContains(StuffableCoreConstants.StuffableWeaponMelee);
            bool flag2 = !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ResourcesRaw);
            bool flag3 = false;
            bool flag4 = !item.IsDrug;

            List<WeaponClassDef> weaponClasses = item.weaponClasses;
            if (!weaponClasses.NullOrEmpty())
            {
                weaponClasses.ForEach(i => {
                    string name = i.defName;
                    flag3 = name.Contains("Melee") || name.Contains("MeleePiercer") || name.Contains("MeleeBlunt");
                });
            }
            
            return item.IsMeleeWeapon && flag1 && flag2 && flag3 && flag4;
        }
    }

    internal class RangedSettings : StuffableCategorySettings
    {
        public RangedSettings()
        {
            settingsLabel = "Ranged Settings";
            enabled = true;
            altSearch = true;
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Ranged settings dropdown {0}".Formatted(rangedSettingsToggle ? "▲" : "▼"), ref rangedSettingsToggle, "Ranged settings dropdown.");
            if (rangedSettingsToggle)
                DropDown(listingStandard);
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            
            bool flag1 = item.weaponTags.NotNullAndContains(StuffableCoreConstants.StuffableWeaponRanged);
            bool flag2 = !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ResourcesRaw);
            bool flag3 = false;
            bool flag4 = !item.IsDrug;

            List<WeaponClassDef> weaponClasses = item.weaponClasses;
            if (!weaponClasses.NullOrEmpty()){
                weaponClasses.ForEach(i => {
                    string name = i.defName;
                    flag3 = name.Contains("Ranged") || name.Contains("RangedHeavy") || name.Contains("RangedLight");
                });
            }

            return item.IsRangedWeapon && flag1 && flag2 && flag4 || flag3;
        }
    }

    internal class WeaponSettings : StuffableCategorySettings, ISettings, IExposable
    {

        public WeaponSettings()
        {
            settingsLabel = "Weapon Settings";
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            base.GetSettings(listingStandard);
            StuffableCoreMod.settings.MeleeSettings.GetSettings(listingStandard);
            StuffableCoreMod.settings.RangedSettings.GetSettings(listingStandard);
            listingStandard.CheckboxLabeled("Catch-all settings dropdown. {0}".Formatted(otherWeaponSettingsToggle ? "▲" : "▼"), ref otherWeaponSettingsToggle, StuffableCoreConstants.CatchAllToolTip);
            if (otherWeaponSettingsToggle)
                DropDown(listingStandard);
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag1 = !item.weaponTags.NotNullAndContains(StuffableCoreConstants.StuffableWeapon)
                && !item.weaponTags.NotNullAndContains(StuffableCoreConstants.StuffableWeaponMelee)
                && !item.weaponTags.NotNullAndContains(StuffableCoreConstants.StuffableWeaponRanged);
            bool flag2 = !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ResourcesRaw);
            bool flag3 = !item.IsDrug;
            return (item.IsRangedWeapon || item.IsMeleeWeapon) && flag1 && flag2 && flag3;
        }
    }
}
