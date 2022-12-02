using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    internal abstract class BaseSettings
    {
        public string settingsLabel = "";

        public virtual void GetSettingsHeader(Listing_Standard listing_Standard)
        {
            if(!"".Equals(settingsLabel))
                listing_Standard.Label("{0}".Formatted(settingsLabel));
            listing_Standard.Label("Please Restart Game For Changes To Take Effect");
            listing_Standard.Gap();
        }
    }
}
