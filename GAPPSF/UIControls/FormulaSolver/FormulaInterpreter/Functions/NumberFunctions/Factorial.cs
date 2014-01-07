using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    class Factorial: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            UInt64 res = 1;

            ArgumentChecker checker = new ArgumentChecker(this.GetType().Name);
            checker.CheckForNumberOfArguments(ref args, 1, 1);
            UInt64? value = checker.GetRangedUInt64(ref args[0], 0, 50);
            if (value != null)
            {
                for (UInt64 i = 2; i < value; ++i)
                {
                    res *= i;
                }
            }
            return res;
        }
    }
}
