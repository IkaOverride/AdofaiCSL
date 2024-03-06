using HarmonyLib;

namespace AdofaiCSL.Patches
{
    public static class CurrentTileTracker
    {
        /// <summary>
        /// Custom tracker for the current tile.
        /// </summary>
        public static string TileKey = string.Empty;
    }

    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.SelectLevel))]
    internal static class TrackerUpdate
    {
        /// <summary>
        /// Update the tracker after a level is selected.
        /// </summary>
        /// <param name="__instance"><see cref="scnCLS"/>.</param>
        private static void Postfix(scnCLS __instance) => CurrentTileTracker.TileKey = __instance.levelToSelect ?? string.Empty;
    }

    [HarmonyPatch(typeof(scrController), nameof(scrController.QuitToMainMenu))]
    internal static class TrackerReset
    {
        /// <summary>
        /// Reset the custom tracker.
        /// </summary>
        private static void Postfix()
        {
            if (GCS.sceneToLoad == "scnLevelSelect")
                CurrentTileTracker.TileKey = string.Empty;
        }
    }
}
