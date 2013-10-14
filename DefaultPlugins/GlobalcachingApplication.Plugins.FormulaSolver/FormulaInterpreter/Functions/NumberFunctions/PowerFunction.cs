using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    class PowerFunction: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 2))
            {
                double b = Convert.ToDouble(args[0]);
                double ex = Convert.ToDouble(args[1]);

                if ((b == 0) && (ex == -1))
                {
                    throw new ArgumentRangeException(StrRes.GetString(StrRes.STR_DIV_BY_ZERO));                    
                }

                if (ex == 0)
                {
                    return 1;
                }

                return Math.Pow(b, ex);
            }
            return "";
        }
    }
}
