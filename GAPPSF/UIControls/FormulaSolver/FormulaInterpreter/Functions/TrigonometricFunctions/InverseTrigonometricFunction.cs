using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.TrigonometricFunctions
{
    public class InverseTrigonometricFunction: Functor
    {
        protected virtual double ExecuteTrigFunction(double arg)
        {
            return 0;
        }

        protected virtual string FunctionName()
        {
            return this.GetType().Name;
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(FunctionName());
            checker.CheckForNumberOfArguments(ref args, 1, 1);
            double arg = Convert.ToDouble(args[0].ToString(), CultureInfo.InvariantCulture);
            double res = ExecuteTrigFunction(arg);

            if (ctx.TrigonometricMeasure == ExecutionContext.TrigonometricBase.Degree)
            {
                res = res * 180 / Math.PI;
            }
            else if (ctx.TrigonometricMeasure == ExecutionContext.TrigonometricBase.Grad)
            {
                res = res * 200 / Math.PI;
            }

            return res;
        }
    }
}
