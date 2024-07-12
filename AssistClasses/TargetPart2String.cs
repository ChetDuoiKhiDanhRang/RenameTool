using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RenameTool.AssistClasses
{
    class TargetPart2String : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (NameOrExtension)value;
            switch (val)
            {
                case NameOrExtension.NameOnly:
                    return "Name";
                case NameOrExtension.ExtensionOnly:
                    return "Extension";
                case NameOrExtension.NameAndExtension:
                    return "Name + Extension";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            if (val != null || val =="Name") { return NameOrExtension.NameOnly; }
            else if (val == "Extension") { return NameOrExtension.ExtensionOnly; }
            else if (val == "Name + Extension") { return NameOrExtension.NameAndExtension; }
            return NameOrExtension.NameOnly;
        }
    }
}
