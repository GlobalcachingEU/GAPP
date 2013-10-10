using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    public class ConcatElement: Expression
    {
        private readonly StringLiteral text;
        private Sequence<Expression> expressionList;

        [Rule("<ConcatElement> ::= StringLiteral")]
        public ConcatElement(StringLiteral text) : this(text, null) { }

        [Rule("<ConcatElement> ::= StringLiteral <ExpressionList>")]
        public ConcatElement(StringLiteral text, Sequence<Expression> expressionList)
        {
            this.text = text;
            this.expressionList = expressionList;
        }

        public override object GetValue(ExecutionContext ctx)
        {
            string res = "";

            if (text != null)
            {
                res += text.GetValue(ctx);
            }
            if (expressionList != null)
            {
                foreach (Expression ex in expressionList)
                {
                    res += ex.GetValue(ctx);
                }
            }

            return res;
        }
    }
}
