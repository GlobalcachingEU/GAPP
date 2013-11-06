using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.TrigonometricFunctions
{
    public class TrigonometricFunction: Functor
    {
        protected virtual object ExecuteTrigFunction(double arg) {
            return "";
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

            if (ctx.TrigonometricMeasure == ExecutionContext.TrigonometricBase.Degree)
            {
                arg = arg * Math.PI / 180;
            }
            else if (ctx.TrigonometricMeasure == ExecutionContext.TrigonometricBase.Grad)
            {
                arg = arg * Math.PI / 200;
            }

            return ExecuteTrigFunction(arg);
        }
    }
}
