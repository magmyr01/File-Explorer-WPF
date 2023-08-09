using System.IO;
using System.Windows;
using File_Explorer.Model;

namespace File_Explorer.ViewModel
{
    public class FileViewModel : FileFolderBaseViewModel
    {
        FileModel _model;

        public override bool IsSelected
        {
            get => _model.IsSelected;
            set
            {
                if (value != _model.IsSelected)
                {
                    _model.IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected override void HandleDelete(object obj)
        {
            MessageBoxResult result = MessageBox.Show("Delete file?", "Delete file?", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                File.Delete(FilePath);
            }
        }

        public FileViewModel(FileModel model) : base(model)
        {
            _model = model;
        }
    }
}
