using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace SleepSchedulePresets.UI
{
    public class Dialog_EditPreset : Window
    {
        private readonly PresetTier tier;
        private readonly Dialog_ManagePresets parent;
        private readonly bool isNew;
        private readonly SchedulePreset original;
        private readonly SchedulePreset working;
        private string nameBuffer;
        private bool painting;
        private string paintDefName;
        private int lastPaintedHour = -1;

        private const float HeaderHeight = 34f;
        private const float NameRowHeight = 30f;
        private const float ToolbarHeight = 30f;
        private const float BottomHeight = 38f;

        public Dialog_EditPreset(PresetTier tier, SchedulePreset preset, Dialog_ManagePresets parent, bool isNew = false)
        {
            this.tier = tier;
            this.parent = parent;
            this.isNew = isNew;
            original = preset;
            working = isNew ? preset : preset.CloneWith(preset.id, preset.name);
            nameBuffer = working.name ?? string.Empty;
            forcePause = false;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            draggable = true;
        }

        public override Vector2 InitialSize => new Vector2(720f, 420f);

        public override void DoWindowContents(Rect inRect)
        {
            // Release anywhere, including outside the cells. An unrelated mouse-up stays available.
            if (Event.current.type == EventType.MouseUp && painting)
            {
                painting = false;
                Event.current.Use();
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight),
                isNew ? "SSP.Edit.TitleNew".Translate() : "SSP.Edit.Title".Translate());
            Text.Font = GameFont.Small;
            float y = inRect.y + HeaderHeight + 4f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(inRect.x, y, 60f, NameRowHeight), "SSP.Edit.Name".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            nameBuffer = Widgets.TextField(new Rect(inRect.x + 64f, y, inRect.width - 64f, NameRowHeight), nameBuffer);
            y += NameRowHeight + 6f;

            DrawToolbar(new Rect(inRect.x, y, inRect.width, ToolbarHeight));
            y += ToolbarHeight + 6f;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 22f), "SSP.Edit.PaintHint".Translate());
            y += 22f;
            // Vanilla intentionally overflows 191x65 into a 477.5x32 footprint (Royalty).
            TimeAssignmentSelector.DrawTimeAssignmentSelectorGrid(new Rect(inRect.x, y, 191f, 65f));
            y += 36f;

            float pitch = Mathf.Floor(inRect.width / 24f);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            for (int h = 0; h < 24; h++)
                Widgets.Label(new Rect(inRect.x + h * pitch, y, pitch - 2f, 18f), h.ToString());
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            y += 18f;
            DrawPaintGrid(new Rect(inRect.x, y, inRect.width, 48f));
            y += 48f;

            int sleep = working.CountAssignment("Sleep");
            Color previous = GUI.color;
            if (sleep < 6) GUI.color = new Color(0.95f, 0.7f, 0.35f);
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 22f), sleep < 6
                ? "SSP.Edit.LowSleep".Translate(sleep).ToString() : working.SummaryText());
            GUI.color = previous;

            Rect bottom = new Rect(inRect.x, inRect.yMax - BottomHeight, inRect.width, BottomHeight);
            float width = (bottom.width - 10f) / 2f;
            if (Widgets.ButtonText(new Rect(bottom.x, bottom.y, width, bottom.height), "SSP.Cancel".Translate())) Close();
            if (Widgets.ButtonText(new Rect(bottom.x + width + 10f, bottom.y, width, bottom.height), "SSP.Save".Translate()))
            {
                if (tier == PresetTier.Save && PresetUIUtility.RefuseIfBlocked()) return;
                Commit();
                Close();
            }
        }

        private void DrawToolbar(Rect rect)
        {
            List<Pawn> pawns = PresetManager.SelectedSchedulables();
            float width = (rect.width - 16f) / 3f;
            Rect capture = new Rect(rect.x, rect.y, width, rect.height);
            bool canCapture = pawns.Count > 0;
            Color previous = GUI.color;
            if (!canCapture) GUI.color = new Color(1f, 1f, 1f, 0.4f);
            if (Widgets.ButtonText(capture, "SSP.Edit.Capture".Translate(), active: canCapture) && canCapture)
                working.CopyAssignmentsFrom(SchedulePreset.CaptureFrom(pawns[0], 0, working.name));
            GUI.color = previous;
            if (canCapture) TooltipHandler.TipRegion(capture, "SSP.Edit.Capture.Tip".Translate(pawns[0].LabelShortCap));

            Rect reset = new Rect(capture.xMax + 8f, rect.y, width, rect.height);
            if (Widgets.ButtonText(reset, "SSP.Edit.ResetVanilla".Translate()))
                working.assignments = new List<string>(PresetManager.VanillaAssignments());
            Rect fill = new Rect(reset.xMax + 8f, rect.y, width, rect.height);
            if (Widgets.ButtonText(fill, "SSP.Edit.FillAll".Translate()))
            {
                string value = PresetManager.ResolveAssignment(TimeAssignmentSelector.selectedAssignment?.defName).defName;
                for (int h = 0; h < 24; h++) working.SetAssignment(h, value);
            }
            TooltipHandler.TipRegion(fill, "SSP.Edit.FillAll.Tip".Translate());
        }

        private void DrawPaintGrid(Rect rect)
        {
            float pitch = Mathf.Floor(rect.width / 24f);
            int hoveredHour = -1;
            for (int h = 0; h < 24; h++)
            {
                Rect cell = new Rect(rect.x + h * pitch, rect.y, pitch - 2f, 48f);
                PresetUIUtility.DrawAssignmentCell(cell, working.GetAssignmentDefName(h), h, true);
                if (cell.Contains(Event.current.mousePosition)) hoveredHour = h;
            }

            Event ev = Event.current;
            if (ev.button != 0) return;
            if (ev.type == EventType.MouseDown && hoveredHour >= 0)
            {
                paintDefName = PresetManager.ResolveAssignment(TimeAssignmentSelector.selectedAssignment?.defName).defName;
                working.SetAssignment(hoveredHour, paintDefName);
                painting = true;
                lastPaintedHour = hoveredHour;
                ev.Use();
            }
            else if (ev.type == EventType.MouseDrag && painting)
            {
                if (hoveredHour >= 0 && hoveredHour != lastPaintedHour)
                {
                    working.SetAssignment(hoveredHour, paintDefName);
                    lastPaintedHour = hoveredHour;
                }
                ev.Use();
            }
        }

        private void Commit()
        {
            string finalName = string.IsNullOrWhiteSpace(nameBuffer)
                ? "SSP.NewPresetName".Translate().ToString() : nameBuffer.Trim();
            working.name = finalName;
            if (tier == PresetTier.Global)
            {
                if (isNew)
                {
                    working.id = PresetManager.NextGlobalId();
                    PresetManager.AddGlobalPreset(working);
                    parent?.SelectPreset(PresetTier.Global, working);
                }
                else
                {
                    original.name = finalName;
                    original.CopyAssignmentsFrom(working);
                    PresetManager.SaveGlobal();
                    parent?.SelectPreset(PresetTier.Global, original);
                }
            }
            else
            {
                if (isNew) PresetActions.AddSavePreset(finalName, working.ToArray());
                else PresetActions.UpdateSavePreset(original.id, finalName, working.ToArray());
            }
        }
    }
}
