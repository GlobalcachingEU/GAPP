using System;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class CrossProduct: Functor
    {
        private int Evaluate(int iNumber)
        {
            return iNumber % 10 * (iNumber > 10 ? Evaluate(iNumber / 10) : 1);
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            return (checker.CheckForMinimumArguments(ref args, 1))
                ? Evaluate(Convert.ToInt32(args[0]))
                : 0;
        }
    }
}
