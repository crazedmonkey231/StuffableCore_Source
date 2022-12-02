using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    internal class StuffableCategorySettings : BaseSettings, IExposable
    {
        public bool enabled = false;
        public bool altSearch = false;
        public bool rangedSettingsToggle = false;
        public bool meleeSettingsToggle = false;
        public bool otherWeaponSettingsToggle = false;
        public bool clothingSettingsToggle = false;
        public bool armorSettingsToggle = false;
        public bool otherClothingToggle = false;
        public float additionalStuffMultiplierArmor = 0.1f;
        public float additionalStuffMultiplierDamageSharp = 0.1f;
        public float defaultStuffCost = 0.25f;
        public bool useRawPlantForStuff = false;
        public Dictionary<string, bool> stuffCategoriesSetting = new Dictionary<string, bool>();
        public Dictionary<string, string> stuffCategoriesDescription = new Dictionary<string, string>();
        public Dictionary<string, string> stuffCategoriesModName = new Dictionary<string, string>();

        public virtual void GetSettings(Listing_Standard listingStandard)
        {
            GetSettingsHeader(listingStandard);
            string label = "Default stuff cost: {0}".Formatted(defaultStuffCost * 100);
            defaultStuffCost = listingStandard.SliderLabeled(label, defaultStuffCost, 0.01f, 1f);
        }

        public virtual void SetSettings(string key, bool enabled, string description, string fromModName)
        {
            stuffCategoriesSetting.SetOrAdd(key, enabled);
            stuffCategoriesDescription.SetOrAdd(key, description);
            stuffCategoriesModName.SetOrAdd(key, fromModName);
        }

        public virtual void RemoveSettings(string key)
        {
            stuffCategoriesSetting.Remove(key);
            stuffCategoriesDescription.Remove(key);
            stuffCategoriesModName.Remove(key);
        }

        public virtual bool IsSettingsValid()
        {
            if (!enabled)
                return true;

            bool flag = false;
            foreach (bool val in stuffCategoriesSetting.Values)
                if(val)
                    flag = val;

            return flag;
        }

        public virtual void ToggleAllSettings(bool toggle)
        {
            enabled = toggle;
            altSearch = toggle;
            rangedSettingsToggle = toggle;
            meleeSettingsToggle = toggle;
            otherWeaponSettingsToggle = toggle;
            clothingSettingsToggle = toggle;
            armorSettingsToggle = toggle;
            otherClothingToggle = toggle;
            additionalStuffMultiplierArmor = 0.1f;
            Dictionary<string, bool> oldStuffCategoriesSetting = stuffCategoriesSetting;
            Dictionary<string, bool> newStuffCategoriesSetting = new Dictionary<string, bool>(); ;
            foreach (string key in oldStuffCategoriesSetting.Keys)
                newStuffCategoriesSetting.SetOrAdd(key, toggle);
            stuffCategoriesSetting = newStuffCategoriesSetting;
        }

        public virtual bool ApplyAltSearch(ThingDef item)
        {
            return false;
        }

        public override void GetSettingsHeader(Listing_Standard listing_Standard)
        {
            listing_Standard.CheckboxLabeled("Enabled On Next Restart?", ref enabled, "Enabled On Next Restart?");
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Use additional search algorithm?", ref altSearch, "Uses an additional search algorithm to try and find potential stuffable items from other mods that may not inherit from base game.");
            listing_Standard.Gap();
        }

        public virtual void GetSettingsHeaderErrorMessage(Listing_Standard listing_Standard)
        {
            if (!IsSettingsValid())
            {
                listing_Standard.GapLine();
                listing_Standard.Label("Please select at least one category.");
                listing_Standard.GapLine();
            }
        }
        public void DropDown(Listing_Standard listingStandard)
        {
            listingStandard.GapLine();
            GetSettingsHeaderErrorMessage(listingStandard);
            DisplayCheckboxes(listingStandard);
            listingStandard.GapLine();
        }

        public void DisplayCheckboxes(Listing_Standard listing_Standard)
        {
            if (!stuffCategoriesSetting.NullOrEmpty())
            {
                int count = stuffCategoriesSetting.Keys.Count;
                for (int i = 0; i < count; i++)
                {
                    string label = stuffCategoriesSetting.ElementAt(i).Key;
                    bool state = stuffCategoriesSetting.ElementAt(i).Value;
                    string desc = stuffCategoriesDescription.ElementAt(i).Value;
                    string fromMod = stuffCategoriesModName.ElementAt(i).Value;
                    listing_Standard.CheckboxLabeled(label, ref state, "{0} from mod {1}".Formatted(desc, fromMod).CapitalizeFirst());
                    stuffCategoriesSetting.SetOrAdd(label, state);
                }
            }
        }

        public virtual void UpdateThingDef(ThingDef thingdef)
        {
            if (thingdef.stuffProps == null)
                thingdef.stuffProps = new StuffProperties();
            if (thingdef.stuffProps.statFactors == null)
                thingdef.stuffProps.statFactors = new List<StatModifier>();
        }

        internal void ClearSettings()
        {
            stuffCategoriesSetting.Clear();
            stuffCategoriesDescription.Clear();
            stuffCategoriesModName.Clear();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", false);
            Scribe_Values.Look(ref altSearch, "altSearch", false);
            Scribe_Values.Look(ref defaultStuffCost, "defaultStuffCost", 0.25f);
            Scribe_Values.Look(ref additionalStuffMultiplierDamageSharp, "additionalStuffMultiplierDamageSharp", 0.1f);
            Scribe_Values.Look(ref useRawPlantForStuff, "useRawPlantForStuff", false);


            Scribe_Values.Look(ref rangedSettingsToggle, "rangedSettingsToggle", false);
            Scribe_Values.Look(ref meleeSettingsToggle, "meleeSettingsToggle", false);
            Scribe_Values.Look(ref otherWeaponSettingsToggle, "otherWeaponSettingsToggle", false);

            Scribe_Values.Look(ref clothingSettingsToggle, "clothingSettingsToggle", false);
            Scribe_Values.Look(ref armorSettingsToggle, "armorSettingsToggle", false);
            Scribe_Values.Look(ref otherClothingToggle, "otherClothingToggle", false);
            Scribe_Values.Look(ref additionalStuffMultiplierArmor, "additionalStuffMultiplier", 0.1f);

            Scribe_Collections.Look(ref stuffCategoriesSetting, "stuffCategoriesSetting");
            Scribe_Collections.Look(ref stuffCategoriesDescription, "stuffCategoriesDescription");
            Scribe_Collections.Look(ref stuffCategoriesModName, "stuffCategoriesModName");
        }
    }
}
