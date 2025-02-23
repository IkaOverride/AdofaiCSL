using DG.Tweening;
using HarmonyLib;
using Steamworks;
using System.Linq;
using Object = UnityEngine.Object;

namespace AdofaiCSL.Patches
{
    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.Refresh))]
    internal static class CustomRefresh
    {
        private static bool Prefix(scnCLS __instance)
        {
            __instance.errorCanvas.gameObject.SetActive(false);
            __instance.chainBottom.gameObject.SetActive(false);
            __instance.chainTop.gameObject.SetActive(false);
            __instance.gemExitFolder.gameObject.SetActive(false);

            if (__instance.showingInitialMenu || scnCLS.featuredLevelsMode || SteamUGC.GetNumSubscribedItems() > 0)
                return true;

            __instance.refreshing = true;
            __instance.DisableCLS(true);
            __instance.DisablePlanets(true);
            __instance.loadingText.gameObject.SetActive(true);

            __instance.levelToSelect = null;
            __instance.lastSongsLoaded = [];
            __instance.lastTexturesLoaded = [];

            __instance.loadedLevelTiles?.Values.Select(tile => tile.gameObject).Do(Object.Destroy);

            __instance.newlyInstalledLevelKeys = [];
            __instance.sortedLevelKeys = [];
            __instance.loadedLevels = [];
            __instance.loadedLevelTiles = [];
            __instance.loadedLevelDirs = [];

            __instance.CreateFloors();
            __instance.levelCount = __instance.loadedLevels.Count;

            if (__instance.levelCount > 0)
            {
                __instance.DisableCLS(false);
                __instance.DisablePlanets(false);
            }

            else
                __instance.errorCanvas.gameObject.SetActive(true);

            __instance.optionsPanels.searchMode = false;
            __instance.optionsPanels.searchInputField.text = string.Empty;
            __instance.currentSearchText.text = RDString.Get("cls.shortcut.find");
            __instance.currentSearchText.SetLocalizedFont();
            __instance.optionsPanels.UpdateOrderText();

            __instance.loadingText.gameObject.SetActive(false);
            __instance.refreshing = false;

            __instance.showingInitialMenu = true;
            DOVirtual.DelayedCall(0f, () => __instance.showingInitialMenu = false);
            return true;
        }
    }
}
