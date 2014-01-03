using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GAPPSF.UIControls
{
    public class PathConverter : IValueConverter
    {
        public PathConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GAPPSF.Core.Data.GeocacheType gct = value as GAPPSF.Core.Data.GeocacheType;
            if (gct != null)
            {
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/CacheTypes/{0}.gif", gct.ID.ToString()));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ContainerConverter : IValueConverter
    {
        public ContainerConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GAPPSF.Core.Data.GeocacheContainer gct = value as GAPPSF.Core.Data.GeocacheContainer;
            if (gct != null)
            {
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Container/{0}.gif", gct.Name.Replace(' ', '_')));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class LogTypeConverter : IValueConverter
    {
        public LogTypeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GAPPSF.Core.Data.LogType gct = value as GAPPSF.Core.Data.LogType;
            if (gct != null)
            {
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/LogTypes/{0}.gif", gct.ID));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GeocacheAttributeConverter : IValueConverter
    {
        public GeocacheAttributeConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GAPPSF.Core.Data.GeocacheAttribute gct = value as GAPPSF.Core.Data.GeocacheAttribute;
            if (gct != null)
            {
                if (Math.Abs(gct.ID) < 100)
                {
                    return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Attributes/{0}.gif", gct.ID.ToString().Replace('-', '_')));
                }
                else
                {
                    return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Attributes/{0}.png", gct.ID.ToString().Replace('-', '_')));
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CompassPathConverter : IValueConverter
    {
        public CompassPathConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Compass/{0}.gif", Utils.Calculus.GetCompassDirectionFromAngle((int)value).ToString()));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }


    public class GeocacheCoordConverter : IValueConverter
    {
        public GeocacheCoordConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GAPPSF.Core.Data.Geocache gc = value as GAPPSF.Core.Data.Geocache;
            if (gc != null)
            {
                if (gc.ContainsCustomLatLon)
                {
                    return Utils.Conversion.GetCoordinatesPresentation((double)gc.CustomLat, (double)gc.CustomLon);
                }
                else
                {
                    return Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

}
