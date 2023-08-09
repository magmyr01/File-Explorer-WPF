using File_Explorer.ViewModel;

namespace File_Explorer.Model
{
    public abstract class FileFolderBaseModel
    {
        public static FileFolderBaseViewModel? SelectedItem { get; set; }
        public bool IsSelected { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public FileFolderBaseModel(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }
    }
}
