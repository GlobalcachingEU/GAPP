using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TrigonometricFunctions
{
    public class TanH : TrigonometricFunction
    {
        protected override object ExecuteTrigFunction(double arg)
        {
            return Math.Tanh(arg);
        }
    }
}
