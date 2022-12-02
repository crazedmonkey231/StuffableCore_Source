using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    internal class ImplantProstheticSettings : StuffableCategorySettings, ISettings
    {
        public ImplantProstheticSettings()
        {
            settingsLabel = "Implant Prosthetic Settings";
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            base.GetSettings(listingStandard);
            DropDown(listingStandard);
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            string name = item.defName.ToLower();
            bool flag1 = name.Contains("prosthetic");
            bool flag2 = name.Contains("bionic");
            bool flag3 = name.Contains("archotech");
            return ((flag1 || flag2 || flag3) && item.category == ThingCategory.Item) || (item.techHediffsTags != null && item.techHediffsTags.Contains(StuffableCoreConstants.stuffableBodyPartTag));
        }
    }
}
