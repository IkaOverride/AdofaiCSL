using ADOFAI;

namespace AdofaiCSL.Interface
{
    internal class EditingPack(string key, FolderDataCLS data)
    {
        internal string Key = key;
        internal string Title = data.title;
        internal string Author = data.author;
        internal string Artist = data.artist;
        internal string Difficulty = data.difficulty.ToString();
        internal string Description = data.description;
        internal string ImageRelativePath = data.previewImage;
        internal string IconRelativePath = data.previewIcon;
        internal string IconColor = data._previewIconColor.ToHex();
    }
}
