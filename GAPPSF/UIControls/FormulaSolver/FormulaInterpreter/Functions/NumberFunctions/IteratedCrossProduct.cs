using System;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions.NumberFunctions
{
    public class IteratedCrossProduct: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            Functor f = ctx.GetFunction("cp");
            int res = Convert.ToInt32(f.Execute(args, ctx));
            while (res > 9)
            {
                res = Convert.ToInt32(f.Execute(new object[] { res }, ctx));
            }
            return res;
        }
    }    
}
