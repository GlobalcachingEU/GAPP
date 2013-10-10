using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("*")]
    public class MultiplyOperator : BinaryOperator
    {
        public override object Evaluate(object left, object right)
        {
            return Convert.ToDecimal(left) * Convert.ToDecimal(right);
        }
    }
}
