using AdofaiCSL.API.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdofaiCSL.Patches
{
    [HarmonyPatch(typeof(EntranceTile), nameof(EntranceTile.LateUpdate))]
    internal static class EntranceTileFix
    {
        private static bool Prefix(EntranceTile __instance)
        {
            scnCLS screen = scnCLS.instance;
            float y = ADOBase.controller.camy.pos.y;

            if (screen.levelCount >= screen.levelCountForLoop)
            {    
                if (screen.IsKeyValid(screen.currentFolderName) && screen.gemExitFolder.gameObject.activeSelf)
                {
                    float topPos = screen.gemExitFolder.position.y;
                    int packLevelCount = screen.loadedLevels.Values.Where(level => level.parentFolderName == screen.currentFolderName).Count();

                    y = Mathf.Clamp(y, topPos - packLevelCount, topPos - 1);
                }
                
                else if (screen.gemBottom.gameObject.activeSelf && screen.gemTop.gameObject.activeSelf)
                    y = Mathf.Clamp(y, screen.gemBottomY + 1, screen.gemTopY - 1);

                else
                {
                    IEnumerable<float> positions = screen.loadedLevels.Where(kvp => kvp.Value.parentFolderName == screen.currentFolderName).Select(kvp => screen.loadedLevelTiles[kvp.Key].y);
                    y = Mathf.Clamp(y, positions.Min(), positions.Max());
                }
            }

            __instance.transform.MoveY(y);

            return false;
        }
    }
}
