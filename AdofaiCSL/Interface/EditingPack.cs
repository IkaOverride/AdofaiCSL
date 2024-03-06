using ADOFAI;

namespace AdofaiCSL.Interface
{
    internal class EditingPack(string key, FolderDataCLS data)
    {
        internal string Key = key;
        internal string Title = data.title.Trim();
        internal string Author = data.author.Trim();
        internal string Artist = data.artist.Trim();
        internal string Difficulty = data.difficulty.ToString();
        internal string Description = data.description.Trim();
        internal string ImageRelativePath = data.previewImage.Trim();
        internal string IconRelativePath = data.previewIcon.Trim();
        internal string IconColor = data._previewIconColor.ToHex();
    }
}
