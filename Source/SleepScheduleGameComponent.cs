using System.Collections.Generic;
using Verse;

namespace SleepSchedulePresets
{
    public class SleepScheduleGameComponent : GameComponent
    {
        public List<SchedulePreset> savePresets = new List<SchedulePreset>();
        public int nextSaveId = 1;
        public SchedulePreset defaultForNewPawns;

        public SleepScheduleGameComponent(Game game) { }

        public static SleepScheduleGameComponent Instance =>
            Current.Game?.GetComponent<SleepScheduleGameComponent>();

        public int NextSaveId() { return nextSaveId++; }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref savePresets, "savePresets", LookMode.Deep);
            Scribe_Values.Look(ref nextSaveId, "nextSaveId", 1);
            Scribe_Deep.Look(ref defaultForNewPawns, "defaultForNewPawns");
            if (savePresets == null) savePresets = new List<SchedulePreset>();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                int max = 0;
                foreach (SchedulePreset preset in savePresets)
                    if (preset != null && preset.id > max) max = preset.id;
                if (nextSaveId <= max) nextSaveId = max + 1;
            }
        }
    }
}
