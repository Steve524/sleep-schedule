using System.Collections.Generic;
using HarmonyLib;
using Verse;
using SleepSchedulePresets.UI;

namespace SleepSchedulePresets.Patches
{
    /// <summary>
    /// Adds a "Schedule preset" quick-apply gizmo to player colonists. Because every selected
    /// colonist yields a gizmo with the same label and icon, RimWorld merges them into a single
    /// button; clicking it opens a float menu and applies the chosen preset to the whole
    /// selection at once. Applying goes through the synced PresetActions.ApplyPreset.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Patch
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            foreach (Gizmo g in gizmos)
            {
                yield return g;
            }

            if (PresetManager.IsValidSchedulable(__instance))
            {
                yield return PresetUIUtility.CreateApplyGizmo(__instance);
            }
        }
    }
}
