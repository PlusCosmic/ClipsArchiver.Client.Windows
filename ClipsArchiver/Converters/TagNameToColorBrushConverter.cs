using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ClipsArchiver.Constants;

namespace ClipsArchiver.Converters;

public class TagNameToColorBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var tagName = value as string;
        
        if (tagName == null)
        {
            return new SolidColorBrush(Colors.White);
        }
        
        int code = 0;
        foreach (var b in Encoding.UTF8.GetBytes(tagName))
        {
            code += b;
        }
        
        string colour = AccentColours.HexColours[code % AccentColours.HexColours.Length];
        var converter = new BrushConverter().ConvertFrom(colour);
        
        if (converter == null)
        {
            return new SolidColorBrush(Colors.White);
        }
        
        SolidColorBrush brush = (SolidColorBrush)converter;
        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}