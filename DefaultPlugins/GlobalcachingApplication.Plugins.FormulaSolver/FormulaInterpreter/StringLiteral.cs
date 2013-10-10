using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("StringLiteral")]
    public class StringLiteral : Expression
    {
        private readonly string _value;

        public StringLiteral(string value)
        {
            _value = value.Substring(1, value.Length - 2);
        }

        public override object GetValue(ExecutionContext ctx)
        {
            return _value;
        }
    }
}
