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
    public class ClothingSettings : StuffableCategorySettings
    {
        public ClothingSettings()
        {
            settingsLabel = "Clothing Settings";
            enabled = true;
            altSearch = true;
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Clothing settings dropdown {0}".Formatted(clothingSettingsToggle ? "▲" : "▼"), ref clothingSettingsToggle, "Clothing settings dropdown.");
            if (clothingSettingsToggle)
                DropDown(listingStandard);
        }

        public override void UpdateThingDef(ThingDef thingdef)
        {
            if (thingdef.statBases != null)
                if (thingdef.statBases.StatListContains(StatDefOf.StuffEffectMultiplierArmor))
                    return;

            if (thingdef.statBases == null)
                thingdef.statBases = new List<StatModifier>();

            thingdef.statBases.Add(new StatModifier()
            {
                stat = StatDefOf.StuffEffectMultiplierArmor,
                value = StuffableCoreMod.settings.ClothingAndArmorSettings.additionalStuffMultiplierArmor
            });
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            return item.IsApparel 
                && item.apparel?.tags != null 
                && item.apparel.tags.Contains(StuffableCoreConstants.StuffableClothing) 
                && !item.apparel.tags.Contains(StuffableCoreConstants.StuffableArmor);
        }
    }

    public class ArmorSettings : StuffableCategorySettings
    {
        public ArmorSettings()
        {
            settingsLabel = "Armor Settings";
            enabled = true;
            altSearch = true;
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Armor settings dropdown {0}".Formatted(armorSettingsToggle ? "▲" : "▼"), ref armorSettingsToggle, "Armor settings dropdown.");
            if (armorSettingsToggle)
                DropDown(listingStandard);
        }
        public override void UpdateThingDef(ThingDef thingdef)
        {
            if (thingdef.statBases != null)
                if (thingdef.statBases.StatListContains(StatDefOf.StuffEffectMultiplierArmor))
                    return;

            if (thingdef.statBases == null)
                thingdef.statBases = new List<StatModifier>();

            thingdef.statBases.Add(new StatModifier()
            {
                stat = StatDefOf.StuffEffectMultiplierArmor,
                value = StuffableCoreMod.settings.ClothingAndArmorSettings.additionalStuffMultiplierArmor
            });
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            return item.IsApparel
                && item.apparel?.tags != null
                && item.apparel.tags.Contains(StuffableCoreConstants.StuffableArmor);
        }
    }

    public class ClothingAndArmorSettings : StuffableCategorySettings, ISettings
    {

        public ClothingAndArmorSettings()
        {
            settingsLabel = "Clothing Settings";
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            base.GetSettings(listingStandard);
            listingStandard.Label("Stuff Effect Multiplier Armor (0-Uses base apparel stats, Recommended between 10-20 to get material bonuses): " + Math.Round(additionalStuffMultiplierArmor * 100));
            additionalStuffMultiplierArmor = listingStandard.Slider(additionalStuffMultiplierArmor, -1f, 1f);
            StuffableCoreMod.settings.ClothingSettings.GetSettings(listingStandard);
            StuffableCoreMod.settings.ArmorSettings.GetSettings(listingStandard);
            listingStandard.CheckboxLabeled("Catch-all settings dropdown {0}".Formatted(otherClothingToggle ? "▲" : "▼"), ref otherClothingToggle, StuffableCoreConstants.CatchAllToolTip);
            if (otherClothingToggle)
                DropDown(listingStandard);
        }

        public override void UpdateThingDef(ThingDef thingdef)
        {
            if (thingdef.statBases != null)
                if (thingdef.statBases.StatListContains(StatDefOf.StuffEffectMultiplierArmor))
                    return;

            if (thingdef.statBases == null)
                thingdef.statBases = new List<StatModifier>();

            thingdef.statBases.Add(new StatModifier()
            {
                stat = StatDefOf.StuffEffectMultiplierArmor,
                value = StuffableCoreMod.settings.ClothingAndArmorSettings.additionalStuffMultiplierArmor
            });
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool? contains = item.apparel?.tags?.NotNullAndContains(StuffableCoreConstants.stuffableTag);
            bool flag = false;
            if (contains.HasValue)
                flag = true;

            return item.IsApparel && flag;
        }
    }
}
