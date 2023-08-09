using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using File_Explorer.Commands;
using File_Explorer.Model;

namespace File_Explorer.ViewModel
{
    public class FileExplorerViewModel : BaseViewModel
    {
        private FileSystemWatcher? _watcher;
        private string _rootDirectory;
        private ICommand _changeRoot;
        private ICommand _exit;
        private ICommand _selectedItemChanged;
        public ObservableCollection<FileFolderBaseViewModel> ListViewItems { get; set; }
        public ObservableCollection<FolderViewModel> Folders { get; set; }
        public ICommand Exit
        {
            get => _exit;
            set
            {
                _exit = value;
            }
        }

        public ICommand ChangeRoot
        {
            get => _changeRoot;
            set
            {
                _changeRoot = value;
            }
        }

        public ICommand SelectedItemChanged
        {
            get => _selectedItemChanged;
            set
            {
                _selectedItemChanged = value;
            }
        }

        public FileFolderBaseViewModel? SelectedItem 
        {
            get => FileFolderBaseModel.SelectedItem;
            set
            {
                if(FileFolderBaseModel.SelectedItem != value)
                {
                    FileFolderBaseModel.SelectedItem = value;
                    RaisePropertyChanged();

                    if (FileFolderBaseModel.SelectedItem is FolderViewModel selectedFolder)
                    {
                        ListViewItems.Clear();
                        foreach (var item in selectedFolder.Folders)
                        {
                            ListViewItems.Add(item);
                        }
                        foreach (var item in selectedFolder.Files)
                        {
                            ListViewItems.Add(item);
                        }
                    }

                    if (FileFolderBaseModel.SelectedItem is not null)
                    {
                        _watcher = new FileSystemWatcher(FileFolderBaseModel.SelectedItem.FilePath);

                        _watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
                        _watcher.Changed += OnChanged;
                        _watcher.Created += OnChanged;
                        _watcher.Deleted += OnChanged;
                        _watcher.Renamed += OnChanged;

                        _watcher.IncludeSubdirectories = true;
                        _watcher.EnableRaisingEvents = true;
                    }
                }
            }
        }

        public string RootDirectory
        {
            get => _rootDirectory;
            set
            {
                _rootDirectory = value;
                Folders.Clear();
                PopulateFolders();
                RaisePropertyChanged();
            }
        }

        public FileExplorerViewModel()
        {
            _rootDirectory = "C:\\Users\\joeoy\\Desktop";
            ListViewItems = new ObservableCollection<FileFolderBaseViewModel>();
            Folders = new ObservableCollection<FolderViewModel>();

            PopulateFolders();

            _changeRoot = new RelayCommand(new Action<object>(HandleChangeRoot));
            _exit = new RelayCommand(new Action<object>(HandleExit));
            _selectedItemChanged = new RelayCommand(new Action<object>(HandleListViewItemChanged));
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background,
              new Action(() => {
                RefetchListBoxItems();  
              }));
        }

        private void RefetchListBoxItems()
        {
            if (FileFolderBaseModel.SelectedItem is FolderViewModel selectedFolder)
            {
                ListViewItems.Clear();
                foreach (var item in selectedFolder.Folders)
                {
                    ListViewItems.Add(item);
                }
                foreach (var item in selectedFolder.Files)
                {
                    ListViewItems.Add(item);
                }
            }
        }

        private void HandleListViewItemChanged(object obj)
        {
            SelectedItem = (FileFolderBaseViewModel)obj;
        }

        private void HandleExit(object obj)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void PopulateFolders()
        {
            string[] directoryPaths = Directory.GetDirectories(RootDirectory);

            foreach (string f in directoryPaths)
            {
                FileAttributes attributes = File.GetAttributes(@f);
                if (attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System))
                {
                    continue;
                }
                Folders.Add((FolderViewModel)CreateFolderItem(f));
            }
        }

        private void HandleChangeRoot(object obj)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if(result == DialogResult.OK)
                {
                    RootDirectory = dialog.SelectedPath;
                }
            }
        }

        private FileFolderBaseViewModel CreateFolderItem(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            return 
                new FolderViewModel(
                    new FolderModel(dirInfo.Name, path), new ObservableCollection<FileFolderBaseViewModel>());
        }
    }
}
