using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.ContextFunctions
{
    public class SetContext: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, 1);

            string arg = args[0].ToString();

            if (String.Equals(arg.Substring(0, 3), "Deg", StringComparison.OrdinalIgnoreCase))
            {
                ctx.TrigonometricMeasure = ExecutionContext.TrigonometricBase.Degree;
            }
            else if (String.Equals(arg.Substring(0, 3), "Rad", StringComparison.OrdinalIgnoreCase))
            {
                ctx.TrigonometricMeasure = ExecutionContext.TrigonometricBase.Radians;
            }
            else if (String.Equals(arg.Substring(0, 4), "Grad", StringComparison.OrdinalIgnoreCase))
            {
                ctx.TrigonometricMeasure = ExecutionContext.TrigonometricBase.Grad;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            };

            return "";
        }
    }
}
