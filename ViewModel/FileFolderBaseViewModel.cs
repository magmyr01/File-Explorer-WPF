using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using File_Explorer.Commands;
using File_Explorer.Model;

namespace File_Explorer.ViewModel
{
    public enum RightClickAction
    {
        Move,
        Copy
    }
    public abstract class FileFolderBaseViewModel : BaseViewModel
    {
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;

        protected static RightClickAction? PreviousRightClickAction {  get; set; }
        

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private ICommand _fileProperties;
        private ICommand _toClipBoard_Move;
        private ICommand _toClipBoard_Copy;
        private ICommand _delete;
        FileFolderBaseModel _model;

        public ICommand FileProperties
        {
            get => _fileProperties;
            set
            {
                _fileProperties = value;
                RaisePropertyChanged();
            }
        }

        public ICommand ToClipBoard_Move
        {
            get => _toClipBoard_Move;
            set
            {
                _toClipBoard_Move = value;
                RaisePropertyChanged();
            }
        }

        public ICommand ToClipBoard_Copy
        {
            get => _toClipBoard_Copy;
            set
            {
                _toClipBoard_Copy = value;
                RaisePropertyChanged();
            }
        }

        public ICommand Delete
        {
            get => _delete;
            set
            {
                _delete = value;
                RaisePropertyChanged();
            }
        }
        public string FileName 
        {
            get => _model.FileName;
            set
            {
                if (_model.FileName != value) 
                {
                    _model.FileName = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string FilePath
        {
            get => _model.FilePath;
            set
            {
                if (_model.FilePath != value)
                {
                    _model.FilePath = value;
                    RaisePropertyChanged();
                }
            }
        }
        public virtual bool IsSelected
        {
            get => _model.IsSelected;
            set
            {
                if (value != _model.IsSelected)
                {
                    _model.IsSelected = value;
                    RaisePropertyChanged();
                    if (_model.IsSelected) { FileFolderBaseModel.SelectedItem = this; }
                }
            }
        }

        public FileFolderBaseViewModel(FileFolderBaseModel model)
        {
            _model = model;
            _fileProperties = new RelayCommand(new Action<object>(ShowProperties));
            _toClipBoard_Move = new RelayCommand(new Action<object>(CopyToClipBoard_Move));
            _toClipBoard_Copy = new RelayCommand(new Action<object>(CopyToClipBoard_Copy));
            _delete = new RelayCommand(new Action<object>(HandleDelete));
        }

        protected abstract void HandleDelete(object obj);
        private void ShowProperties(object obj)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = FilePath;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref info);
        }

        private void CopyToClipBoard_Copy(object obj)
        {
            Clipboard.SetText(FilePath);
            PreviousRightClickAction = RightClickAction.Copy;
        }

        private void CopyToClipBoard_Move(object obj)
        {
            Clipboard.SetText(FilePath);
            PreviousRightClickAction = RightClickAction.Move;
        }
    }
}
