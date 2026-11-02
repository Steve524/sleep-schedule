using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using SleepSchedulePresets.UI;

namespace SleepSchedulePresets.Patches
{
    /// <summary>
    /// Adds a "Presets" button to the bottom-right of the vanilla Schedule tab, inside the
    /// pawn-table window's extra bottom strip. The top row has no free space at any width:
    /// the window is exactly as wide as its table (selector, hour columns, "Manage
    /// areas..."), so a top-right button always overlaps "Manage areas...". We patch the
    /// shared base method and guard on the Schedule window so other pawn-table tabs are
    /// untouched.
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

            Rect buttonRect = new Rect(rect.xMax - ButtonWidth, rect.yMax - ButtonHeight, ButtonWidth, ButtonHeight);

            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(buttonRect, "SSP.Presets".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManagePresets());
            }
            TooltipHandler.TipRegion(buttonRect, "SSP.Presets.Tip".Translate());
        }
    }
}
