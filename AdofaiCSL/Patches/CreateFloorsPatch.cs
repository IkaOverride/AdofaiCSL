using System.IO;
using HarmonyLib;
using AdofaiCSL.API;
using UnityEngine;
using System;

namespace AdofaiCSL.Patches {

    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.CreateFloors))]
    internal static class CreateFloorsPatch {

        private static void Postfix(scnCLS __instance) {
            FileUtil.SortAsSongDirectory(Main.SongsDirectory);

            int position = Mathf.FloorToInt(__instance.loadedLevels.Count / 2) + 1;

            string[] songs = Directory.GetDirectories(Main.SongsDirectory);

            try {
                foreach (string songPath in songs) {

                    // Single song
                    if (Directory.GetFiles(songPath, "main.adofai").Length > 0)
                        __instance.AddCustomLevel(songPath, position);

                    // Pack song
                    else if (Directory.GetFiles(songPath, "*.pack").Length > 0)
                        __instance.AddCustomPack(songPath, position);

                    position++;
                }
            } catch (Exception e) {
                Main.ModEntry.Logger.LogException(e);
            }
            
            __instance.sortedLevelKeys.Sort();
            
            __instance.gemTopY = position;
            __instance.gemTop.MoveY(__instance.gemTopY);
        }
    }
}