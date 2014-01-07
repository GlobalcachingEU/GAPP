using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TrigonometricFunctions
{
    public class ACos : InverseTrigonometricFunction
    {
        protected override double ExecuteTrigFunction(double arg)
        {
            double res = Math.Acos(arg);
            if (Double.IsNaN(res))
            {
                throw new ArgumentRangeException(String.Format(StrRes.GetString(StrRes.STR_VALUE_OUT_OF_RANGE), GetType().Name, -1, 1));
            }
            return res;
        }
    }
}
