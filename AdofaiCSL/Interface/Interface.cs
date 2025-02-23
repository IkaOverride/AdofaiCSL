using AdofaiCSL.API.Extensions;
using AdofaiCSL.API.Features;
using AdofaiCSL.Patches;
using DG.Tweening;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static SteamWorkshop;
using static UnityModManagerNet.UnityModManager;

namespace AdofaiCSL.Interface
{
    internal static class Interface
    {
        internal static scnCLS screen;

        /// <summary>
        /// Selected levels.
        /// </summary>
        internal static HashSet<string> selectedKeys = [];

        /// <summary>
        /// If the player is on a tile.
        /// </summary>
        internal static bool isOnTile = false;

        /// <summary>
        /// If the player is in a pack.
        /// </summary>
        internal static bool isInPack = false;

        internal static EditingPack editingPack = null;

        internal static void Check(ModEntry _)
        {
            screen = scnCLS.instance;

            if (screen is null || !screen.enabled || screen.refreshing || scnCLS.featuredLevelsMode || screen.showingInitialMenu)
            {
                if (BetterCLSLoader.IsBetterCLS)
                {
                    bool openSongsPath = GUILayout.Button("Open songs directory", GUILayout.Width(212), GUILayout.Height(22));

                    if (openSongsPath)
                        Process.Start(Main.SongsDirectory);

                    return;
                }

                ShowDisabled();
                return;
            }

            Show();
        }

        internal static void ShowDisabled()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            bool openSongsPath = GUILayout.Button("Open songs directory", GUILayout.Width(212), GUILayout.Height(22));
            GUI.enabled = false;
            GUILayout.Button("New pack here", GUILayout.Width(212), GUILayout.Height(22));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("Select all here", GUILayout.Width(140), GUILayout.Height(22));
            GUILayout.Button("Select levels here", GUILayout.Width(140), GUILayout.Height(22));
            GUILayout.Button("Select packs here", GUILayout.Width(140), GUILayout.Height(22));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            GUILayout.Button("Select", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Button("Edit pack info", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Button("Open directory", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Label($"Current tile: /");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Button("Select", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Button("Edit pack info", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Button("Open directory", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Label($"Current pack: /");
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            GUILayout.EndVertical();

            GUI.enabled = true;

            if (openSongsPath)
                Process.Start(Main.SongsDirectory);
        }

        internal static void Show()
        {
            isOnTile = screen.IsKeyValid(screen.levelToSelect);
            isInPack = screen.IsKeyValid(screen.currentFolderName);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            bool openSongsPath = GUILayout.Button("Open songs directory", GUILayout.Width(212), GUILayout.Height(22));
            // New pack
            GUI.enabled = isOnTile;
            bool newPackHere = GUILayout.Button("New pack here", GUILayout.Width(212), GUILayout.Height(22));
            GUILayout.EndHorizontal();

            // Select
            GUILayout.BeginHorizontal();
            GUI.enabled = true;
            bool selectAllHere = GUILayout.Button("Select all here", GUILayout.Width(140), GUILayout.Height(22));
            bool selectLevelsHere = GUILayout.Button("Select levels here", GUILayout.Width(140), GUILayout.Height(22));
            bool selectPacksHere = GUILayout.Button("Select packs here", GUILayout.Width(140), GUILayout.Height(22));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Current tile
            GUILayout.BeginHorizontal();
            GUI.enabled = isOnTile;
            bool selectTile = GUILayout.Button(isOnTile && selectedKeys.Contains(screen.levelToSelect) ? "Deselect" : "Select", GUILayout.Width(110), GUILayout.Height(22));
            GUI.enabled = isOnTile && screen.loadedLevels[screen.levelToSelect].isFolder;
            bool editTile = GUILayout.Button("Edit pack info", GUILayout.Width(110), GUILayout.Height(22));
            GUI.enabled = true;
            bool openTilePath = GUILayout.Button("Open directory", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Label($"Current tile: {(isOnTile ? Regex.Replace(screen.loadedLevelTiles[screen.levelToSelect].title.text.Replace('\n', ' '), CustomLevelTileExtensions.NoTagsRegex, string.Empty) : "/")}");
            GUILayout.EndHorizontal();

            // Current pack
            GUILayout.BeginHorizontal();
            GUI.enabled = isInPack;
            bool selectPack = GUILayout.Button(isInPack && selectedKeys.Contains(screen.currentFolderName) ? "Deselect" : "Select", GUILayout.Width(110), GUILayout.Height(22));
            bool editPack = GUILayout.Button("Edit pack info", GUILayout.Width(110), GUILayout.Height(22));
            GUI.enabled = true;
            bool openPackPath = GUILayout.Button("Open directory", GUILayout.Width(110), GUILayout.Height(22));
            GUILayout.Label($"Current pack: {(isInPack ? Regex.Replace(screen.loadedLevelTiles[screen.currentFolderName].title.text.Replace('\n', ' '), CustomLevelTileExtensions.NoTagsRegex, string.Empty) : "/")}");
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // Buttons
            if (openSongsPath)
                Process.Start(Main.SongsDirectory);

            if (selectTile)
            {
                if (selectedKeys.Contains(screen.levelToSelect))
                    selectedKeys.Remove(screen.levelToSelect);
                else
                    selectedKeys.Add(screen.levelToSelect);
            }

            if (openTilePath)
                Process.Start(screen.loadedLevelDirs[screen.levelToSelect]);

            if (editTile)
            {
                string packConfigPath = Path.Combine(screen.loadedLevelDirs[screen.levelToSelect], "main.pack");
                editingPack = new EditingPack(screen.levelToSelect, CustomConfig.Read(packConfigPath).AsPackData());
            }

            if (selectPack)
            {
                if (selectedKeys.Contains(screen.currentFolderName))
                    selectedKeys.Remove(screen.currentFolderName);
                else
                    selectedKeys.Add(screen.currentFolderName);
            }

            if (openPackPath)
                Process.Start(screen.loadedLevelDirs[screen.currentFolderName]);

            if (editPack)
            {
                string packConfigPath = Path.Combine(screen.loadedLevelDirs[screen.currentFolderName], "main.pack");
                editingPack = new EditingPack(screen.currentFolderName, CustomConfig.Read(packConfigPath).AsPackData());
            }

            if (newPackHere)
            {
                string newPackPath = Path.Combine(isInPack ? screen.loadedLevelDirs[screen.currentFolderName] : Main.SongsDirectory, "Pack").GetUniqueDirectoryPath();
                string newPackName = newPackPath.Split(Path.DirectorySeparatorChar).Last();
                newPackPath.CreatePack();
                CurrentTileTracker.TileKey = isInPack ? Path.Combine(screen.currentFolderName, newPackName) : $"Custom:{newPackName}";
                DOVirtual.DelayedCall(0f, () => screen.Refresh());
            }

            if (selectAllHere)
                screen.loadedLevels.Where(loadedLevel => !selectedKeys.Contains(loadedLevel.Key) && screen.IsKeySearched(loadedLevel.Key))
                    .Do(loadedLevel => selectedKeys.Add(loadedLevel.Key));

            if (selectLevelsHere)
                screen.loadedLevels.Where(loadedLevel => loadedLevel.Value.isLevel && !selectedKeys.Contains(loadedLevel.Key) && screen.IsKeySearched(loadedLevel.Key))
                    .Do(loadedLevel => selectedKeys.Add(loadedLevel.Key));

            if (selectPacksHere)
                screen.loadedLevels.Where(loadedLevel => loadedLevel.Value.isFolder && !selectedKeys.Contains(loadedLevel.Key) && screen.IsKeySearched(loadedLevel.Key))
                    .Do(loadedLevel => selectedKeys.Add(loadedLevel.Key));

            selectedKeys.ToList().DoIf(key => !screen.IsKeyValid(key), key => selectedKeys.Remove(key));

            if (selectedKeys.Count > 0)
                ShowSelector(screen);

            if (editingPack is not null && screen.IsKeyValid(editingPack.Key))
                ShowEditor(screen);
            else
                editingPack = null;

            GUILayout.EndVertical();
        }

        internal static void ShowSelector(scnCLS screen)
        {
            bool isOnPack = false;
            isOnPack = isOnTile && screen.loadedLevels[screen.levelToSelect].isFolder;

            IEnumerable<string> workshopKeys = selectedKeys.ToList().Where(screen.IsKeyWorkshop);
            IEnumerable<string> customKeys = selectedKeys.ToList().Where(key => !screen.IsKeyWorkshop(key));
            bool containsWorkshop = workshopKeys.Count() > 0;
            bool onlyWorkshop = containsWorkshop && customKeys.Count() == 0;

            GUILayout.Space(6);
            GUILayout.Label("Selector", Styles.Header);
            GUILayout.Space(1);
            GUILayout.Label($"Selected: {string.Join(", ", selectedKeys.Select(key => Regex.Replace(screen.loadedLevelTiles[key].title.text.Replace('\n', ' '), CustomLevelTileExtensions.NoTagsRegex, string.Empty)))}");

            GUILayout.BeginHorizontal();

            GUI.enabled = isOnTile && !onlyWorkshop;
            bool moveHere = GUILayout.Button("Move here", GUILayout.Width(150), GUILayout.Height(22));

            GUI.enabled = isOnPack && !onlyWorkshop;
            bool moveIn = GUILayout.Button("Move in tile", GUILayout.Width(150), GUILayout.Height(22));

            GUI.enabled = isInPack && !onlyWorkshop;
            bool moveOut = GUILayout.Button("Move out of here", GUILayout.Width(150), GUILayout.Height(22));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUI.enabled = containsWorkshop;
            bool convertCSL = GUILayout.Button("Convert Workshop -> AdofaiCSL", GUILayout.Width(227), GUILayout.Height(22));

            GUI.enabled = true;
            bool deselect = GUILayout.Button("Deselect all", GUILayout.Width(227), GUILayout.Height(22));

            GUILayout.EndHorizontal();

            if (containsWorkshop)
                GUILayout.Label("Warning: you cannot move workshop levels", Styles.Warning);

            if (convertCSL)
            {
                HashSet<string> newKeys = [];

                foreach (string key in workshopKeys)
                {
                    selectedKeys.Remove(key);

                    string newPath = Path.Combine(Main.SongsDirectory, key).GetUniqueDirectoryPath();
                    string tileName = newPath.Split(Path.DirectorySeparatorChar).Last();
                    Directory.Move(screen.loadedLevelDirs[key], newPath);

                    screen.isWorkshopLevel.Remove(key);
                    screen.loadedLevelIsDeleted.Remove(key);

                    if (ulong.TryParse(key, out ulong levelId))
                        foreach (ResultItem resultItem in resultItems.Where(item => (ulong) item.id == levelId))
                            Unsubscribe(resultItem.id);

                    newKeys.Add("Custom:" + tileName);
                }

                DOVirtual.DelayedCall(0.5f, () =>
                {
                    screen.Refresh();
                    newKeys.Do(key => selectedKeys.Add(key));
                });
            }

            if (moveHere)
            {
                HashSet<string> newKeys = [];

                string basePath = isInPack ? Path.Combine(Main.SongsDirectory, screen.currentFolderName.Replace("Custom:", string.Empty)) : Main.SongsDirectory;

                foreach (string key in customKeys)
                {
                    string tileName = key.Replace("Custom:", string.Empty).Split(Path.DirectorySeparatorChar).Last();
                    string newPath = Path.Combine(basePath, tileName);
                    string oldPath = screen.loadedLevelDirs[key];

                    if (oldPath == newPath || newPath.IsSubDirectoryOf(oldPath))
                        continue;

                    newPath = newPath.GetUniqueDirectoryPath();
                    tileName = newPath.Split(Path.DirectorySeparatorChar).Last();

                    selectedKeys.Remove(key);

                    Directory.Move(oldPath, newPath);

                    newKeys.Add(isInPack ? Path.Combine(screen.currentFolderName, tileName) : $"Custom:{tileName}");
                }

                DOVirtual.DelayedCall(0f, () =>
                {
                    screen.Refresh();
                    newKeys.Do(key => selectedKeys.Add(key));
                });
            }

            if (moveIn)
            {
                HashSet<string> newKeys = [];

                string basePath = Path.Combine(Main.SongsDirectory, screen.levelToSelect.Replace("Custom:", string.Empty));

                foreach (string key in customKeys)
                {
                    string tileName = key.Replace("Custom:", string.Empty).Split(Path.DirectorySeparatorChar).Last();
                    string newPath = Path.Combine(basePath, tileName);
                    string oldPath = screen.loadedLevelDirs[key];

                    if (oldPath == newPath || newPath.IsSubDirectoryOf(oldPath))
                        continue;

                    newPath = newPath.GetUniqueDirectoryPath();
                    tileName = newPath.Split(Path.DirectorySeparatorChar).Last();

                    selectedKeys.Remove(key);

                    Directory.Move(oldPath, newPath);

                    newKeys.Add(Path.Combine(screen.levelToSelect, tileName));
                }

                DOVirtual.DelayedCall(0f, () =>
                {
                    screen.Refresh();
                    newKeys.Do(key => selectedKeys.Add(key));
                });
            }

            if (moveOut)
            {
                HashSet<string> newKeys = [];

                string[] currentSplitKey = screen.levelToSelect.Split(Path.DirectorySeparatorChar);
                string[] currentSplitPath = screen.loadedLevelDirs[screen.levelToSelect].Split(Path.DirectorySeparatorChar);
                string basePath = string.Join(Path.DirectorySeparatorChar.ToString(), currentSplitPath.Take(currentSplitPath.Length - 2));

                foreach (string key in customKeys)
                {
                    string tileName = key.Replace("Custom:", string.Empty).Split(Path.DirectorySeparatorChar).Last();
                    string newPath = Path.Combine(basePath, tileName);
                    string oldPath = screen.loadedLevelDirs[key];

                    if (oldPath == newPath || newPath.IsSubDirectoryOf(oldPath))
                        continue;

                    newPath = newPath.GetUniqueDirectoryPath();
                    tileName = newPath.Split(Path.DirectorySeparatorChar).Last();

                    selectedKeys.Remove(key);

                    Directory.Move(oldPath, newPath);

                    newKeys.Add(Path.Combine($"Custom:{string.Join(Path.DirectorySeparatorChar.ToString(), currentSplitKey.Take(currentSplitKey.Length - 2))}", tileName));
                }

                DOVirtual.DelayedCall(0f, () =>
                {
                    screen.Refresh();
                    newKeys.Do(key => selectedKeys.Add(key));
                });
            }

            if (deselect)
                selectedKeys.Clear();
        }

        internal static void ShowEditor(scnCLS screen)
        {
            string packPath = screen.loadedLevelDirs[editingPack.Key];
            string baseTitle = screen.loadedLevels[editingPack.Key].title;

            GUILayout.Space(6);
            GUILayout.Label("Pack editor", Styles.Header);
            GUILayout.Space(1);
            GUILayout.Label($"Selected pack: {Regex.Replace(baseTitle, CustomLevelTileExtensions.NoTagsRegex, string.Empty)}");
            GUILayout.Label($"Path: {packPath}");

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Title:", GUILayout.ExpandWidth(false));
            editingPack.Title = GUILayout.TextField(editingPack.Title, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Author:", GUILayout.ExpandWidth(false));
            editingPack.Author = GUILayout.TextField(editingPack.Author, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Artist:", GUILayout.ExpandWidth(false));
            editingPack.Artist = GUILayout.TextField(editingPack.Artist, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Difficulty:", GUILayout.ExpandWidth(false));
            editingPack.Difficulty = GUILayout.TextField(editingPack.Difficulty, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Description:", GUILayout.ExpandWidth(false));
            editingPack.Description = GUILayout.TextField(editingPack.Description, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Image relative path:", GUILayout.ExpandWidth(false));
            editingPack.ImageRelativePath = GUILayout.TextField(editingPack.ImageRelativePath, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Icon relative path:", GUILayout.ExpandWidth(false));
            editingPack.IconRelativePath = GUILayout.TextField(editingPack.IconRelativePath, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(650));
            GUILayout.Label("Icon color:", GUILayout.ExpandWidth(false));
            editingPack.IconColor = GUILayout.TextField(editingPack.IconColor, GUILayout.Height(21));
            GUILayout.EndHorizontal();

            GUILayout.Space(0.5f);

            GUILayout.BeginHorizontal();

            bool cancel = GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(22));
            bool save = GUILayout.Button("Save pack", GUILayout.Width(80), GUILayout.Height(22));

            GUILayout.EndHorizontal();

            if (cancel)
                editingPack = null;

            if (save)
            {
                string packConfigPath = Path.Combine(packPath, "main.pack");

                CustomConfig.Write(packConfigPath, new Dictionary<string, string>() {
                    { "title", editingPack.Title },
                    { "author", editingPack.Author },
                    { "artist", editingPack.Artist },
                    { "difficulty", editingPack.Difficulty },
                    { "description", editingPack.Description },
                    { "image", editingPack.ImageRelativePath },
                    { "icon", editingPack.IconRelativePath },
                    { "color", editingPack.IconColor },
                });

                editingPack = null;

                DOVirtual.DelayedCall(0f, () => screen.Refresh());
            }
        }
    }
}
