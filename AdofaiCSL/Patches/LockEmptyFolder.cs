using DG.Tweening;
using HarmonyLib;
using System.Linq;

namespace AdofaiCSL.Patches {

    /// <summary>
    /// Don't enter if there's no levels.
    /// </summary>
    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.EnterFolder))]
    internal static class LockEmptyFolder {

        private static bool Prefix(scnCLS __instance) {

            if (scnCLS.featuredLevelsMode || __instance.loadedLevels.Values.Where(level => level.parentFolderName == __instance.levelToSelect).Count() > 0)
                return true;
            
            ADOBase.controller.ScreenShake(0.5f, 0.8f);
            DOVirtual.DelayedCall(0f, () => __instance.SelectLevel(__instance.loadedLevelTiles[__instance.levelToSelect], true));
            return false;
        }
    }
}
