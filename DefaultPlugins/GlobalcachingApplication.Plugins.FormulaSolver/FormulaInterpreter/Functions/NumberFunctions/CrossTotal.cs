using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class CrossTotal: Functor
    {
        private UInt64 Evaluate(UInt64 iNumber)
        {
            return iNumber % 10 + (iNumber > 10 ? Evaluate(iNumber / 10) : 0);
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            return (checker.CheckForMinimumArguments(ref args, 1))
                ? Evaluate(Convert.ToUInt64(args[0]))
                : 0;
        }
    }
}
