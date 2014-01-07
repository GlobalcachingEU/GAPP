using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GAPPSF.Core.Data;
using System.Globalization;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Latitude: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, 1);
            Core.Data.Location ll = Utils.Conversion.StringToLocation(args[0].ToString());
            if (ll != null)
            {
                return ll.Lat.ToString("G", CultureInfo.InvariantCulture);
            }
            return "";
        }
    }
}
