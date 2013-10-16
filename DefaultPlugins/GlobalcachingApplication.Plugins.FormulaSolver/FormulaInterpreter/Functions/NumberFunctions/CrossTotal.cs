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
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            return Evaluate(Convert.ToUInt64(args[0]));
        }
    }
}
