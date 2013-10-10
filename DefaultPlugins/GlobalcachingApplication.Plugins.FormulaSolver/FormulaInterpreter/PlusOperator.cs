using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("+")]
    public class PlusOperator : BinaryOperator
    {
        public override object Evaluate(object left, object right)
        {
            string _left = left.ToString();
            string _right = right.ToString();
            if ((_left.Length == 0) || (_right.Length == 0))
            {
                return "";
            }
            else
            {
                return Utils.Conversion.StringToDouble(_left) + Utils.Conversion.StringToDouble(_right);
            }
        }
    }
}
