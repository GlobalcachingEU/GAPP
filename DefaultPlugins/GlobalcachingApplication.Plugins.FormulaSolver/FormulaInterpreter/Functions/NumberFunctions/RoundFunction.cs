using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    class RoundFunction: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 1))
            {
                decimal value = Convert.ToDecimal(args[0]);
                int decimals = (args.Length > 1) ? Convert.ToInt32(args[1]) : 0;
                return Math.Round(value, decimals);
            }
            return "";
        }
    }
}
