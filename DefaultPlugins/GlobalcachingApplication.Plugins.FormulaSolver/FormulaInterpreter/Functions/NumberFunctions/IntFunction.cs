using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class IntFunction : Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            decimal d = Convert.ToDecimal(args[0]);
            return Math.Sign(d) * Math.Floor(Math.Abs(d));
        }
    }
}
