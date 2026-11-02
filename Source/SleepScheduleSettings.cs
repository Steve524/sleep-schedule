using System.Collections.Generic;
using Verse;

namespace SleepSchedulePresets
{
    public class SleepScheduleSettings : ModSettings
    {
        public List<SchedulePreset> globalPresets = new List<SchedulePreset>();
        public int nextGlobalId = 1;
        public bool builtInsSeeded = false;

        public void RepairCounter()
        {
            if (globalPresets == null) globalPresets = new List<SchedulePreset>();
            int max = 0;
            foreach (SchedulePreset preset in globalPresets)
                if (preset != null && preset.id > max) max = preset.id;
            if (nextGlobalId <= max) nextGlobalId = max + 1;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref globalPresets, "globalPresets", LookMode.Deep);
            Scribe_Values.Look(ref nextGlobalId, "nextGlobalId", 1);
            Scribe_Values.Look(ref builtInsSeeded, "builtInsSeeded", false);
            if (globalPresets == null) globalPresets = new List<SchedulePreset>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit) RepairCounter();
        }
    }
}
