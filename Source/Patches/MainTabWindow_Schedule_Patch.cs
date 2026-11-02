using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using SleepSchedulePresets.UI;

namespace SleepSchedulePresets.Patches
{
    /// <summary>
    /// Adds a "Presets" button to the top-right of the vanilla Schedule tab. We patch the shared
    /// base method and guard on the Schedule window so the other pawn-table tabs are untouched.
    /// This is interface-only drawing, so it has no bearing on simulation or Multiplayer sync.
    /// </summary>
    [HarmonyPatch(typeof(MainTabWindow_PawnTable), "DoWindowContents")]
    public static class MainTabWindow_Schedule_DoWindowContents_Patch
    {
        private const float ButtonWidth = 140f;
        private const float ButtonHeight = 30f;

        static void Postfix(MainTabWindow_PawnTable __instance, object[] __args)
        {
            if (!(__instance is MainTabWindow_Schedule)) return;
            if (__args == null || __args.Length == 0 || !(__args[0] is Rect rect)) return;

            Rect buttonRect = new Rect(rect.xMax - ButtonWidth, rect.y, ButtonWidth, ButtonHeight);

            float selectorRight = rect.x + (ModsConfig.RoyaltyActive ? 477.5f : 382f);
            if (buttonRect.x <= selectorRight + 8f) return;

            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(buttonRect, "SSP.Presets".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManagePresets());
            }
            TooltipHandler.TipRegion(buttonRect, "SSP.Presets.Tip".Translate());
        }
    }
}
