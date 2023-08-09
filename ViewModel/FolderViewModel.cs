using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using File_Explorer.Commands;
using File_Explorer.Model;

namespace File_Explorer.ViewModel
{
    public class FolderViewModel : FileFolderBaseViewModel
    {
        FolderModel _folderModel;
        public ObservableCollection<FileFolderBaseViewModel> _files;
        public ObservableCollection<FileFolderBaseViewModel> _folders;

        private FileSystemWatcher? _watcher;

        private ICommand _paste;
        private ICommand _doubleClickListView;

        public ICommand DoubleClickListBoxFolder
        {
            get => _doubleClickListView;
            set
            {
                _doubleClickListView = value;
                RaisePropertyChanged();
            }
        }
        public ICommand PasteClicked
        {
            get => _paste;
            set
            {
                _paste = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<FileFolderBaseViewModel> Folders
        {
            get
            {
                if(_folderModel.HasDummy)
                {
                    _folders.Clear();
                    PopulateFoldersOnDemand();
                }
                return _folders;
            }
            set { _folders = value; RaisePropertyChanged(); }
        }
        public ObservableCollection<FileFolderBaseViewModel> Files
        {
            get
            {
                _files.Clear();
                PopulateFilesOnDemand();
                return _files;
            }
            set { _files = value; RaisePropertyChanged(); }
        }
        public override bool IsSelected
        {
            get => _folderModel.IsSelected;
            set
            {
                if (value != _folderModel.IsSelected)
                {
                    _folderModel.IsSelected = value;
                    RaisePropertyChanged();

                    //Remove contents only if not expanded and not selected
                    if (!_folderModel.IsSelected && !_folderModel._isExpanded)
                    {
                        if (_folders.Count() > 0)
                        {
                            _folderModel.HasDummy = true;
                            Folders.Clear();
                            Folders.Add(new FileViewModel(new FileModel("dummy", "dummy")));
                        }
                        Files.Clear();
                    }
                    else
                    {
                        if (_watcher is null)
                        {
                            _watcher = new FileSystemWatcher(FilePath);

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
        }
        public bool IsExpanded
        {
            get => _folderModel._isExpanded;
            set
            {
                if (value != _folderModel._isExpanded)
                {
                    _folderModel._isExpanded = value;
                    RaisePropertyChanged();

                    //Remove contents only if not expanded and not selected
                    if (!_folderModel._isExpanded && !_folderModel.IsSelected)
                    {
                        if (_folders.Count() > 0)
                        {
                            _folderModel.HasDummy = true;
                            _folders.Clear();
                            Folders.Add(new FileViewModel(new FileModel("dummy", "nopath")));
                        }
                        _files.Clear();
                        _watcher = null;
                    }
                    else
                    {
                        if(_watcher is null)
                        {
                            _watcher = new FileSystemWatcher(FilePath);

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
        }

        public FolderViewModel(FolderModel folderModel, ObservableCollection<FileFolderBaseViewModel> contents) : base(folderModel)
        {
            _folderModel = folderModel;
            _files = new ObservableCollection<FileFolderBaseViewModel>(contents.OfType<FileViewModel>());
            _folders = new ObservableCollection<FileFolderBaseViewModel>(contents.OfType<FolderViewModel>());

            if(Directory.GetDirectories(_folderModel.FilePath).Any())
            {
                _folderModel.HasDummy = true;
                _folders.Add(new FileViewModel(new FileModel("dummy", "nopath")));
            }

            _paste = new RelayCommand(new Action<object>(HandlePaste));
            _doubleClickListView = new RelayCommand(new Action<object>(HandleClickListBoxFolder));
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
            Folders.Clear();
            Files.Clear();
            PopulateFoldersOnDemand();
            PopulateFilesOnDemand();
        }

        protected override void HandleDelete(object obj)
        {
            MessageBoxResult result = MessageBox.Show("Delete folder and all its contents?", "Delete folder?", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes) { Directory.Delete(FilePath, true); }
        }

        private void Move(string file)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(file);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    Directory.Move(file, FilePath + "\\" + new DirectoryInfo(file).Name);
                }
                else
                {
                    File.Move(file, FilePath + "\\" + Path.GetFileName(file), true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK);
                return;
            }
        }

        private void Copy(string file)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(file);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(file, FilePath + "\\" + new DirectoryInfo(file).Name, true);
                }
                else
                {
                    File.Copy(file, FilePath + "\\" + Path.GetFileName(file), true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK);
                return;
            }
        }

        private void HandlePaste(object obj)
        {
            string pathOfItem = Clipboard.GetText();

            //Check if the clipboard has a valid path
            if(!File.Exists(pathOfItem) && !Directory.Exists(pathOfItem))
            {
                return;
            }
            //Check if the file exists in directory
            if(File.Exists(FilePath + "\\" + Path.GetFileName(pathOfItem)))
            {
                MessageBoxResult retVal = MessageBox.Show("File already exists. Overwrite?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (retVal != MessageBoxResult.Yes) { return; }
            }
            if (PreviousRightClickAction is RightClickAction.Move)
            {
                Move(pathOfItem);
            }
            else if (PreviousRightClickAction is RightClickAction.Copy)
            {
                Copy(pathOfItem);
            }
        }

        private void HandleClickListBoxFolder(object obj) => IsSelected = true;

        private void PopulateFoldersOnDemand()
        {
            string[] dirs = Directory.GetDirectories(_folderModel.FilePath);
            FileFolderBaseModel model;
            DirectoryInfo inf;

            foreach (string dir in dirs)
            {
                inf = new DirectoryInfo(dir);
                model = new FolderModel(inf.Name, inf.FullName);
                _folders.Add(new FolderViewModel((FolderModel)model, new ObservableCollection<FileFolderBaseViewModel>()));
            }
        }
        private void PopulateFilesOnDemand()
        {
            string[] files = Directory.GetFiles(_folderModel.FilePath);
            FileFolderBaseModel model;
            FileInfo fi;

            foreach (string f in files)
            {
                fi = new FileInfo(f);
                model = new FileModel(fi.Name, fi.FullName);
                _files.Add(new FileViewModel((FileModel)model));
            }
        }
    }
}
