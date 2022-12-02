using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    internal class LandingSettingsPage : BaseSettings, ISettings
    {
        public void GetSettings(Listing_Standard listing_Standard)
        {
            listing_Standard.Label("Welcome!");
            listing_Standard.Label("Select Tab For Settings");
            GetSettingsHeader(listing_Standard);
        }
    }
}
