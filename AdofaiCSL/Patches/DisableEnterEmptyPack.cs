using DG.Tweening;
using HarmonyLib;
using System.Linq;

namespace AdofaiCSL.Patches
{
    /// <summary>
    /// Disable entering an empty pack.
    /// </summary>
    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.EnterFolder))]
    internal static class DisableEnterEmptyPack
    {
        private static bool Prefix(scnCLS __instance)
        {
            if (scnCLS.featuredLevelsMode || __instance.loadedLevels.Values.Where(level => level.parentFolderName == __instance.levelToSelect).Count() > 0)
                return true;
            
            ADOBase.controller.ScreenShake(0.5f, 0.8f);
            DOVirtual.DelayedCall(0f, () => __instance.SelectLevel(__instance.loadedLevelTiles[__instance.levelToSelect], true));
            return false;
        }
    }
}
