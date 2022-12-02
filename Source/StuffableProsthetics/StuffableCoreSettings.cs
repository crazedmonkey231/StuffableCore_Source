using RimWorld;
using StuffableCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore
{
    [StaticConstructorOnStartup]
    internal class StuffableCoreSettings : ModSettings
    {

        public CoreSettings CoreSettings = new CoreSettings();
        public ImplantProstheticSettings ImplantProstheticSettings = new ImplantProstheticSettings();
        public WeaponSettings WeaponSettings = new WeaponSettings();
        public MeleeSettings MeleeSettings = new MeleeSettings();
        public RangedSettings RangedSettings = new RangedSettings();
        public ClothingAndArmorSettings ClothingAndArmorSettings = new ClothingAndArmorSettings();
        public ClothingSettings ClothingSettings = new ClothingSettings();
        public ArmorSettings ArmorSettings = new ArmorSettings();

        public List<StuffableCategorySettings> GetAllStuffableCategorySettings()
        {
            return new List<StuffableCategorySettings> {
                ImplantProstheticSettings,
                WeaponSettings,
                MeleeSettings,
                RangedSettings,
                ClothingAndArmorSettings,
                ClothingSettings,
                ArmorSettings,
            };
        }

        public void ClearAllSettings()
        {
            GetAllStuffableCategorySettings().ForEach(i =>
            {
                i.ClearSettings();
            });
        }

        public void SetAllStuffableSettings(string key, bool enabled, string description, string fromModName)
        {
            GetAllStuffableCategorySettings().ForEach(i =>
            {
                i.SetSettings(key, enabled, description, fromModName);
            });
        }

        public void ToggleAll(bool toggle)
        {
            GetAllStuffableCategorySettings().ForEach(i =>
            {
                i.ToggleAllSettings(toggle);
            });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref CoreSettings, "CoreSettings");
            Scribe_Deep.Look(ref ImplantProstheticSettings, "ImplantProstheticSettings");
            Scribe_Deep.Look(ref WeaponSettings, "WeaponSettings");
            Scribe_Deep.Look(ref MeleeSettings, "MeleeSettings");
            Scribe_Deep.Look(ref RangedSettings, "RangedSettings");
            Scribe_Deep.Look(ref ClothingAndArmorSettings, "ClothingAndArmorSettings");
            Scribe_Deep.Look(ref ClothingSettings, "ClothingSettings");
            Scribe_Deep.Look(ref ArmorSettings, "ArmorSettings");
        }
    }
}
