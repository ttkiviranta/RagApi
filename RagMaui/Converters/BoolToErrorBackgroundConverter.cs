using System.Globalization;

namespace RagMaui.Converters
{
    /// <summary>
    /// Converter that transforms a boolean value to an error color
    /// </summary>
    public class BoolToErrorBackgroundConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a color
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isError && isError)
            {
                return Colors.LightPink;
            }
            return Colors.White;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}