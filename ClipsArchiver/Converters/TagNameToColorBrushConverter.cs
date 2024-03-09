using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ClipsArchiver.Constants;

namespace ClipsArchiver.Converters;

public class TagNameToColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var tagName = value as string;
        int code = 0;
        foreach (var b in Encoding.UTF8.GetBytes(tagName))
        {
            code += b;
        }
        string colour = AccentColours.HexColours[code % AccentColours.HexColours.Length];
        SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFrom(colour);
        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}