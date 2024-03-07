using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using static UnityModManagerNet.UnityModManager;

namespace AdofaiCSL
{
    public class Main {
        /// <summary>
        /// The path to the custom songs.
        /// </summary>
        public static string SongsDirectory = Path.Combine(AppContext.BaseDirectory, "CustomSongs");

        /// <summary>
        /// The mod entry.
        /// </summary>
        public static ModEntry ModEntry;

        /// <summary>
        /// The harmony instance.
        /// </summary>
        public static Harmony HarmonyInstance;

        /// <summary>
        /// Entry point for AdofaiCSL.
        /// </summary>
        /// <param name="modEntry">The mod entry.</param>
        private static void Load(ModEntry modEntry) 
        {
            ModEntry = modEntry;

            HarmonyInstance = new Harmony("adofaicsl");

            if (!Directory.Exists(SongsDirectory)) 
            {
                modEntry.Logger.Log($"Creating directory: '{SongsDirectory}'");
                Directory.CreateDirectory(SongsDirectory);
            }

            BetterCLSLoader.BetterCLSUnity = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "BetterCLSUnity");

            if (BetterCLSLoader.IsBetterCLS)
            {
                HarmonyInstance.Patch(
                    AccessTools.Method(BetterCLSLoader.BetterCLSUnity.GetType("BetterCLSUnity.SongListController"), "LoadCustomFiles"), // Method to patch
                    postfix: new HarmonyMethod(typeof(BetterCLSLoader).GetMethod(nameof(BetterCLSLoader.LoadCustomFiles))) // Postfix
                );
            }

            else
            {
                HarmonyInstance.PatchAll();
                modEntry.OnGUI += Interface.Interface.Check;
            }
        }
    }
}
