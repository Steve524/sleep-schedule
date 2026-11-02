using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SleepSchedulePresets.UI
{
    public class Dialog_ManagePresets : Window
    {
        private PresetTier tier = PresetTier.Global;
        private SchedulePreset selectedGlobal;
        private SchedulePreset selectedSave;

        private Vector2 listScroll;

        private const float HeaderHeight = 34f;
        private const float TabHeight = 32f;
        private const float ListWidth = 320f;
        private const float BottomButtonHeight = 34f;
        private const float RowHeight = 64f;
        private const float Pad = 10f;

        public Dialog_ManagePresets()
        {
            forcePause = false;
            doCloseX = true;
            doCloseButton = false;
            absorbInputAroundWindow = false;
            closeOnClickedOutside = false;
            draggable = true;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(920f, 700f);

        private SchedulePreset Selected
        {
            get { return tier == PresetTier.Global ? selectedGlobal : selectedSave; }
            set
            {
                if (tier == PresetTier.Global) selectedGlobal = value;
                else selectedSave = value;
            }
        }

        private List<SchedulePreset> CurrentList =>
            tier == PresetTier.Global ? PresetManager.GlobalPresets : PresetManager.SavePresets;

        public override void DoWindowContents(Rect inRect)
        {
            if (Selected != null && !CurrentList.Contains(Selected)) Selected = null;

            // Header
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, "SSP.ModTitle".Translate());
            Text.Font = GameFont.Small;

            float curY = inRect.y + HeaderHeight + 4f;

            // Tabs
            DrawTierTabs(new Rect(inRect.x, curY, inRect.width, TabHeight));
            curY += TabHeight + 6f;

            float bodyHeight = inRect.yMax - curY - Pad;
            Rect listRect = new Rect(inRect.x, curY, ListWidth, bodyHeight);
            Rect rightRect = new Rect(listRect.xMax + Pad, curY, inRect.xMax - listRect.xMax - Pad, bodyHeight);

            DrawListColumn(listRect);
            DrawDetailColumn(rightRect);
        }

        private void DrawTierTabs(Rect rect)
        {
            float half = rect.width / 2f;
            Rect globalTab = new Rect(rect.x, rect.y, half - 2f, rect.height);
            Rect saveTab = new Rect(rect.x + half + 2f, rect.y, half - 2f, rect.height);

            if (DrawTabButton(globalTab, "SSP.Tab.Global".Translate(PresetManager.GlobalPresets.Count), tier == PresetTier.Global))
            {
                tier = PresetTier.Global;
            }

            string saveLabel = PresetManager.SaveTierAvailable
                ? "SSP.Tab.Save".Translate(PresetManager.SavePresets.Count)
                : "SSP.Tab.SaveUnavailable".Translate();
            bool saveActive = tier == PresetTier.Save;
            if (DrawTabButton(saveTab, saveLabel, saveActive) && PresetManager.SaveTierAvailable)
            {
                tier = PresetTier.Save;
            }
            TooltipHandler.TipRegion(globalTab, "SSP.Tab.Global.Tip".Translate());
            TooltipHandler.TipRegion(saveTab, "SSP.Tab.Save.Tip".Translate());
        }

        private bool DrawTabButton(Rect rect, string label, bool active)
        {
            Color prev = GUI.color;
            if (active) GUI.color = new Color(0.55f, 0.75f, 1f);
            bool clicked = Widgets.ButtonText(rect, label);
            GUI.color = prev;
            return clicked;
        }

        /// <summary>A button that greys out and ignores clicks when not enabled.</summary>
        private bool DrawButton(Rect rect, string label, bool enabled)
        {
            if (!enabled)
            {
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.4f);
                Widgets.ButtonText(rect, label, active: false);
                GUI.color = prev;
                return false;
            }
            return Widgets.ButtonText(rect, label);
        }

        public void SelectPreset(PresetTier t, SchedulePreset preset)
        {
            tier = t;
            if (t == PresetTier.Global) selectedGlobal = preset;
            else selectedSave = preset;
        }

        private void DrawListColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(4f);

            Rect buttonsRect = new Rect(inner.x, inner.yMax - BottomButtonHeight, inner.width, BottomButtonHeight);
            Rect scrollOuter = new Rect(inner.x, inner.y, inner.width, inner.height - BottomButtonHeight - 4f);

            List<SchedulePreset> list = CurrentList;
            Rect viewRect = new Rect(0f, 0f, scrollOuter.width - 16f, list.Count * (RowHeight + 2f));
            Widgets.BeginScrollView(scrollOuter, ref listScroll, viewRect);
            float y = 0f;
            foreach (SchedulePreset preset in list)
            {
                Rect row = new Rect(0f, y, viewRect.width, RowHeight);
                if (Selected == preset) Widgets.DrawHighlightSelected(row);
                else if (Mouse.IsOver(row)) Widgets.DrawHighlight(row);

                Rect nameRect = new Rect(row.x + 6f, row.y + 3f, row.width - 12f, 20f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, preset.name);

                PresetUIUtility.DrawMiniStrip(new Rect(row.x + 6f, row.y + 25f, 264f, 14f), preset);

                Rect subRect = new Rect(row.x + 6f, row.y + 42f, row.width - 12f, 18f);
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                Text.Font = GameFont.Tiny;
                Widgets.Label(subRect, preset.SummaryText());
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonInvisible(row))
                {
                    Selected = preset;
                }
                y += RowHeight + 2f;
            }
            Widgets.EndScrollView();
            if (list.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.gray;
                Widgets.Label(scrollOuter, "SSP.EmptyList".Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            DrawListButtons(buttonsRect);
        }

        private void DrawListButtons(Rect rect)
        {
            float w = (rect.width - 8f) / 3f;
            Rect newRect = new Rect(rect.x, rect.y, w, rect.height);
            Rect dupRect = new Rect(newRect.xMax + 4f, rect.y, w, rect.height);
            Rect delRect = new Rect(dupRect.xMax + 4f, rect.y, w, rect.height);

            if (Widgets.ButtonText(newRect, "SSP.New".Translate()))
            {
                OpenNewMenu();
            }

            bool hasSel = Selected != null;
            if (DrawButton(dupRect, "SSP.Duplicate".Translate(), hasSel))
            {
                DuplicateSelected();
            }
            if (DrawButton(delRect, "SSP.Delete".Translate(), hasSel))
            {
                DeleteSelected();
            }
        }

        private void DrawDetailColumn(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(8f);

            SchedulePreset preset = Selected;

            // Preset name header
            Rect titleRect = new Rect(inner.x, inner.y, inner.width, 28f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, preset != null ? preset.name : "SSP.NoSelection".Translate().ToString());
            Text.Font = GameFont.Small;

            SleepScheduleGameComponent comp = SleepScheduleGameComponent.Instance;
            bool hasDefault = comp?.defaultForNewPawns != null;
            float lineY = titleRect.yMax + 2f;
            Rect defRect = new Rect(inner.x, lineY, inner.width - (hasDefault ? 176f : 0f), 20f);
            GUI.color = hasDefault ? new Color(0.6f, 0.85f, 0.6f) : new Color(0.7f, 0.7f, 0.7f);
            Widgets.Label(defRect, hasDefault
                ? "SSP.CurrentDefault".Translate(comp.defaultForNewPawns.name)
                : "SSP.CurrentDefault.None".Translate());
            GUI.color = Color.white;
            if (hasDefault && Widgets.ButtonText(new Rect(inner.xMax - 170f, lineY - 2f, 170f, 24f),
                "SSP.ClearDefault".Translate()) && !PresetUIUtility.RefuseIfBlocked())
            {
                PresetActions.ClearDefaultForNewPawns();
                Messages.Message("SSP.DefaultCleared".Translate(), MessageTypeDefOf.SilentInput, false);
            }

            float bottomY = inner.yMax - BottomButtonHeight;
            Rect previewRect = new Rect(inner.x, titleRect.yMax + 24f, inner.width, bottomY - titleRect.yMax - 30f);
            Widgets.DrawBoxSolid(previewRect, new Color(0f, 0f, 0f, 0.15f));
            PresetUIUtility.DrawPresetPreview(previewRect, preset);

            DrawDetailButtons(new Rect(inner.x, bottomY, inner.width, BottomButtonHeight), preset);
        }

        private void DrawDetailButtons(Rect rect, SchedulePreset preset)
        {
            bool hasSel = preset != null;
            List<Pawn> selectedWorkers = PresetManager.SelectedSchedulables();

            float w = (rect.width - 12f) / 3f;
            Rect editRect = new Rect(rect.x, rect.y, w, rect.height);
            Rect applyRect = new Rect(editRect.xMax + 6f, rect.y, w, rect.height);
            Rect defaultRect = new Rect(applyRect.xMax + 6f, rect.y, w, rect.height);

            if (DrawButton(editRect, "SSP.Edit".Translate(), hasSel))
            {
                Find.WindowStack.Add(new Dialog_EditPreset(tier, preset, this));
            }

            string applyLabel = "SSP.ApplyToSelected".Translate(selectedWorkers.Count);
            if (DrawButton(applyRect, applyLabel, hasSel && selectedWorkers.Count > 0))
            {
                PresetUIUtility.ApplyToPawns(preset, selectedWorkers);
            }
            if (hasSel && selectedWorkers.Count == 0)
            {
                TooltipHandler.TipRegion(applyRect, "SSP.ApplyToSelected.None".Translate());
            }

            bool canSetDefault = hasSel && PresetManager.SaveTierAvailable;
            if (DrawButton(defaultRect, "SSP.SetDefault".Translate(), canSetDefault) && !PresetUIUtility.RefuseIfBlocked())
            {
                PresetActions.SetDefaultForNewPawns(preset.name, preset.ToArray());
                Messages.Message("SSP.DefaultSet".Translate(preset.name), MessageTypeDefOf.SilentInput, false);
            }
            if (!PresetManager.SaveTierAvailable)
            {
                TooltipHandler.TipRegion(defaultRect, "SSP.SetDefault.NeedGame".Translate());
            }
        }

        // ---------- Actions ----------

        private void OpenNewMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("SSP.New.FromScratch".Translate(), delegate
            {
                SchedulePreset data = SchedulePreset.Baseline(0, "SSP.NewPresetName".Translate());
                Find.WindowStack.Add(new Dialog_EditPreset(tier, data, this, isNew: true));
            }));

            List<Pawn> workers = PresetManager.SelectedSchedulables();
            if (workers.Count > 0)
            {
                Pawn source = workers[0];
                options.Add(new FloatMenuOption("SSP.New.FromColonist".Translate(source.LabelShortCap), delegate
                {
                    SchedulePreset data = SchedulePreset.CaptureFrom(source, 0,
                        "SSP.PresetFromName".Translate(source.LabelShortCap));
                    Find.WindowStack.Add(new Dialog_EditPreset(tier, data, this, isNew: true));
                }));
            }
            else
            {
                options.Add(new FloatMenuOption("SSP.New.FromColonist.None".Translate(), null));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void DuplicateSelected()
        {
            SchedulePreset preset = Selected;
            if (preset == null) return;
            string newName = "SSP.CopyName".Translate(preset.name);

            if (tier == PresetTier.Global)
            {
                SchedulePreset clone = preset.CloneWith(PresetManager.NextGlobalId(), newName);
                PresetManager.AddGlobalPreset(clone);
                selectedGlobal = clone;
            }
            else
            {
                if (PresetUIUtility.RefuseIfBlocked()) return;
                PresetActions.AddSavePreset(newName, preset.ToArray());
            }
        }

        private void DeleteSelected()
        {
            SchedulePreset preset = Selected;
            if (preset == null) return;

            PresetTier deleteTier = tier;
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "SSP.Delete.Confirm".Translate(preset.name),
                delegate
                {
                    if (deleteTier == PresetTier.Global)
                    {
                        PresetManager.DeleteGlobalPreset(preset);
                        selectedGlobal = null;
                    }
                    else
                    {
                        if (PresetUIUtility.RefuseIfBlocked()) return;
                        PresetActions.DeleteSavePreset(preset.id);
                        selectedSave = null;
                    }
                },
                destructive: true));
        }

    }
}
