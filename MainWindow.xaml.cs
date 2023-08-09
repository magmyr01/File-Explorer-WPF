using System.Windows;
using System.Windows.Controls;
using File_Explorer.ViewModel;

namespace File_Explorer
{
    public partial class MainWindow : Window
    {
        FileExplorerViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new FileExplorerViewModel();
            DataContext = _viewModel;
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(sender is TreeViewItem tvi)
            {
                tvi.Focus();
            }
        }
    }
}
