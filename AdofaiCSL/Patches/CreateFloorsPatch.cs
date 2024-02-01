using System.IO;
using HarmonyLib;
using AdofaiCSL.API;
using System;

namespace AdofaiCSL.Patches {

    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.CreateFloors))]
    internal static class CreateFloorsPatch {
     
        private static void Postfix(scnCLS __instance) {
            
            if (scnCLS.featuredLevelsMode)
                return;

            FileUtil.SortAsSongDirectory(Main.SongsDirectory);

            foreach (string levelPath in Directory.GetDirectories(Main.SongsDirectory)) {

                try {

                    // Pack
                    if (Directory.GetFiles(levelPath, "*.pack").Length > 0)
                        __instance.AddCustomPack(levelPath);

                    // Single song
                    else if (Directory.GetFiles(levelPath, "main.adofai").Length > 0)
                        __instance.AddCustomLevel(levelPath);

                } catch (Exception e) {
                    Main.ModEntry.Logger.Error($"Could not load the level at '{levelPath}'. Error: '{e.GetType().Name} - {e.Message}'");
                }
            }
            
            __instance.sortedLevelKeys.Sort();
        }
    }
}