using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings.Editor
{
    public class CategorySelector : BaseModule
    {
        public CategorySelector(StuffableCategorySettings selected) : base(selected) { }

        public override void GetSettings(Listing_Standard listing_Standard)
        {
            listing_Standard.Label("Stuffable Categorinator".Colorize(Color.green), tooltip: "Enable/Disable the stuff category for this recipe.");
            listing_Standard.GapLine();
            Selected.GetSettingsHeaderErrorMessage(listing_Standard);
            Selected.DisplayCheckboxes(listing_Standard);
        }
    }
}
