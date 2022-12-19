using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings.Editor
{
    public class IngredientSelector : BaseModule
    {
        private static string searchText = "";
        private static ICarousel ingredientsInnerWindow;

        private static int carouselIndex = 0;
        private static int carouselListSize = 10;

        public static int CarouselIndexMax
        {
            get
            {
                return Selected.ingredientStateFilterSize / carouselListSize;
            }
        }

        public IngredientSelector(StuffableCategorySettings selected) : base(selected)
        {

        }

        public override void GetSettings(Listing_Standard listing_Standard)
        {
            listing_Standard.Label("Ingredient Selectinator".Colorize(Color.green), tooltip: "Remove ingredient from recipe.");
            listing_Standard.GapLine();
            searchText = listing_Standard.TextEntryLabeled("Search? ", searchText);
            if (!searchText.NullOrEmpty())
                ingredientsInnerWindow.Search(searchText, out carouselIndex);
            listing_Standard.Gap();

            Rect rect = listing_Standard.GetRect(250);
            if (ingredientsInnerWindow == null)
                ingredientsInnerWindow = new IngredientLister(Selected, carouselListSize);
            DoInner(ingredientsInnerWindow, new Listing_Standard(), new Rect(rect.x, rect.y, rect.width, rect.height));

            Rect selectorRect = listing_Standard.GetRect(30);
            DoInner(new Listing_Standard(), new Rect(selectorRect.x, selectorRect.y + 5, selectorRect.width, selectorRect.height));
        }

        private static void DoInner(ICarousel innerWindow, Listing_Standard inner, Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect;
            innerRect.x += 5;
            innerRect.y += 5;
            innerRect.width -= 10;
            inner.Begin(innerRect);
            if (innerWindow != null)
                innerWindow.GetSettings(inner);
            inner.End();
        }

        private static void DoInner(Listing_Standard inner, Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            float width = rect.width / 3;
            DoLSection(inner, new Rect(rect.x, rect.y, width, rect.height));
            DoCSection(inner, new Rect(rect.x + width, rect.y, width, rect.height));
            DoRSection(inner, new Rect(rect.x + (width * 2), rect.y, width, rect.height));
        }

        private static void DoLSection(Listing_Standard inner, Rect rect)
        {
            inner.Begin(rect);
            if (inner.ButtonText("◄"))
            {
                carouselIndex = Math.Max(--carouselIndex, 0);
                ingredientsInnerWindow.ChangeIndex(carouselIndex);
            }
            inner.End();
        }

        private static void DoCSection(Listing_Standard inner, Rect rect)
        {
            Rect center = rect;
            center.x += 25;
            center.y += 5;
            inner.Begin(center);
            carouselIndex = Math.Min(carouselIndex, CarouselIndexMax);
            inner.Label("{0} / {1}".Formatted(carouselIndex, CarouselIndexMax));
            inner.End();
        }

        private static void DoRSection(Listing_Standard inner, Rect rect)
        {
            inner.Begin(rect);
            if(inner.ButtonText("►"))
            {
                carouselIndex = Math.Min(++carouselIndex, CarouselIndexMax);
                ingredientsInnerWindow.ChangeIndex(carouselIndex);
            }
            inner.End();
        }
    }
}
