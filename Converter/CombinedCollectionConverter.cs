using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using File_Explorer.ViewModel;

namespace File_Explorer.Converter
{
    class CombinedCollectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var folders = values[0] as ObservableCollection<FileFolderBaseViewModel>;
            var files = values[1] as ObservableCollection<FileFolderBaseViewModel>;

            if(folders != null && files != null)
            {
                return new ObservableCollection<FileFolderBaseViewModel>(folders.Concat(files));
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
