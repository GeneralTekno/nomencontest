using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Nomencontest.Base
{
    public class LocalFileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = parameter as string;
            if (String.IsNullOrWhiteSpace(str)) return "";

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, str)))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, str);
            }
            else return Path.Combine("pack://application:,,,/Nomencontest;component/", str);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToUpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? string.Empty : str.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsEvenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((float.Parse(value.ToString()) % 2).Equals(0));
            }
            catch
            {
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    [ValueConversion(typeof(double), typeof(double))]
    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            double v1 = 0;
            double v2 = 0;

            if
                (
                Double.TryParse(value.ToString(), out v1) &&
                Double.TryParse(parameter.ToString(), out v2)
                )
                return v1 > v2;

            return false;
        }

        public object ConvertBack
            (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(double), typeof(double))]
    public class IsEqualConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value.Length != 2) return false;

            return (value[0].Equals(value[1]));
        }

        public object[] ConvertBack
            (object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
