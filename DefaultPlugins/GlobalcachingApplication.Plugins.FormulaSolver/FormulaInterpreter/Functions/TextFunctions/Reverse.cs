using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.TextFunctions
{
    public class Reverse: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            string res = "";
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 1))
            {
                res = new string(args[0].ToString().ToCharArray().Reverse().ToArray());
            }
            return res;
        }
    }
}
