using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TrigonometricFunctions
{
    public class ATan : InverseTrigonometricFunction
    {
        protected override double ExecuteTrigFunction(double arg)
        {
            double res = Math.Atan(arg);
            if (Double.IsNaN(res))
            {
                throw new ArgumentRangeException(String.Format(StrRes.GetString(StrRes.STR_BAD_PARAMETER_VALUE), GetType().Name));
            }
            return res;
        }
    }
}
