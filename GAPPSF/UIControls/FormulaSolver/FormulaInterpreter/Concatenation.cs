using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class Concatenation: Expression
    {
        private readonly ConcatElement element;
        private readonly Expression concatenation;

        [Rule("<Value> ::= <Concatenation>")]
        public Concatenation(Expression concatenation)
        {
            this.concatenation = concatenation;
        }

        [Rule("<Concatenation> ::= <ConcatElement> <Concatenation>")]
        public Concatenation(ConcatElement element, Expression concatenation)
        {
            this.element = element;
            this.concatenation = concatenation;
        }

        public override object GetValue(ExecutionContext ctx)
        {
            string res = "";
            if (element != null)
            {
                res += Convert.ToString(element.GetValue(ctx));
            }
            if (concatenation != null)
            {
                res += Convert.ToString(concatenation.GetValue(ctx));
            }
            return res;
        }
    }
}
