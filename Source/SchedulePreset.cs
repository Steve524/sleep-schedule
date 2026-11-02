using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SleepSchedulePresets
{
    // Persist names, never Def references: unavailable assignments survive DLC/mod removal.
    public class SchedulePreset : IExposable
    {
        public int id;
        public string name = "Preset";
        public List<string> assignments = new List<string>(PresetManager.VanillaAssignments());

        public SchedulePreset() { }
        public SchedulePreset(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public string GetAssignmentDefName(int hour)
        {
            if (hour < 0 || hour >= 24 || assignments == null || hour >= assignments.Count)
                return "Anything";
            return string.IsNullOrEmpty(assignments[hour]) ? "Anything" : assignments[hour];
        }

        public void SetAssignment(int hour, string defName)
        {
            if (hour < 0 || hour >= 24) return;
            Ensure24();
            assignments[hour] = string.IsNullOrEmpty(defName) ? "Anything" : defName;
        }

        public void ApplyTo(Pawn pawn)
        {
            if (!PresetManager.IsValidSchedulable(pawn)) return;
            ApplyToTracker(pawn.timetable);
        }

        // The ctor postfix must use its new tracker, before pawn.timetable is assigned.
        public void ApplyToTracker(Pawn_TimetableTracker tracker)
        {
            if (tracker == null) return;
            for (int hour = 0; hour < 24; hour++)
                tracker.SetAssignment(hour, PresetManager.ResolveAssignment(GetAssignmentDefName(hour)));
        }

        public static SchedulePreset CaptureFrom(Pawn pawn, int id, string name)
        {
            SchedulePreset preset = new SchedulePreset(id, name);
            if (pawn?.timetable == null) return preset;
            for (int hour = 0; hour < 24; hour++)
                preset.SetAssignment(hour, pawn.timetable.times[hour]?.defName ?? "Anything");
            return preset;
        }

        public static SchedulePreset Baseline(int id, string name)
        {
            return new SchedulePreset(id, name);
        }

        public SchedulePreset CloneWith(int newId, string newName)
        {
            return new SchedulePreset(newId, newName) { assignments = new List<string>(ToArray()) };
        }

        public void CopyAssignmentsFrom(SchedulePreset other)
        {
            assignments = new List<string>(other.ToArray());
        }

        public string[] ToArray()
        {
            string[] result = new string[24];
            for (int hour = 0; hour < 24; hour++) result[hour] = GetAssignmentDefName(hour);
            return result;
        }

        public int CountAssignment(string defName)
        {
            int count = 0;
            for (int hour = 0; hour < 24; hour++)
                if (PresetManager.ResolveAssignment(GetAssignmentDefName(hour)).defName == defName) count++;
            return count;
        }

        public string SummaryText()
        {
            int sleep = CountAssignment("Sleep");
            int work = CountAssignment("Work");
            int joy = CountAssignment("Joy");
            int meditate = CountAssignment("Meditate");
            return meditate > 0
                ? "SSP.PresetSummary.Meditate".Translate(sleep, work, joy, meditate).ToString()
                : "SSP.PresetSummary".Translate(sleep, work, joy).ToString();
        }

        public void Ensure24()
        {
            if (assignments == null) assignments = new List<string>();
            while (assignments.Count < 24) assignments.Add("Anything");
            if (assignments.Count > 24) assignments.RemoveRange(24, assignments.Count - 24);
            for (int hour = 0; hour < 24; hour++)
                if (string.IsNullOrEmpty(assignments[hour])) assignments[hour] = "Anything";
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id", 0);
            Scribe_Values.Look(ref name, "name", "Preset");
            Scribe_Collections.Look(ref assignments, "assignments", LookMode.Value);
            Ensure24();
        }
    }
}
