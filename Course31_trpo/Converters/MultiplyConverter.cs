using System.Globalization;

namespace Course31_trpo.Converters
{
    public class MultiplyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) { return value; }
            string par = (parameter?.ToString() ?? "").Replace('.', ',');
            if (value is double val && double.TryParse(par, out double multiplier)) { return val * multiplier; }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) { return value; }
            string par = (parameter?.ToString() ?? "").Replace('.', ',');
            if (value is double val && double.TryParse(par, out double divisor) && divisor != 0) { return val / divisor; }
            return value;
        }
    }
}
