using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    public class BinaryOperation : Expression
    {
        private readonly Expression _left;
        private readonly BinaryOperator _op;
        private readonly Expression _right;

        [Rule("<Expression>  ::= <Expression> '+' <Mult Exp>")]
        [Rule("<Expression>  ::= <Expression> '-' <Mult Exp>")]
        [Rule("<Mult Exp>    ::= <Mult Exp> '*' <Negate Exp>")]
        [Rule("<Mult Exp>    ::= <Mult Exp> '/' <Negate Exp>")]
        [Rule("<Mult Exp>    ::= <Mult Exp> '%' <Negate Exp>")]
        public BinaryOperation(Expression left, BinaryOperator op, Expression right)
        {
            _left = left;
            _op = op;
            _right = right;
        }

        public override object GetValue(ExecutionContext ctx)
        {
            object lStart = _left.GetValue(ctx);
            object rStart = _right.GetValue(ctx);

            object lFinal;
            object rFinal;

            TypeChecker.GreatestCommonType gct = TypeChecker.GCT(lStart, rStart);
            switch (gct)
            {
                case TypeChecker.GreatestCommonType.StringType:
                    lFinal = Convert.ToString(lStart);
                    rFinal = Convert.ToString(rStart);
                    break;
                case TypeChecker.GreatestCommonType.BooleanType:
                    lFinal = Convert.ToBoolean(lStart);
                    rFinal = Convert.ToBoolean(rStart);
                    break;
                case TypeChecker.GreatestCommonType.NumericType:
                    lFinal = Convert.ToDecimal(lStart);
                    rFinal = Convert.ToDecimal(rStart);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // execute on the converted values
            return _op.Evaluate(lFinal, rFinal);
        }
    }
}
