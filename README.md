# Sleep Schedule

Save named 24-hour schedules and apply them to colonists in RimWorld 1.6.
Multiplayer integration uses deterministic commands; evidence so far is static review and compilation only.

---

## What it does

A **preset** stores all 24 hourly assignments. Apply it to one colonist or the whole selection.

Two kinds of presets:

- **My library (Global)**: your personal presets, stored in mod settings. They follow you across colonies, save files, and game restarts. Changes are saved immediately.
- **This colony (Save)**: presets stored inside the current save. Shared by everyone playing that colony and synced in Multiplayer.

Seven starters appear on first launch: **Vanilla Day**, **Day Worker**, **Night Owl**, **Night Shift**, **Biphasic**, **Child**, and **Psycaster**. Edit or delete them freely; deleted starters do not reappear on restart. Psycaster is included even without Royalty, so enabling the DLC later restores its meditation hours.

---

## How to use it

Open the manager from **Presets** at the top-right of the **Schedule** tab, or the mod settings page while a game is loaded.

- **New…**: start from the vanilla schedule, or capture the first selected colonist's current hours.
- **Edit**: rename, choose an assignment, then click or drag over the hours. Copy from a colonist, reset to vanilla, or fill all hours with the selected assignment. Cancel or close-X discards edits. A blank name becomes “New preset”.
- **Duplicate / Delete**: manage either tier. Delete asks for confirmation.
- **Apply to selected (N)**: apply all 24 hours to the selected colonists. Children and secure colony slaves are eligible; prisoners, animals, mechs, and subhuman pawns are not.
- **Set as new-pawn default**: store a snapshot for this colony. Recruits/joiners receive it when their timetable is first created during play. Existing timetables and starting colonists are unchanged. Editing or deleting the source preset leaves the snapshot intact. **Clear new-pawn default** restores vanilla behavior for future creations.

For quick apply, select colonists and use the **Schedule preset** gizmo with the vanilla Paste icon. The menu lists My library first, then This colony, and ends with the manager.

The editor uses vanilla's shared assignment selector: picking paint also changes the Schedule tab's selected assignment. A warning below six sleep hours is advisory; saving stays available. At extreme UI scales, the Schedule-tab button hides to avoid the vanilla selector; use the gizmo or settings instead. Settings contain no options.

An **orange outline** marks an unavailable assignment. It displays and applies as **Anything**, but its stored name stays intact. This includes Meditate without Royalty and assignments from removed mods. Re-enabling the DLC/mod restores those preset hours. Opening and saving an editor preserves untouched unavailable hours; painting over them replaces them.

---

## Requirements

- RimWorld **1.6** (compiled against installed **1.6.4871**).
- **Harmony** (`brrainz.harmony`), loaded before this mod.
- **Royalty** is optional. Meditation is available in the selector only with Royalty.
- **Multiplayer** (`rwmt.Multiplayer`) is optional; load this mod after it when present.

---

## Multiplayer / determinism notes

- Six static actions cover applying schedules, colony preset add/update/delete, and setting/clearing the new-pawn default. Each registers as a Multiplayer sync method. The UI sends copied 24-hour name arrays and real pawn targets; receiving clients never look up a personal-library preset.
- Global library edits remain local gameplay operations. Multiplayer's optional **host-config transfer** can copy settings files to joining clients and redirect settings to session configs. A joining client may therefore start with the host's library copy. No action requires libraries to match, and this mod does not change MP's config-ignore list.
- Save IDs come from a shared counter inside the synced add action. Hours are ordered lists/arrays; no randomness or dictionary order affects simulation.
- The new-pawn hook runs during normal simulation and reads only the shared colony default and assignment defs. It applies to the newly constructed tracker, skips loading/setup, and does not queue another apply command.
- All `Multiplayer.API` calls stay in non-inlined methods in the optional compatibility class. The rest of the assembly has no API attributes or API-bearing signatures.
- Multiplayer evidence is **static only**. A two-client runtime/desync test has not been performed.

**Core mods:** Work Priority Presets uses separate stores and the Work tab, so both managers can coexist. RimHUD also edits pawn timetables: **last write wins** for each affected hour. Changing an hour in RimHUD after an apply overrides that hour; applying again replaces all 24. Allow Tool and Smarter Construction need no special configuration.

**Adding/removing:** safe to add mid-save; the colony tier starts empty and the default is off. Removing the mod leaves the last applied vanilla pawn schedules intact. On loading an old save containing this mod's component, vanilla is expected to log two missing-component errors, discard that data, and continue loading. Saving with the mod absent removes the orphan node; later loads of that new save should have no such errors. Colony presets and default are then lost. The personal library remains in the separate mod settings file and returns when re-enabled under the same folder/class names. Removing DLC/custom assignment defs can separately affect vanilla's pawn timetable loading.

---

## Building from source

Close RimWorld first. `Source/SleepSchedulePresets.csproj` is SDK-style **net472**, C# **9.0**, and references the game's own Mono/Unity DLLs plus local Harmony and Multiplayer API. No NuGet packages or copied dependency DLLs are needed.

With a .NET SDK available, from `Source/`:

```powershell
dotnet build -c Release
```

On this machine, MSBuild could not resolve `Microsoft.NET.Sdk`. The successful route is VS BuildTools' Roslyn compiler. From the mod root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Source\build.ps1
```

The script reads the exact reference list from the project, prints its full compiler command, and writes `Assemblies/SleepSchedulePresets.dll`. For another install, pass `-RimWorldManaged '<game>\RimWorldWin64_Data\Managed'` and optionally `-BuildTools '<VS BuildTools folder>'`. Harmony and MP paths derive from the managed folder. For SDK builds, adjust the corresponding project properties or pass MSBuild properties.

---

## Key files

| File | Purpose |
|------|---------|
| `About/About.xml` | Metadata, `steven.sleepschedule`, Harmony dependency, MP load order. |
| `LoadFolders.xml` | Root content for RimWorld 1.6. |
| `Assemblies/SleepSchedulePresets.dll` | Compiled mod. |
| `Source/SchedulePreset.cs` | Ordered hourly names, capture/apply/copy, normalization and serialization. |
| `Source/SleepScheduleSettings.cs` | Global library, seeding latch and counter repair. |
| `Source/SleepScheduleGameComponent.cs` | Colony presets, default snapshot and counter repair. |
| `Source/PresetManager.cs` | Assignment resolution, eligibility and store access. |
| `Source/PresetActions.cs` | Six game-state actions, synced when MP is present. |
| `Source/Compat/MultiplayerCompat.cs` | Optional API detection and registration. |
| `Source/Startup.cs` | Harmony patches and seven starter presets. |
| `Source/Patches/*` | Schedule button, pawn gizmo and new-tracker default hook. |
| `Source/UI/*` | Manager, paint editor and shared preview helpers. |
| `Source/build.ps1` | Direct Roslyn rebuild using project references. |
| `Languages/English/Keyed/*` | All translated UI text. |
