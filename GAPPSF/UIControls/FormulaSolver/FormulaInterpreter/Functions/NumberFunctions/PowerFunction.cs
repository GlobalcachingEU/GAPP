using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    class PowerFunction: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 2, 2);
            double b = Convert.ToDouble(args[0]);
            double ex = Convert.ToDouble(args[1]);

            if ((b == 0) && (ex == -1))
            {
                throw new ArgumentRangeException(StrRes.GetString(StrRes.STR_DIV_BY_ZERO));                    
            }

            return Math.Pow(b, ex);
        }
    }
}
