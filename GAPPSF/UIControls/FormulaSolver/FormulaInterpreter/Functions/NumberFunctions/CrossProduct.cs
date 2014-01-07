using System;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class CrossProduct: Functor
    {
        private int Evaluate(int iNumber)
        {
            return iNumber % 10 * (iNumber >= 10 ? Evaluate(iNumber / 10) : 1);
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, null);
            return Evaluate(Convert.ToInt32(args[0]));
        }
    }
}
