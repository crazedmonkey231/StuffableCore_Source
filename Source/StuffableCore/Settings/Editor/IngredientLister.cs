using HarmonyLib;
using RimWorld;
using StuffableCore.SCCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings.Editor
{
    public class IngredientLister : BaseModule, ICarousel
    {
        private int index = 0;
        private int windowListSize = 0;

        private int boundsMin = 0;
        private int boundsMax = 0;

        private Dictionary<string, bool> ingredientsEnabledCache;

        public IngredientLister(StuffableCategorySettings selected) : base(selected)
        {
        }

        public IngredientLister(StuffableCategorySettings selected, int windowListSize) : base(selected)
        {
            this.windowListSize = windowListSize;
        }

        public void ChangeIndex(int index)
        {
            this.index = index;
        }

        public override void GetSettings(Listing_Standard listing_Standard)
        {
            ingredientsEnabledCache = Selected.GetIngredientsForEnabledCategories();
            if (!ingredientsEnabledCache.NullOrEmpty())
            {
                int listSize = ingredientsEnabledCache.Count;
                boundsMin = index * windowListSize;
                boundsMax = Math.Min(boundsMin + windowListSize, listSize);

                for (int i = boundsMin; i < boundsMax; i++)
                {
                    string key = ingredientsEnabledCache.ElementAt(i).Key;
                    bool value = ingredientsEnabledCache.ElementAt(i).Value;
                    Selected.GetIngredientDescription(key, out string label);
                    listing_Standard.CheckboxLabeled(label, ref value, tooltip: "Keep enabled for recipe?");
                    Selected.ingredientStateSetting.SetOrAdd(key, value);
                }
            }
        }

        public bool Search(string search, out int newIndex)
        {
            int count = ingredientsEnabledCache.Count;
            newIndex = -1;
            bool flag = false;
            for(int i = 0; i < count; i++)
            {
                var item = ingredientsEnabledCache.ElementAt(i);
                string key = item.Key;
                if (key.ToLower().Contains(search.ToLower()))
                {
                    newIndex = i / windowListSize;
                    index = newIndex;
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
