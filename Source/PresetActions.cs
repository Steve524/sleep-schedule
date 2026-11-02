using System.Collections.Generic;
using Verse;

namespace SleepSchedulePresets
{
    // Only copied values cross the sync boundary. Never read settings or UI here.
    public static class PresetActions
    {
        private static bool blockedActionLogged;

        private static bool CanRunSharedAction()
        {
            if (!Compat.MultiplayerCompat.SharedActionsBlocked) return true;
            if (!blockedActionLogged)
            {
                blockedActionLogged = true;
                Log.Error("[Sleep Schedule Presets] Multiplayer sync registration failed; shared preset action refused because the session is multiplayer or its state could not be determined.");
            }
            return false;
        }

        public static void ApplyPreset(List<Pawn> pawns, string[] assignments)
        {
            if (!CanRunSharedAction()) return;
            if (assignments == null || assignments.Length != 24 || pawns == null) return;
            SchedulePreset data = new SchedulePreset(0, null) { assignments = new List<string>(assignments) };
            foreach (Pawn pawn in pawns)
                if (PresetManager.IsValidSchedulable(pawn)) data.ApplyTo(pawn);
        }

        public static void AddSavePreset(string name, string[] assignments)
        {
            if (!CanRunSharedAction()) return;
            if (assignments == null || assignments.Length != 24) return;
            SleepScheduleGameComponent comp = SleepScheduleGameComponent.Instance;
            if (comp == null) return;
            SchedulePreset preset = new SchedulePreset(comp.NextSaveId(), name)
                { assignments = new List<string>(assignments) };
            preset.Ensure24();
            comp.savePresets.Add(preset);
        }

        public static void UpdateSavePreset(int id, string name, string[] assignments)
        {
            if (!CanRunSharedAction()) return;
            if (assignments == null || assignments.Length != 24) return;
            SchedulePreset preset = PresetManager.FindSavePreset(id);
            if (preset == null) return;
            preset.name = name;
            preset.assignments = new List<string>(assignments);
            preset.Ensure24();
        }

        public static void DeleteSavePreset(int id)
        {
            if (!CanRunSharedAction()) return;
            SleepScheduleGameComponent comp = SleepScheduleGameComponent.Instance;
            if (comp != null) comp.savePresets.RemoveAll(p => p != null && p.id == id);
        }

        public static void SetDefaultForNewPawns(string name, string[] assignments)
        {
            if (!CanRunSharedAction()) return;
            if (assignments == null || assignments.Length != 24) return;
            SleepScheduleGameComponent comp = SleepScheduleGameComponent.Instance;
            if (comp == null) return;
            SchedulePreset preset = new SchedulePreset(0, name) { assignments = new List<string>(assignments) };
            preset.Ensure24();
            comp.defaultForNewPawns = preset;
        }

        public static void ClearDefaultForNewPawns()
        {
            if (!CanRunSharedAction()) return;
            SleepScheduleGameComponent comp = SleepScheduleGameComponent.Instance;
            if (comp != null) comp.defaultForNewPawns = null;
        }
    }
}
