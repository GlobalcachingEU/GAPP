using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gavaghan.Geodesy;

namespace GAPPSF.Utils
{
    public class Calculus
    {
        public static GeodeticMeasurement CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            GlobalCoordinates p1 = new GlobalCoordinates(new Angle(lat1), new Angle(lon1));
            GlobalCoordinates p2 = new GlobalCoordinates(new Angle(lat2), new Angle(lon2));
            GeodeticCalculator gc = new GeodeticCalculator();
            GlobalPosition gp1 = new GlobalPosition(p1);
            GlobalPosition gp2 = new GlobalPosition(p2);
            GeodeticMeasurement gm = gc.CalculateGeodeticMeasurement(Ellipsoid.WGS84, gp1, gp2);
            return gm;
        }
        public static GeodeticMeasurement CalculateDistance(GAPPSF.Core.Data.Geocache gc, GAPPSF.Core.Data.Location loc)
        {
            return CalculateDistance(gc.Lat, gc.Lon, loc.Lat, loc.Lon);
        }

        public static void SetDistanceAndAngleGeocacheFromLocation(GAPPSF.Core.Data.Geocache gc, GAPPSF.Core.Data.Location loc)
        {
            GeodeticMeasurement gm = CalculateDistance(loc.Lat, loc.Lon, gc.Lat, gc.Lon);
            gc.DistanceToCenter = (long)gm.EllipsoidalDistance;
            gc.AngleToCenter = (int)gm.Azimuth.Degrees;
        }

        public static void SetDistanceAndAngleGeocacheFromLocation(List<GAPPSF.Core.Data.Geocache> gcList, GAPPSF.Core.Data.Location loc)
        {
            foreach (GAPPSF.Core.Data.Geocache gc in gcList)
            {
                SetDistanceAndAngleGeocacheFromLocation(gc, loc);
            }
        }

        public static GAPPSF.Core.Data.Location Convert2Location(string LatDegrees, string LatMinutes, string LonDegrees, string LonMinutes)
        {
            GAPPSF.Core.Data.Location result = null;
            try
            {
                result = new GAPPSF.Core.Data.Location(Conversion.StringToDouble(LatDegrees) + (Conversion.StringToDouble(LatMinutes) / 60.0),
                    Conversion.StringToDouble(LonDegrees) + (Conversion.StringToDouble(LonMinutes) / 60.0));
            }
            catch
            {
                result = null;
            }
            return result;
        }


        public static GAPPSF.Core.Data.Location LocationFromString(string s)
        {
            GAPPSF.Core.Data.Location result = null;
            try
            {
                s = s.ToUpper();
                string[] parts = s.Split(new char[] { ' ', 'N', 'E', 'S', 'W', '.', '°', ',', '\'' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 6 || parts.Length == 4)
                {
                    double lat;
                    double lon;
                    GAPPSF.Core.Data.Location ll = new GAPPSF.Core.Data.Location();
                    if (parts.Length == 6)
                    {
                        lat = Conversion.StringToDouble(parts[0]) + ((Conversion.StringToDouble(parts[1]) + (Conversion.StringToDouble(parts[2]) / 1000.0)) / 60.0);
                        lon = Conversion.StringToDouble(parts[3]) + ((Conversion.StringToDouble(parts[4]) + (Conversion.StringToDouble(parts[5]) / 1000.0)) / 60.0);

                        if (s.IndexOf("S", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            lat = -1.0 * lat;
                        }
                        if (s.IndexOf("W", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            lon = -1.0 * lon;
                        }
                    }
                    else
                    {
                        lat = Conversion.StringToDouble(string.Format("{0},{1}", parts[0], parts[1]));
                        lon = Conversion.StringToDouble(string.Format("{0},{1}", parts[2], parts[3]));
                    }
                    result = new GAPPSF.Core.Data.Location(lat, lon);
                }
                else if (parts.Length == 2)
                {
                    double x = Conversion.StringToDouble(parts[0]);
                    double y = Conversion.StringToDouble(parts[1]);
                    result = LocationFromRD(x, y);
                }
            }
            catch
            {
            }
            return result;
        }

        public static GAPPSF.Core.Data.Location LocationFromRD(double x, double y)
        {
            GAPPSF.Core.Data.Location result = null;
            double lattitude;
            double longitude;
            if (LatLonFromRD(x, y, out lattitude, out longitude))
            {
                result = new GAPPSF.Core.Data.Location(lattitude, longitude);
            }
            return result;
        }

        public static bool LatLonFromRD(double x, double y, out double lattitude, out double longitude)
        {
            double f0, l0, x0, y0;
            double a01, a02, a03, a04, a20, a21, a22, a23, a24, a40, a41, a42;
            double b10, b11, b12, b13, b14, b15, b30, b31, b32, b33, b50, b51;

            double dx, dy, dl, df;
            double l, f;

            if (x < 0 || x > 290000)
            {
                lattitude = 0;
                longitude = 0;
                return false;
            }
            else if (y < 290000 || y > 630000)
            {
                lattitude = 0;
                longitude = 0;
                return false;
            }

            x0 = 155000;
            y0 = 463000;
            f0 = 52.156160556;
            l0 = 5.387638889;
            a01 = 3236.0331637;
            b10 = 5261.3028966;
            a20 = -32.5915821;
            b11 = 105.9780241;
            a02 = -0.2472814;
            b12 = 2.4576469;
            a21 = -0.8501341;
            b30 = -0.8192156;
            a03 = -0.0655238;
            b31 = -0.0560092;
            a22 = -0.0171137;
            b13 = 0.0560089;
            a40 = 0.0052771;
            b32 = -0.0025614;
            a23 = -0.0003859;
            b14 = 0.001277;
            a41 = 0.0003314;
            b50 = 0.0002574;
            a04 = 0.0000371;
            b33 = -0.0000973;
            a42 = 0.0000143;
            b51 = 0.0000293;
            a24 = -0.000009;
            b15 = 0.0000291;



            dx = (x - x0) * 0.00001;
            dy = (y - y0) * 0.00001;

            df = a01 * dy + a20 * Math.Pow(dx, 2) + a02 * Math.Pow(dy, 2) + a21 * Math.Pow(dx, 2) * dy + a03 * Math.Pow(dy, 3);
            df = df + a40 * Math.Pow(dx, 4) + a22 * Math.Pow(dx, 2) * Math.Pow(dy, 2) + a04 * Math.Pow(dy, 4) + a41 * Math.Pow(dx, 4) * dy;
            df = df + a23 * Math.Pow(dx, 2) * Math.Pow(dy, 3) + a42 * Math.Pow(dx, 4) * Math.Pow(dy, 2) + a24 * Math.Pow(dx, 2) * Math.Pow(dy, 4);
            f = f0 + df / 3600; //N RD-bessel

            dl = b10 * dx + b11 * dx * dy + b30 * Math.Pow(dx, 3) + b12 * dx * Math.Pow(dy, 2) + b31 * Math.Pow(dx, 3) * dy;
            dl = dl + b13 * dx * Math.Pow(dy, 3) + b50 * Math.Pow(dx, 5) + b32 * Math.Pow(dx, 3) * Math.Pow(dy, 2) + b14 * dx * Math.Pow(dy, 4);
            dl = dl + b51 * Math.Pow(dx, 5) * dy + b33 * Math.Pow(dx, 3) * Math.Pow(dy, 3) + b15 * dx * Math.Pow(dy, 5);
            l = l0 + dl / 3600; //E RD-bessel

            lattitude = f + (-96.862 - (f - 52) * 11.714 - (l - 5) * 0.125) * 0.00001; //N WGS84
            longitude = l + ((f - 52) * 0.329 - 37.902 - (l - 5) * 14.667) * 0.00001; //E WGS84

            return true;
        }

        public static bool RDFromLatLong(double lattitude, double longitude, out double x, out double y)
        {
            double f0, l0;
            double x0, y0;
            double c01, c03, c05, c11, c13, c21, c23, c31, c41;
            double d02, d04, d10, d12, d14, d20, d22, d30, d32, d40;
            double dx, dy, dl, df;
            double l, f;

            if (lattitude < 50.579 || lattitude > 53.639)
            {
                x = 0;
                y = 0;
                return false;
            }
            else if (longitude < 3.043 || longitude > 7.429)
            {
                x = 0;
                y = 0;
                return false;
            }

            x0 = 155000;
            y0 = 463000;
            f0 = 52.15616056;
            l0 = 5.38763889;
            c01 = 190066.98903;
            d10 = 309020.3181;
            c11 = -11830.85831;
            d02 = 3638.36193;
            c21 = -114.19754;
            d12 = -157.95222;
            c03 = -32.3836;
            d20 = 72.97141;
            c31 = -2.34078;
            d30 = 59.79734;
            c13 = -0.60639;
            d22 = -6.43481;
            c23 = 0.15774;
            d04 = 0.09351;
            c41 = -0.04158;
            d32 = -0.07379;
            c05 = -0.00661;
            d14 = -0.05419;
            d40 = -0.03444;


            f = lattitude - (-96.862 - (lattitude - 52) * 11.714 - (longitude - 5) * 0.125) * 0.00001; //N bessel
            l = longitude - ((lattitude - 52) * 0.329 - 37.902 - (longitude - 5) * 14.667) * 0.00001; //E bessel

            df = (f - f0) * 0.36;
            dl = (l - l0) * 0.36;

            dx = c01 * dl + c11 * df * dl + c21 * Math.Pow(df, 2) * dl + c03 * Math.Pow(dl, 3);
            dx = dx + c31 * Math.Pow(df, 3) * dl + c13 * df * Math.Pow(dl, 3) + c23 * Math.Pow(df, 2) * Math.Pow(dl, 3);
            dx = dx + c41 * Math.Pow(df, 4) * dl + c05 * Math.Pow(dl, 5);
            x = (int)Math.Round(x0 + dx); //RD x


            dy = d10 * df + d20 * Math.Pow(df, 2) + d02 * Math.Pow(dl, 2) + d12 * df * Math.Pow(dl, 2);
            dy = dy + d30 * Math.Pow(df, 3) + d22 * Math.Pow(df, 2) * Math.Pow(dl, 2) + d40 * Math.Pow(df, 4);
            dy = dy + d04 * d14 + d32 * Math.Pow(df, 3) * Math.Pow(dl, 2) + d14 * df * Math.Pow(dl, 4);
            y = (int)Math.Round(y0 + dy); //RD y

            return true;
        }


        public static bool PointInPolygon(List<GAPPSF.Core.Data.Location> points, GAPPSF.Core.Data.Location ll)
        {
            return PointInPolygon(points, ll.Lat, ll.Lon);
        }
        public static bool PointInPolygon(List<GAPPSF.Core.Data.Location> points, double lat, double lon)
        {
            bool result = false;

            double MULTIPLEFACTOR = 100000.0;
            double EPSSHIFT = 0.000001;
            double EPS = 0.000001;

            double r;
            double l;
            double t;
            double b;
            double sect;
            double q;

            double py = lat;
            double px = lon;

            px = (int)(MULTIPLEFACTOR * px) / MULTIPLEFACTOR + EPSSHIFT;
            py = (int)(MULTIPLEFACTOR * py) / MULTIPLEFACTOR + EPSSHIFT;

            double Ax;
            double Ay;
            double Bx;
            double By;

            int parity1 = 0;
            int parity2 = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Ay = points[i].Lat;
                Ax = points[i].Lon;
                By = points[i + 1].Lat;
                Bx = points[i + 1].Lon;

                Ay = (int)(MULTIPLEFACTOR * Ay) / MULTIPLEFACTOR;
                Ax = (int)(MULTIPLEFACTOR * Ax) / MULTIPLEFACTOR;
                By = (int)(MULTIPLEFACTOR * By) / MULTIPLEFACTOR;
                Bx = (int)(MULTIPLEFACTOR * Bx) / MULTIPLEFACTOR;

                l = r = Ax;
                b = t = Ay;
                if (Bx < l)
                    l = Bx;
                else
                    r = Bx;
                if (By < b)
                    b = By;
                else
                    t = By;

                if ((b > py) || (t < py))
                    continue; // toto je dooooost divne

                q = (py - Ay) / (By - Ay);

                sect = Ax + q * (Bx - Ax);

                if (System.Math.Abs(px - sect) < EPS)
                {
                    // point is on the boundary
                    // preco sa overuje iba - ova suradnica?
                    return true;
                }

                if (sect < px)
                    parity1++;
                else
                    parity2++;

            }

            parity1 = parity1 % 2;
            parity2 = parity2 % 2;

            result = (parity1 != 0);

            return result;
        }

        public static GAPPSF.Core.Data.CompassDirection GetCompassDirectionFromAngle(int angle)
        {
            GAPPSF.Core.Data.CompassDirection result = GAPPSF.Core.Data.CompassDirection.N;
            double deg = angle;
            if (deg < 0)
            {
                deg += 360;
            }
            if (deg >= (360 - 22.5) || deg <= 22.5) result = GAPPSF.Core.Data.CompassDirection.N;
            else if (deg >= (45 - 22.5) && deg <= (45 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.NE;
            else if (deg >= (90 - 22.5) && deg <= (90 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.E;
            else if (deg >= (135 - 22.5) && deg <= (135 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.SE;
            else if (deg >= (180 - 22.5) && deg <= (180 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.S;
            else if (deg >= (225 - 22.5) && deg <= (225 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.SW;
            else if (deg >= (270 - 22.5) && deg <= (270 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.W;
            else if (deg >= (315 - 22.5) && deg <= (315 + 22.5)) result = GAPPSF.Core.Data.CompassDirection.NW;
            return result;
        }

    }
}
