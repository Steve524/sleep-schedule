using HarmonyLib;
using RimWorld;
using Verse;

namespace SleepSchedulePresets.Patches
{
    [HarmonyPatch(typeof(Pawn_TimetableTracker), MethodType.Constructor, new[] { typeof(Pawn) })]
    public static class Pawn_TimetableTracker_Patch
    {
        static void Postfix(Pawn_TimetableTracker __instance, Pawn pawn)
        {
            if (Current.ProgramState != ProgramState.Playing || Scribe.mode != LoadSaveMode.Inactive) return;
            if (__instance == null || pawn == null || !pawn.RaceProps.Humanlike || pawn.Faction?.IsPlayer != true) return;
            SchedulePreset preset = SleepScheduleGameComponent.Instance?.defaultForNewPawns;
            if (preset?.assignments == null || preset.assignments.Count != 24) return;
            preset.ApplyToTracker(__instance);
        }
    }
}
