using ADOFAI;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdofaiCSL.API.Extensions
{
    public static class CustomLevelTileExtensions
    {
        public const string NoTagsRegex = @"<[^>]+>";

        /// <summary>
        /// Setup a level's tile.
        /// </summary>
        /// <param name="tile">The <see cref="CustomLevelTile"/>.</param>
        /// <param name="key">The level's key.</param>
        /// <param name="data">The <see cref="LevelDataCLS"/>.</param>
        public static void Setup(this CustomLevelTile tile, string key, LevelDataCLS data)
        {
            tile.levelKey = key;
            tile.title.text = Regex.Replace(data.title, NoTagsRegex, "").Trim();
            tile.artist.text = Regex.Replace(data.artist, NoTagsRegex, "").Trim();
            tile.image.enabled = data.previewIcon.Any();
        }

        /// <summary>
        /// Setup a pack's tile.
        /// </summary>
        /// <param name="tile">The <see cref="CustomLevelTile"/>.</param>
        /// <param name="key">The pack's key.</param>
        /// <param name="data">The <see cref="FolderDataCLS"/>.</param>
        public static void Setup(this CustomLevelTile tile, string key, FolderDataCLS data)
        {
            tile.levelKey = key;
            tile.title.text = data.title;
            tile.artist.text = data.artist;
            tile.image.enabled = data.previewImage.HasImageFileExtension();
        }
    }
}
