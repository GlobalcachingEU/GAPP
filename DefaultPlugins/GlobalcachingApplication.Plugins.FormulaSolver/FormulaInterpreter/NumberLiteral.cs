using System;
using bsn.GoldParser.Semantic;
using System.Globalization;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("DecLiteral")]
    [Terminal("FloatLiteral")]
    public class NumberLiteral : Expression
    {
        private readonly decimal _value;

        public NumberLiteral(string value)
        {
            _value = Convert.ToDecimal(value.Replace(',', '.'), new CultureInfo(""));
        }

        public override object GetValue(ExecutionContext ctx)
        {
            return _value;
        }
    }
}
