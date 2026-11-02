using System;
using System.Runtime.CompilerServices;
using Verse;

namespace SleepSchedulePresets.Compat
{
    /// <summary>
    /// Optional integration with the RimWorld Multiplayer mod (rwmt.Multiplayer).
    ///
    /// Detection uses only the loaded-mod list — it touches NO Multiplayer.API type. Only after
    /// confirming MP is present do we call <see cref="WireUp"/> or probe the session state.
    /// Because the API type is never referenced from any always-JIT'd
    /// path, the mod loads perfectly fine when Multiplayer is not installed.
    ///
    /// We register the plain static action methods in <see cref="PresetActions"/> as sync methods.
    /// Callers everywhere just call PresetActions.* normally: in an MP session those calls turn
    /// into single deterministic commands replayed on all clients; in singleplayer they run inline.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MultiplayerCompat
    {
        private static bool detected;
        private static bool registered;

        public static bool Active => registered;

        /// <summary>True only when in an actual MP session. Never touches the API unless MP is loaded.</summary>
        public static bool InMultiplayer => detected && GetMultiplayerSessionState() == true;

        public static bool SharedActionsBlocked => detected && !registered && GetMultiplayerSessionState() != false;

        private static bool? GetMultiplayerSessionState()
        {
            try
            {
                return InMultiplayerInternal();
            }
            catch (Exception)
            {
                // Unknown session state must fail closed when sync registration failed.
                return null;
            }
        }

        static MultiplayerCompat()
        {
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                string id = mod.PackageId?.ToLowerInvariant() ?? string.Empty;
                if (id == "rwmt.multiplayer" || id == "rwmt.multiplayer_steam" || id.StartsWith("rwmt.multiplayer"))
                {
                    detected = true;
                    break;
                }
            }

            if (!detected) return;

            try
            {
                WireUp();
                registered = true;
                Log.Message("[Sleep Schedule Presets] Multiplayer detected — sync methods registered.");
            }
            catch (Exception e)
            {
                Log.Warning("[Sleep Schedule Presets] Failed to wire up Multiplayer compatibility, disabling it: " + e);
            }
        }

        // All Multiplayer.API IL is confined to this method (and InMultiplayerInternal below).
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WireUp()
        {
            Multiplayer.API.MP.RegisterSyncMethod(typeof(PresetActions), nameof(PresetActions.ApplyPreset));
            Multiplayer.API.MP.RegisterSyncMethod(typeof(PresetActions), nameof(PresetActions.AddSavePreset));
            Multiplayer.API.MP.RegisterSyncMethod(typeof(PresetActions), nameof(PresetActions.UpdateSavePreset));
            Multiplayer.API.MP.RegisterSyncMethod(typeof(PresetActions), nameof(PresetActions.DeleteSavePreset));
            Multiplayer.API.MP.RegisterSyncMethod(typeof(PresetActions), nameof(PresetActions.SetDefaultForNewPawns));
            Multiplayer.API.MP.RegisterSyncMethod(typeof(PresetActions), nameof(PresetActions.ClearDefaultForNewPawns));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool InMultiplayerInternal()
        {
            return Multiplayer.API.MP.IsInMultiplayer;
        }
    }
}
