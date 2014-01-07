using System;
using Gavaghan.Geodesy;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Projection: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            string ret = "";
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 3, null);
            Core.Data.Location ll = Utils.Conversion.StringToLocation(args[0].ToString());
            double distance = Utils.Conversion.StringToDouble(args[1].ToString());
            double angle = Utils.Conversion.StringToDouble(args[2].ToString());

            if (ll != null)
            {
                GeodeticCalculator gc = new GeodeticCalculator();
                GlobalCoordinates p = gc.CalculateEndingGlobalCoordinates(Ellipsoid.WGS84, 
                    new GlobalCoordinates(new Angle(ll.Lat), new Angle(ll.Lon)), 
                    new Angle(angle), distance);
                ret = Utils.Conversion.GetCoordinatesPresentation(p.Latitude.Degrees, p.Longitude.Degrees);
            }

            return ret;
        }
    }
}
