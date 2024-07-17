using RenameTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace RenameTool.AssistClasses
{
    internal class SelectedItem2String : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var SelectedItem = (value as ViewItem);
            if (SelectedItem != null)
            {
                string content = (SelectedItem.IsFile) ? "File info:" : "Folder info:";
                content += "\n  Location: " + SelectedItem.Location;
                content += "\n  Name: " + SelectedItem.FullName;
                content += "\n  New name: " + SelectedItem.NewName;
                return (content);
            }
            return "No info!";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as ViewItem);
        }
    }
}
