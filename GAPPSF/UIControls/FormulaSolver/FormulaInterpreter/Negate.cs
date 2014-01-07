using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class Negate : Expression
    {
        private readonly Expression computable;

        [Rule("<Negate Exp> ::= ~'-' <Value>")]
        public Negate(Expression computable)
        {
            this.computable = computable;
        }

        public override object GetValue(ExecutionContext ctx)
        {
            return -(Convert.ToDecimal(computable.GetValue(ctx)));
        }
    }
}
