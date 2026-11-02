using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SleepSchedulePresets.UI
{
    [StaticConstructorOnStartup]
    public static class PresetUIUtility
    {
        public static readonly Texture2D GizmoIcon = TexButton.Paste;

        public static bool RefuseIfBlocked()
        {
            if (!Compat.MultiplayerCompat.SharedActionsBlocked) return false;
            Messages.Message("SSP.MpSyncBlocked".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return true;
        }

        public static void ApplyToPawns(SchedulePreset preset, List<Pawn> pawns)
        {
            if (preset == null || pawns == null) return;
            List<Pawn> targets = pawns.Where(PresetManager.IsValidSchedulable).ToList();
            if (targets.Count == 0) return;
            if (RefuseIfBlocked()) return;
            PresetActions.ApplyPreset(targets, preset.ToArray());
            Messages.Message("SSP.Applied".Translate(preset.name, targets.Count), MessageTypeDefOf.SilentInput, historical: false);
        }

        public static Command_Action CreateApplyGizmo(Pawn pawn)
        {
            return new Command_Action
            {
                defaultLabel = "SSP.Gizmo.Label".Translate(),
                defaultDesc = "SSP.Gizmo.Desc".Translate(),
                icon = GizmoIcon,
                action = delegate
                {
                    List<Pawn> targets = PresetManager.SelectedSchedulables();
                    if (targets.Count == 0) targets = new List<Pawn> { pawn };
                    Find.WindowStack.Add(BuildApplyFloatMenu(targets));
                }
            };
        }

        public static FloatMenu BuildApplyFloatMenu(List<Pawn> pawns)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (SchedulePreset preset in PresetManager.GlobalPresets)
            {
                SchedulePreset local = preset;
                options.Add(new FloatMenuOption("SSP.MenuEntry".Translate(local.name, "SSP.Tier.Global".Translate()),
                    delegate { ApplyToPawns(local, pawns); }));
            }
            if (PresetManager.SaveTierAvailable)
            {
                foreach (SchedulePreset preset in PresetManager.SavePresets)
                {
                    SchedulePreset local = preset;
                    options.Add(new FloatMenuOption("SSP.MenuEntry".Translate(local.name, "SSP.Tier.Save".Translate()),
                        delegate { ApplyToPawns(local, pawns); }));
                }
            }
            if (options.Count == 0) options.Add(new FloatMenuOption("SSP.NoPresets".Translate(), null));
            options.Add(new FloatMenuOption("SSP.OpenManager".Translate(),
                delegate { Find.WindowStack.Add(new Dialog_ManagePresets()); }));
            return new FloatMenu(options);
        }

        // Shared rendering: fallback is display-only; the stored string is untouched.
        public static void DrawAssignmentCell(Rect cell, string defName, int hour, bool interactivePreview)
        {
            TimeAssignmentDef def = PresetManager.ResolveAssignment(defName);
            GUI.DrawTexture(cell, def.ColorTexture);
            if (interactivePreview)
            {
                if (Mouse.IsOver(cell)) Widgets.DrawHighlight(cell);
                TooltipHandler.TipRegion(cell, "SSP.Cell.Tip".Translate($"{hour}:00", def.LabelCap));
            }
            if (PresetManager.IsUnresolvable(defName))
            {
                Color previous = GUI.color;
                GUI.color = new Color(0.95f, 0.55f, 0.2f);
                Widgets.DrawBox(cell, 1);
                GUI.color = previous;
            }
        }

        public static void DrawMiniStrip(Rect rect, SchedulePreset preset)
        {
            for (int h = 0; h < 24; h++)
                DrawAssignmentCell(new Rect(rect.x + h * 11f, rect.y, 10f, 14f), preset.GetAssignmentDefName(h), h, false);
        }

        public static void DrawPresetPreview(Rect rect, SchedulePreset preset)
        {
            if (preset == null)
            {
                GUI.color = Color.gray;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "SSP.SelectPresetHint".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                return;
            }

            Rect inner = rect.ContractedBy(8f);
            float pitch = Mathf.Floor(inner.width / 24f);
            for (int h = 0; h < 24; h++)
            {
                Rect cell = new Rect(inner.x + h * pitch, inner.y, pitch - 2f, 44f);
                DrawAssignmentCell(cell, preset.GetAssignmentDefName(h), h, true);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(cell.x, cell.yMax, cell.width, 14f), h.ToString());
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(new Rect(inner.x, inner.y + 58f, inner.width, 22f), preset.SummaryText());

            List<TimeAssignmentDef> defs = PresetManager.AllAssignments;
            float slotWidth = defs.Count == 0 ? inner.width : inner.width / defs.Count;
            for (int i = 0; i < defs.Count; i++)
            {
                float x = inner.x + i * slotWidth;
                GUI.DrawTexture(new Rect(x, inner.y + 84f, 14f, 14f), defs[i].ColorTexture);
                Widgets.Label(new Rect(x + 18f, inner.y + 80f, slotWidth - 26f, 22f), defs[i].LabelCap);
            }
        }
    }
}
