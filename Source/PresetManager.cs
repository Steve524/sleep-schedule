using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SleepSchedulePresets
{
    public enum PresetTier { Global, Save }

    public static class PresetManager
    {
        // Def order matches the vanilla selector: Anything, Work, Joy, Sleep, Meditate.
        public static List<TimeAssignmentDef> AllAssignments =>
            DefDatabase<TimeAssignmentDef>.AllDefsListForReading;

        public static TimeAssignmentDef ResolveAssignment(string name)
        {
            return (string.IsNullOrEmpty(name) ? null : DefDatabase<TimeAssignmentDef>.GetNamedSilentFail(name))
                ?? TimeAssignmentDefOf.Anything;
        }

        public static bool IsUnresolvable(string name)
        {
            return !string.IsNullOrEmpty(name) && DefDatabase<TimeAssignmentDef>.GetNamedSilentFail(name) == null;
        }

        public static string[] VanillaAssignments()
        {
            string[] result = new string[24];
            for (int hour = 0; hour < 24; hour++) result[hour] = hour > 5 && hour <= 21 ? "Anything" : "Sleep";
            return result;
        }

        public static List<SchedulePreset> GlobalPresets => SleepScheduleMod.Settings != null
            ? SleepScheduleMod.Settings.globalPresets : new List<SchedulePreset>();
        public static int NextGlobalId() => SleepScheduleMod.Settings == null ? 1 : SleepScheduleMod.Settings.nextGlobalId++;
        public static void SaveGlobal() { SleepScheduleMod.Settings?.Write(); }
        public static void AddGlobalPreset(SchedulePreset preset) { GlobalPresets.Add(preset); SaveGlobal(); }
        public static void DeleteGlobalPreset(SchedulePreset preset) { GlobalPresets.Remove(preset); SaveGlobal(); }

        public static List<SchedulePreset> SavePresets => SleepScheduleGameComponent.Instance != null
            ? SleepScheduleGameComponent.Instance.savePresets : new List<SchedulePreset>();
        public static bool SaveTierAvailable => SleepScheduleGameComponent.Instance != null;
        public static SchedulePreset FindSavePreset(int id) => SavePresets.FirstOrDefault(p => p != null && p.id == id);

        public static bool IsValidSchedulable(Pawn p) =>
            p != null && p.timetable != null && p.IsColonist && !p.IsPrisonerOfColony;

        public static List<Pawn> SelectedSchedulables()
        {
            if (Current.Game == null || Find.Selector == null) return new List<Pawn>();
            return Find.Selector.SelectedPawns.Where(IsValidSchedulable).ToList();
        }
    }
}
