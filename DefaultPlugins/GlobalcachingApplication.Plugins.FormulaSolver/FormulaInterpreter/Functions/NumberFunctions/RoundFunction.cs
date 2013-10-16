using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    class RoundFunction: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            decimal value = Convert.ToDecimal(args[0]);
            int decimals = (args.Length > 1) ? Convert.ToInt32(args[1]) : 0;
            return Math.Round(value, decimals);
        }
    }
}
