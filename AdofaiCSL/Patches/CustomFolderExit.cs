using DG.Tweening;
using HarmonyLib;
using System.IO;
using System.Linq;

namespace AdofaiCSL.Patches {

    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.ExitFolder))]
    internal static class CustomFolderExit {

        /// <summary>
        /// Exit folder for subfolders / nested folders and refresh to fix icons after exiting a level.
        /// </summary>
        private static bool Prefix(scnCLS __instance) {

            if (scnCLS.featuredLevelsMode)
                return true;

            CurrentTileTracker.TileKey = __instance.currentFolderName;

            string[] dirs = __instance.currentFolderName.Split(Path.DirectorySeparatorChar);
            __instance.currentFolderName = dirs.Length == 1 ? null : string.Join(Path.DirectorySeparatorChar.ToString(), dirs.Take(dirs.Length - 1));

            DOVirtual.DelayedCall(0f, () => __instance.Refresh());

            return false;
        }
    }
}
