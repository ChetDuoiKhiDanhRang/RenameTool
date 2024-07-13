using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;

namespace RenameTool.AssistClasses
{
    class Bool2Image : IValueConverter
    {
        ImageSource fileIcon = new BitmapImage(new Uri(@"pack://application:,,,/RenameTool;component/imgs/file.png"));
        ImageSource folderIcon = new BitmapImage(new Uri(@"pack://application:,,,/RenameTool;component/imgs/folder.png"));
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool)value;
            if (val)
            {
                return fileIcon;
            }
            return folderIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
