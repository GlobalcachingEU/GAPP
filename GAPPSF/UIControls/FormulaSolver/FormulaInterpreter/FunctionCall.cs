using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;
using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions;
using System.Collections;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class FunctionCall: Expression
    {
        private readonly Expression function;
        private readonly GCToken args;
        private readonly bool emptyArgsList;

        [Rule("<Value> ::= <FunctionToken> ~'(' <OptionalArgumentList> ~')'")]
        public FunctionCall(Expression function, GCToken args)
        {
            this.function = function;
            this.args = args;
            this.emptyArgsList = (args.GetType() == typeof(Optional<Sequence<Expression>>));
        }

        public override object GetValue(ExecutionContext ctx)
        {
            string functionName = function.GetValue(ctx).ToString();
            Functor f = ctx.GetFunction(functionName);
            ArrayList arguments = new ArrayList();
            if (!emptyArgsList)
            {
                if (args.GetType() == typeof(Sequence<Expression>))
                {
                    Sequence<Expression> seq = (Sequence<Expression>)args;

                    foreach (Expression ex in (Sequence<Expression>)args)
                    {
                        arguments.Add(ex.GetValue(ctx));
                    }
                }
                else
                {
                    arguments.Add(((Expression)args).GetValue(ctx));
                }
            }
            return f.Execute(arguments.ToArray(), ctx);
        }
    }
}
