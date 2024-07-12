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
    class TextFormatting2String : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (TextFormattings)value;
            switch (val)
            {
                case TextFormattings.None:
                    return "Keep original";
                case TextFormattings.LowerCase:
                    return "lower case";
                case TextFormattings.UpperCase:
                    return "UPPER CASE";
                case TextFormattings.TitleCase:
                    return "Title Case";
                default:
                    return "Keep original";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value.ToString();
            if (val == "lower case") { return TextFormattings.LowerCase; }
            else if (val =="UPPER CASE") { return TextFormattings.UpperCase; }
            else if(val =="Title Case") { return TextFormattings.TitleCase; }
            else if (val == "Keep original") { return TextFormattings.None; }
            return TextFormattings.None;
        }
    }
}
