using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class IteratedCrossTotal: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            Functor f = ctx.GetFunction("ct");
            int res = Convert.ToInt32(f.Execute(args, ctx));
            while (res > 9)
            {
                res = Convert.ToInt32(f.Execute(new object[] { res }, ctx));
            }
            return res;
        }
    }
}
