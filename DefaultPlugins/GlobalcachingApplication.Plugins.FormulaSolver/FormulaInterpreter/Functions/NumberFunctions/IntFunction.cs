using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class IntFunction : Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 1))
            {
                decimal d = Convert.ToDecimal(args[0]);
                return Math.Sign(d) * Math.Floor(Math.Abs(d));
            }
            return 0;
        }
    }
}
