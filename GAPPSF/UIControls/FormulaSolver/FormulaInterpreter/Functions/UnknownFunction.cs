using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions
{
    public class UnknownFunction: Functor
    {
        private readonly string functionName;

        public UnknownFunction(string name)
        {
            functionName = name;
        }

        public override object Execute(object[] args, ExecutionContext ctx)
        {
            return StrRes.GetString(StrRes.STR_UNKNOWN_FUNCTION) + functionName;
        }
    }
}
