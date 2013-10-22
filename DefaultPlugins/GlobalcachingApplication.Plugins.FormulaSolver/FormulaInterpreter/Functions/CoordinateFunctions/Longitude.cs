using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Data;
using System.Globalization;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Longitude : Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, 1);
            Framework.Data.Location ll = Utils.Conversion.StringToLocation(args[0].ToString());
            if (ll != null)
            {
                return ll.Lon.ToString("G", CultureInfo.InvariantCulture);
            }
            return "";
        }
    }
}
