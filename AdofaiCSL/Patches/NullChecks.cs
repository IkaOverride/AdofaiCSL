using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace AdofaiCSL.Patches
{
    [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.SelectLevel))]
    internal static class SelectLevelNullCheck
    {
        /// <summary>
        /// Do not process the select level if the tile is null.
        /// </summary>
        private static bool Prefix(CustomLevelTile tileToSelect) => tileToSelect != null;
    }

    [HarmonyPatch(typeof(DOTweenModuleSprite), nameof(DOTweenModuleSprite.DOFade))]
    internal static class DOFadeSpriteNullCheck
    {
        /// <summary>
        /// Do not process if the sprite renderer is null.
        /// </summary>
        private static bool Prefix(SpriteRenderer target) => target != null;
    }

    [HarmonyPatch(typeof(DOTweenModuleSprite), nameof(DOTweenModuleSprite.DOColor))]
    internal static class DOColorSpriteNullCheck
    {
        /// <summary>
        /// Do not process if the sprite renderer or the color is null.
        /// </summary>
        private static bool Prefix(SpriteRenderer target, Color endValue) => target != null && endValue != null;
    }
}
