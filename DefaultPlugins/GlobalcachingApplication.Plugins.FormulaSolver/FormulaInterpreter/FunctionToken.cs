using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("Function")]
    public class FunctionToken: Expression
    {
        private readonly string functionName;

        public FunctionToken(string name)
        {
            functionName = name.ToLower();
        }

        public override object GetValue(ExecutionContext ctx)
        {
            return functionName;
        }
    }
}
