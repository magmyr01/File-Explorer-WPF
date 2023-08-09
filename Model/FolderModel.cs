using System.IO;
using System.Linq;

namespace File_Explorer.Model
{
    public class FolderModel : FileFolderBaseModel
    {
        public bool _isExpanded;
        public bool HasDummy { get; set; }

        public FolderModel(
            string fileName, string filePath)
            : base(fileName, filePath)
        {
        }
    }
}