using bsn.GoldParser.Semantic;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class AssignStatement : Expression
    {
        private readonly Expression _expr;
        private readonly Identifier _receiver;

        [Rule(@"<Assignment> ::= Identifier ~'=' ")]
        [Rule(@"<Assignment> ::= Identifier ~'=' <Expression>")]
        public AssignStatement(Identifier receiver, Expression expr)
        {
            _receiver = receiver;
            _expr = expr;
        }

        public override object GetValue(ExecutionContext ctx)
        {
            ctx[_receiver._idName] = (_expr != null) ? _expr.GetValue(ctx) : "";
            return string.Format("{0}={1}", _receiver._idName, ctx[_receiver._idName]);
        }
    }
}
