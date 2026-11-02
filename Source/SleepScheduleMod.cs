using UnityEngine;
using Verse;

namespace SleepSchedulePresets
{
    public class SleepScheduleMod : Mod
    {
        public static SleepScheduleSettings Settings;

        public SleepScheduleMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SleepScheduleSettings>();
        }

        public override string SettingsCategory() => "SSP.ModTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("SSP.Settings.Intro".Translate());
            listing.GapLine();
            listing.Label("SSP.Settings.Multiplayer".Translate());
            listing.Gap();
            if (Current.Game != null)
            {
                if (listing.ButtonText("SSP.OpenManager".Translate()))
                    Find.WindowStack.Add(new UI.Dialog_ManagePresets());
            }
            else
            {
                Color previous = GUI.color;
                GUI.color = Color.gray;
                listing.Label("SSP.Settings.LoadGameHint".Translate());
                GUI.color = previous;
            }
            listing.End();
        }
    }
}
