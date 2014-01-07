using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
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
