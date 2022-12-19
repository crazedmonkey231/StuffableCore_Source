using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCUtils
{
    public static class SearchUtil
    {
        public static bool IsSCWeapon(ThingDef item)
        {
            bool flag = (item.weaponTags.NotNullAndContains(SCConstants.StuffableWeapon)
                || item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponMelee)
                || item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponRanged));
            return flag;
        }

        public static bool IsNotSCWeapon(ThingDef item)
        {
            return !IsSCWeapon(item);
        }

        public static bool IsTurret(ThingDef item)
        {
            return item.weaponTags.NotNullAndContains("TurretGun");
        }

        public static bool IsNotTurret(ThingDef item)
        {
            return !IsTurret(item);
        }

        public static bool IsMechWeapon(ThingDef item)
        {
            bool flag = false;
            List<string> search = item.weaponTags;
            if (search.NullOrEmpty())
                return flag;
            foreach (string key in search)
            {
                string v0 = key.ToLower();
                if (v0.Contains("MechanoidGun".ToLower()) 
                    || v0.Contains("Hellsphere".ToLower()) 
                    || v0.Contains("BeamGraser".ToLower()) 
                    || v0.Contains("InfernoCannonGun".ToLower())
                    || v0.Contains("ChargeBlaster".ToLower()))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
