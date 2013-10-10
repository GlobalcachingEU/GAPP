using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Intersection: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            string ret = StrRes.GetString(StrRes.STR_NO_CROSSING);
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 4))
            {
                Framework.Data.Location ll0 = Utils.Conversion.StringToLocation(args[0].ToString());
                Framework.Data.Location ll1 = Utils.Conversion.StringToLocation(args[1].ToString());
                Framework.Data.Location ll2 = Utils.Conversion.StringToLocation(args[2].ToString());
                Framework.Data.Location ll3 = Utils.Conversion.StringToLocation(args[3].ToString());

                // x -> Lon , y -> Lat
                decimal y0 = (decimal)ll0.Lat;
                decimal y1 = (decimal)ll1.Lat;
                decimal y2 = (decimal)ll2.Lat;
                decimal y3 = (decimal)ll3.Lat;

                decimal x0 = (decimal)ll0.Lon;
                decimal x1 = (decimal)ll1.Lon;
                decimal x2 = (decimal)ll2.Lon;
                decimal x3 = (decimal)ll3.Lon;

                decimal? m0 = null; // Gerade 1 senkrecht -> alle x-Werte der Geraden sind gleich
                decimal? m1 = null; // Gerade 2 senkrecht -> alle x-Werte der Geraden sind gleich
                decimal? n0 = null; 
                decimal? n1 = null;

                if (x1 != x0)
                {
                    m0 = (y1 - y0) / (x1 - x0);
                }

                if (x3 != x2)
                {
                    m1 = (y3 - y2) / (x3 - x2);
                }

                if (m0 != m1) 
                {
                    decimal? lon = null;
                    decimal? lat = null;
                    if (m0 == null)
                    {
                        n1 = y2 - x2 * m1;
                        lon = x0;
                        lat = m1 * x0 + n1;
                    }
                    else if (m1 == null)
                    {
                        n0 = y0 - x0 * m0;
                        lon = x2;
                        lat = m0 * x2 + n0;
                    }
                    else
                    {
                        n0 = y0 - x0 * m0;
                        n1 = y2 - x2 * m1;

                        lon = (n1 - n0) / (m0 - m1); // x
                        lat = lon * m0 + n0; // y
                    }
                    ret = Utils.Conversion.GetCoordinatesPresentation((double)lat, (double)lon);
                }
            }
            return ret;
        }
    }
}

/*

Intersection("N 51 10.824 E 010 29.287"; "N 51 11.075 E 010 30.968"; "N 51 11.456 E 010 29.799"; "N 51 10.635 E 010 30.690")

Intersection("N 51 10.824 E 010 29.287"; "N 51 11.075 E 010 30.968"; "N 51 11.456 E 010 29.799"; "N 51 10.567 E 010 29.799")

Intersection("N 51 10.824 E 010 29.287"; "N 51 10.824 E 010 30.968"; "N 51 11.456 E 010 29.799"; "N 51 10.635 E 010 30.690")

*/