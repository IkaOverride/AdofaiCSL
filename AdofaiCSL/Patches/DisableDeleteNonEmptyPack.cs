using ADOFAI;
using HarmonyLib;

namespace AdofaiCSL.Patches
{
    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.DeleteLevel))]
    internal static class DisableDeletePack
    {
        /// <summary>
        /// Disable deleting a pack with levels.
        /// </summary>
        private static bool Prefix(scnCLS __instance) => !(__instance.loadedLevels[__instance.levelToSelect] is FolderDataCLS packData && packData.containingLevels.Count > 0);
    }
}
