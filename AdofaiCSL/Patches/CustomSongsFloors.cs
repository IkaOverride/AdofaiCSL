using AdofaiCSL.API.Extensions;
using AdofaiCSL.API.Features;
using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdofaiCSL.Patches
{
    /// <summary>
    /// Add new tiles for the custom songs.
    /// </summary>
    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.CreateFloors))]
    internal static class CustomSongsFloors
    {
        private static string currentLevelKey;

        private static void Prefix(scnCLS __instance)
        {
            currentLevelKey = CurrentTileTracker.TileKey ;
            __instance.currentFolderName = null;
        }
        
        private static void Postfix(scnCLS __instance)
        {
            if (!scnCLS.featuredLevelsMode)
            {
                // Sort songs directory
                SongsDirectory.SortAsSongsDirectory(Main.SongsDirectory);

                // Add levels
                __instance.AddLevels(Main.SongsDirectory);
            }

            DOVirtual.DelayedCall(0, () =>
            {
                // Sort levels
                __instance.optionsPanels.UpdateSorting();

                List<CustomLevelTile> orderedTiles = __instance.loadedLevelTiles.Values.Where(tile => __instance.loadedLevels[tile.levelKey].parentFolderName is null).OrderBy(tile => tile.y).ToList();

                // Custom tile tracking
                if (__instance.loadedLevelTiles.ContainsKey(currentLevelKey))
                {
                    // Put the player back in the right folder
                    if (currentLevelKey.Contains(Path.DirectorySeparatorChar))
                    {
                        string[] dirs = currentLevelKey.Split(Path.DirectorySeparatorChar);

                        __instance.currentFolderName = string.Join(Path.DirectorySeparatorChar.ToString(), dirs.Take(dirs.Length - 1));
                        __instance.sortedLevelKeys = __instance.optionsPanels.SortedLevelKeys();
                        __instance.SearchLevels(__instance.searchParameter);
                    }

                    // Put the player back on the right tile
                    __instance.SelectLevel(__instance.loadedLevelTiles[currentLevelKey], true);
                }

                // Put the player back on the center tile
                else if (__instance.currentFolderName is null && orderedTiles.Count > 0)
                        __instance.SelectLevel(orderedTiles[(int) Math.Round(orderedTiles.Count / 2f)], true);
            });
        }
    }
}
