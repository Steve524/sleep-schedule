using System.Reflection;
using HarmonyLib;
using Verse;

namespace SleepSchedulePresets
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        public const string HarmonyId = "steven.sleepschedule";

        static Startup()
        {
            new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
            SeedBuiltIns();
            Log.Message("[Sleep Schedule Presets] Initialized.");
        }

        private static void SeedBuiltIns()
        {
            SleepScheduleSettings settings = SleepScheduleMod.Settings;
            if (settings == null) return;
            settings.RepairCounter();
            if (settings.builtInsSeeded) return;
            string[] keys = { "VanillaDay", "DayWorker", "NightOwl", "NightShift", "Biphasic", "Child", "Psycaster" };
            foreach (string key in keys)
            {
                SchedulePreset preset = SchedulePreset.Baseline(PresetManager.NextGlobalId(), ("SSP.Builtin." + key).Translate());
                for (int h = 0; h < 24; h++)
                {
                    string value = preset.GetAssignmentDefName(h);
                    switch (key)
                    {
                        case "DayWorker":
                            if (h >= 7 && h <= 16) value = "Work";
                            else if (h >= 17 && h <= 21) value = "Joy";
                            break;
                        case "NightOwl": value = h >= 6 && h <= 13 ? "Sleep" : "Anything"; break;
                        case "NightShift":
                            value = h >= 20 || h <= 3 ? "Work"
                                : h >= 8 && h <= 15 ? "Sleep"
                                : h >= 16 && h <= 18 ? "Joy" : "Anything";
                            break;
                        case "Biphasic": value = h <= 4 || (h >= 13 && h <= 15) ? "Sleep" : "Anything"; break;
                        case "Child":
                            value = h >= 20 || h <= 6 ? "Sleep"
                                : (h >= 8 && h <= 11) || (h >= 13 && h <= 16) ? "Joy" : "Anything";
                            break;
                        case "Psycaster": if (h >= 6 && h <= 8) value = "Meditate"; break;
                    }
                    preset.SetAssignment(h, value);
                }
                settings.globalPresets.Add(preset);
            }
            settings.builtInsSeeded = true;
            settings.Write();
        }
    }
}
