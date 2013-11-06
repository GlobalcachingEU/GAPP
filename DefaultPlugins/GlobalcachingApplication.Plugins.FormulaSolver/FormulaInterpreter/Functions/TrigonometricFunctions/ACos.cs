using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.TrigonometricFunctions
{
    public class ACos : InverseTrigonometricFunction
    {
        protected override double ExecuteTrigFunction(double arg)
        {
            return Math.Acos(arg);
        }
    }
}
